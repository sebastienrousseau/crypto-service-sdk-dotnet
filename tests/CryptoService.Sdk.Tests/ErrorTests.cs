// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Net;
using System.Text;
using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk.Tests;

/// <summary>The error model: problems, about:blank, decoding failures and the exception types.</summary>
public sealed class ErrorTests
{
    [Fact]
    public async Task AProblemIsReadInFull()
    {
        const string problem = """
            {"type":"urn:crypto-service:problem:validation-failed","title":"Validation failed","status":400,
             "detail":"body must have required property 'data'","instance":"/v2/hash","code":"INVALID_HEX",
             "errors":[{"field":"/data","message":"must have required property 'data'"}],
             "limit":60,"resetSeconds":12,"traceId":"t"}
            """;
        using CryptoServiceClient client = StubHandler.Client(
            StubHandler.Answer(HttpStatusCode.BadRequest, problem, "application/problem+json"));

        CryptoApiException error = await Assert.ThrowsAsync<CryptoApiException>(
            () => client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "a" }));

        Assert.Equal(400, error.StatusCode);
        ProblemDetails p = error.Problem;
        Assert.Equal("urn:crypto-service:problem:validation-failed", p.Type);
        Assert.Equal("Validation failed", p.Title);
        Assert.Equal(400, p.Status);
        Assert.Equal("/v2/hash", p.Instance);
        Assert.Equal("INVALID_HEX", p.Code);
        Assert.Equal(60, p.Limit);
        Assert.Equal(12, p.ResetSeconds);
        ValidationError field = Assert.Single(p.Errors!);
        Assert.Equal("/data", field.Field);
        Assert.Equal("must have required property 'data'", field.Message);
        Assert.Equal("t", p.Extensions!["traceId"].GetString());

        // The message names the status, type and title, never the detail.
        Assert.Equal(
            "The Crypto Service API answered HTTP 400 (urn:crypto-service:problem:validation-failed: Validation failed); see Problem for the details.",
            error.Message);
        Assert.DoesNotContain("required property", error.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("<html>bad gateway</html>")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("""{"detail":"no type"}""")]
    [InlineData("""{"type":"t","title":"t","status":"502","detail":"d"}""")]
    [InlineData("""[1,2]""")]
    public async Task ABodyThatIsNotAProblemBecomesAboutBlank(string body)
    {
        using CryptoServiceClient client = StubHandler.Client(StubHandler.Answer(HttpStatusCode.BadGateway, body, "text/html"));

        CryptoApiException error = await Assert.ThrowsAsync<CryptoApiException>(() => client.HealthAsync());

        Assert.Equal(502, error.StatusCode);
        Assert.Equal("about:blank", error.Problem.Type);
        Assert.Equal("Bad Gateway", error.Problem.Title);
        Assert.Equal(502, error.Problem.Status);
        Assert.Equal("The server answered 502 without a problem body", error.Problem.Detail);
    }

    [Fact]
    public async Task AnOversizedErrorBodyIsNotRead()
    {
        string huge = "{\"type\":\"t\",\"title\":\"t\",\"status\":500,\"detail\":\"" + new string('x', Decoding.MaxErrorBodyBytes) + "\"}";
        using CryptoServiceClient client = StubHandler.Client(StubHandler.Answer(HttpStatusCode.InternalServerError, huge));

        CryptoApiException error = await Assert.ThrowsAsync<CryptoApiException>(() => client.HealthAsync());

        Assert.Equal("about:blank", error.Problem.Type);
    }

    [Fact]
    public async Task AnErrorBodyOfExactlyTheCapIsRead()
    {
        byte[] body = Encoding.UTF8.GetBytes(new string(' ', Decoding.MaxErrorBodyBytes));
        using var content = new ByteArrayContent(body);

        byte[] read = await Decoding.ReadCappedAsync(content, Decoding.MaxErrorBodyBytes, CancellationToken.None);

        Assert.Equal(Decoding.MaxErrorBodyBytes, read.Length);
    }

    [Fact]
    public void ABlankProblemWithoutAReasonPhraseHasAGenericTitle()
    {
        Assert.Equal("HTTP error", ProblemDetails.Blank(599, null).Title);
        Assert.Equal("HTTP error", ProblemDetails.Blank(599, "").Title);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{"data":null}""")]
    [InlineData("""{"data":{"algorithm":"sha256","digest":"00"}}""")]
    [InlineData("""{"data":{"algorithm":"sha256","digest":null,"length":1}}""")]
    [InlineData("""{"data":{"algorithm":"sha256","digest":"00","length":"1"}}""")]
    [InlineData("not json")]
    public async Task A2xxAnswerThatBreaksTheContractIsATransportError(string body)
    {
        using CryptoServiceClient client = StubHandler.Client(StubHandler.Answer(HttpStatusCode.OK, body));

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(
            () => client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "a" }));

        Assert.False(error.TimedOut);
        Assert.Equal("The server's 2xx answer is not the JSON the operation documents.", error.Message);
    }

    [Fact]
    public void TheSynchronousDecoderMatchesTheStreamingOne()
    {
        HashResult result = Decoding.DecodeSuccess(
            """{"algorithm":"sha256","digest":"00","length":1}"""u8,
            CryptoJsonContext.Default.HashResult);

        Assert.Equal(1, result.Length);
        Assert.Throws<CryptoTransportException>(() => Decoding.DecodeSuccess("null"u8, CryptoJsonContext.Default.HashResult));
        Assert.Throws<CryptoTransportException>(() => Decoding.DecodeSuccess("{"u8, CryptoJsonContext.Default.HashResult));
    }

    [Fact]
    public void TheExceptionTypesHaveTheStandardConstructors()
    {
        var inner = new InvalidOperationException("cause");

        Assert.Equal(0, new CryptoApiException().StatusCode);
        Assert.Equal("m", new CryptoApiException("m").Message);
        Assert.Same(inner, new CryptoApiException("m", inner).InnerException);
        Assert.Equal("about:blank", new CryptoApiException("m").Problem.Type);
        Assert.Throws<ArgumentNullException>(() => new CryptoApiException(400, null!));

        Assert.False(new CryptoTransportException().TimedOut);
        Assert.Equal("m", new CryptoTransportException("m").Message);
        Assert.Same(inner, new CryptoTransportException("m", inner).InnerException);
        Assert.True(new CryptoTransportException("m", inner, timedOut: true).TimedOut);

        CryptoServiceException error = new CryptoTransportException();
        Assert.Equal("The Crypto Service call failed.", error.Message);
        Assert.Equal("The Crypto Service API returned an error.", new CryptoApiException().Message);
    }

    [Theory]
    [InlineData("GET", "/v2/algorithms", true)]
    [InlineData("GET", "/health", true)]
    [InlineData("POST", "/v2/hash", true)]
    [InlineData("POST", "/v2/sign", true)]
    [InlineData("POST", "/v2/kms/encrypt", true)]
    [InlineData("POST", "/v2/opaque/login/init", true)]
    [InlineData("POST", "/v2/keys/export", true)]
    [InlineData("POST", "/v2/keys/wrap", true)]
    [InlineData("POST", "/v2/keys/generate", false)]
    [InlineData("POST", "/v2/keys/delete", false)]
    [InlineData("POST", "/v2/pq/keygen", false)]
    [InlineData("POST", "/v2/pq/hybrid/keygen", false)]
    [InlineData("POST", "/v2/pq/dsa/keygen", false)]
    [InlineData("POST", "/v2/pq/slh-dsa/keygen", false)]
    [InlineData("POST", "/v2/hpke/keygen", false)]
    [InlineData("POST", "/v2/kms/create-key", false)]
    [InlineData("POST", "/v2/opaque/register/init", false)]
    [InlineData("POST", "/v2/opaque/register/finish", false)]
    public void TheRetrySafetyTableMatchesTheReferenceSdk(string method, string path, bool idempotent)
    {
        Assert.Equal(idempotent, Idempotency.IsIdempotent(new HttpMethod(method), path));
    }

    [Fact]
    public void ANoRetryAfterHeaderAsksForNothing()
    {
        Assert.Null(RetryDecision.RetryAfter(null, DateTimeOffset.UnixEpoch));
    }
}
