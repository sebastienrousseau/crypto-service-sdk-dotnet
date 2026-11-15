// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CryptoService.Sdk.Tests;

/// <summary>
/// The kit's template language (conformance/README.md): exact values,
/// same-length arrays, objects with exactly the template's members, and
/// <c>$shape</c> JSON Schemas; <c>${name}</c> substitution and JSON pointer
/// captures.
/// </summary>
internal static class Template
{
    /// <summary>Every place <paramref name="actual"/> departs from <paramref name="template"/>.</summary>
    internal static List<string> Mismatches(JsonElement template, JsonElement actual, IReadOnlyDictionary<string, string> captured)
    {
        var found = new List<string>();
        Match(template, actual, captured, "", found);
        return found;
    }

    /// <summary>Replaces every <c>${name}</c> in every string of <paramref name="value"/>.</summary>
    internal static JsonNode? Substitute(JsonElement value, IReadOnlyDictionary<string, string> captured) =>
        value.ValueKind switch
        {
            JsonValueKind.Object => new JsonObject(value.EnumerateObject()
                .Select(p => KeyValuePair.Create(p.Name, Substitute(p.Value, captured)))),
            JsonValueKind.Array => new JsonArray([.. value.EnumerateArray().Select(e => Substitute(e, captured))]),
            JsonValueKind.String => JsonValue.Create(Fill(value.GetString()!, captured)),
            _ => JsonNode.Parse(value.GetRawText()),
        };

    /// <summary>The value at a JSON pointer, as the kit keeps captures: a string.</summary>
    internal static string Pointer(JsonElement document, string pointer)
    {
        JsonElement at = document;
        foreach (string raw in pointer.Split('/').Skip(1))
        {
            string token = raw.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
            at = at.ValueKind == JsonValueKind.Array
                ? at[int.Parse(token, CultureInfo.InvariantCulture)]
                : at.GetProperty(token);
        }

        return at.ValueKind == JsonValueKind.String ? at.GetString()! : at.GetRawText();
    }

    private static string Fill(string text, IReadOnlyDictionary<string, string> captured) =>
        Regex.Replace(text, @"\$\{([A-Za-z0-9_]+)\}", m => captured[m.Groups[1].Value]);

    private static void Match(JsonElement t, JsonElement a, IReadOnlyDictionary<string, string> captured, string at, List<string> found)
    {
        if (Shape(t) is { } schema)
        {
            found.AddRange(Schema.Errors(schema, a, at));
            return;
        }

        switch (t.ValueKind)
        {
            case JsonValueKind.Object:
                MatchObject(t, a, captured, at, found);
                break;
            case JsonValueKind.Array:
                MatchArray(t, a, captured, at, found);
                break;
            case JsonValueKind.String:
                Expect(a.ValueKind == JsonValueKind.String && a.GetString() == Fill(t.GetString()!, captured), t, a, at, found);
                break;
            case JsonValueKind.Number:
                Expect(a.ValueKind == JsonValueKind.Number && a.GetDouble() == t.GetDouble(), t, a, at, found);
                break;
            default:
                Expect(a.ValueKind == t.ValueKind, t, a, at, found);
                break;
        }
    }

    private static JsonElement? Shape(JsonElement t) =>
        t.ValueKind == JsonValueKind.Object && t.TryGetProperty("$shape", out JsonElement schema) && t.EnumerateObject().Count() == 1
            ? schema
            : null;

    private static void MatchObject(JsonElement t, JsonElement a, IReadOnlyDictionary<string, string> captured, string at, List<string> found)
    {
        if (a.ValueKind != JsonValueKind.Object)
        {
            found.Add($"{at}: expected an object, got {a.ValueKind}");
            return;
        }

        var expected = t.EnumerateObject().Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
        foreach (JsonProperty extra in a.EnumerateObject().Where(p => !expected.Contains(p.Name)))
        {
            found.Add($"{at}/{extra.Name}: unexpected member");
        }

        foreach (JsonProperty member in t.EnumerateObject())
        {
            if (a.TryGetProperty(member.Name, out JsonElement value))
            {
                Match(member.Value, value, captured, at + "/" + member.Name, found);
            }
            else
            {
                found.Add($"{at}/{member.Name}: missing");
            }
        }
    }

    private static void MatchArray(JsonElement t, JsonElement a, IReadOnlyDictionary<string, string> captured, string at, List<string> found)
    {
        if (a.ValueKind != JsonValueKind.Array || a.GetArrayLength() != t.GetArrayLength())
        {
            found.Add($"{at}: expected an array of {t.GetArrayLength()}");
            return;
        }

        for (int i = 0; i < t.GetArrayLength(); i++)
        {
            Match(t[i], a[i], captured, at + "/" + i.ToString(CultureInfo.InvariantCulture), found);
        }
    }

    private static void Expect(bool ok, JsonElement t, JsonElement a, string at, List<string> found)
    {
        if (!ok)
        {
            found.Add($"{at}: expected {Short(t)}, got {Short(a)}");
        }
    }

    private static string Short(JsonElement e)
    {
        string text = e.ValueKind == JsonValueKind.Undefined ? "nothing" : e.GetRawText();
        return text.Length > 80 ? text[..80] + "..." : text;
    }
}

/// <summary>The JSON Schema keywords the kit's <c>$shape</c> markers use.</summary>
internal static class Schema
{
    /// <summary>Every way <paramref name="value"/> fails <paramref name="schema"/>.</summary>
    internal static IEnumerable<string> Errors(JsonElement schema, JsonElement value, string at)
    {
        var found = new List<string>();
        Check(schema, value, at, found);
        return found;
    }

    private static void Check(JsonElement schema, JsonElement value, string at, List<string> found)
    {
        if (schema.TryGetProperty("type", out JsonElement type) && !OfType(type.GetString()!, value))
        {
            found.Add($"{at}: not of type {type.GetString()}");
            return;
        }

        if (schema.TryGetProperty("const", out JsonElement constant) && constant.GetRawText() != value.GetRawText())
        {
            found.Add($"{at}: not {constant.GetRawText()}");
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            CheckString(schema, value.GetString()!, at, found);
        }

        if (value.ValueKind == JsonValueKind.Object)
        {
            CheckObject(schema, value, at, found);
        }

        if (value.ValueKind == JsonValueKind.Array && schema.TryGetProperty("items", out JsonElement items))
        {
            int i = 0;
            foreach (JsonElement item in value.EnumerateArray())
            {
                Check(items, item, at + "/" + i++.ToString(CultureInfo.InvariantCulture), found);
            }
        }
    }

    private static void CheckString(JsonElement schema, string text, string at, List<string> found)
    {
        if (schema.TryGetProperty("pattern", out JsonElement pattern)
            && !Regex.IsMatch(text, pattern.GetString()!, RegexOptions.None, TimeSpan.FromSeconds(5)))
        {
            found.Add($"{at}: does not match {pattern.GetString()}");
        }

        if (schema.TryGetProperty("format", out JsonElement format) && !OfFormat(format.GetString()!, text))
        {
            found.Add($"{at}: not a {format.GetString()}");
        }
    }

    private static void CheckObject(JsonElement schema, JsonElement value, string at, List<string> found)
    {
        if (schema.TryGetProperty("required", out JsonElement required))
        {
            foreach (JsonElement name in required.EnumerateArray())
            {
                if (!value.TryGetProperty(name.GetString()!, out _))
                {
                    found.Add($"{at}/{name.GetString()}: missing");
                }
            }
        }

        bool hasProperties = schema.TryGetProperty("properties", out JsonElement properties);
        bool hasAdditional = schema.TryGetProperty("additionalProperties", out JsonElement additional);
        foreach (JsonProperty member in value.EnumerateObject())
        {
            if (hasProperties && properties.TryGetProperty(member.Name, out JsonElement sub))
            {
                Check(sub, member.Value, at + "/" + member.Name, found);
            }
            else if (hasAdditional && additional.ValueKind == JsonValueKind.Object)
            {
                Check(additional, member.Value, at + "/" + member.Name, found);
            }
        }
    }

    private static bool OfType(string type, JsonElement value) =>
        type switch
        {
            "string" => value.ValueKind == JsonValueKind.String,
            "integer" => value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out _),
            "number" => value.ValueKind == JsonValueKind.Number,
            "boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "array" => value.ValueKind == JsonValueKind.Array,
            "object" => value.ValueKind == JsonValueKind.Object,
            "null" => value.ValueKind == JsonValueKind.Null,
            _ => throw new NotSupportedException("JSON Schema type " + type),
        };

    private static bool OfFormat(string format, string text) =>
        format switch
        {
            "date-time" => Regex.IsMatch(text, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})$")
                && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
            "uuid" => Guid.TryParseExact(text, "D", out _),
            _ => throw new NotSupportedException("JSON Schema format " + format),
        };
}

/// <summary>HS256 tokens for the kit's <c>auth</c> principals (test credentials only).</summary>
internal static class Jwt
{
    /// <summary>A token for <paramref name="subject"/> with exactly <paramref name="scopes"/>.</summary>
    internal static string Mint(string secret, string subject, IEnumerable<string> scopes, string? issuer, string? audience)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var claims = new JsonObject
        {
            ["sub"] = subject,
            ["scopes"] = new JsonArray([.. scopes.Select(s => (JsonNode)JsonValue.Create(s))]),
            ["iat"] = now,
            ["exp"] = now + 300,
        };
        if (issuer is not null)
        {
            claims["iss"] = issuer;
        }

        if (audience is not null)
        {
            claims["aud"] = audience;
        }

        string input = Base64Url("""{"alg":"HS256","typ":"JWT"}"""u8) + "." + Base64Url(Encoding.UTF8.GetBytes(claims.ToJsonString()));
        byte[] mac = System.Security.Cryptography.HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(input));
        return input + "." + Base64Url(mac);
    }

    private static string Base64Url(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
