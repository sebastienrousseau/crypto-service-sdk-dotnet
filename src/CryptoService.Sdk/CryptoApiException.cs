// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Globalization;

namespace CryptoService.Sdk;

/// <summary>
/// The server answered with a status outside 2xx. <see cref="Problem"/>
/// is its RFC 9457 problem; an answer without one (a proxy's HTML page, an
/// empty body) becomes an <c>about:blank</c> problem for the status.
/// </summary>
/// <remarks>
/// The message names the status, the problem type and its title only.
/// The <see cref="ProblemDetails.Detail"/> is kept in
/// <see cref="Problem"/> for the caller to inspect.
/// </remarks>
public sealed class CryptoApiException : CryptoServiceException
{
    /// <summary>Creates an exception with a default message and an empty problem.</summary>
    public CryptoApiException()
        : this("The Crypto Service API returned an error.")
    {
    }

    /// <summary>Creates an exception with the given message and an empty problem.</summary>
    /// <param name="message">What went wrong.</param>
    public CryptoApiException(string message)
        : this(message, null)
    {
    }

    /// <summary>Creates an exception with a message, its cause and an empty problem.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public CryptoApiException(string message, Exception? innerException)
        : base(message, innerException)
    {
        Problem = ProblemDetails.Blank(0, null);
    }

    /// <summary>Creates an exception for an error answer.</summary>
    /// <param name="statusCode">The HTTP status code of the answer.</param>
    /// <param name="problem">The server's problem, or the <c>about:blank</c>
    /// one built for a body that is not a problem.</param>
    /// <exception cref="ArgumentNullException"><paramref name="problem"/> is
    /// <see langword="null"/>.</exception>
    public CryptoApiException(int statusCode, ProblemDetails problem)
        : base(Describe(statusCode, problem))
    {
        StatusCode = statusCode;
        Problem = problem;
    }

    /// <summary>The HTTP status code of the answer, or 0 when there was none.</summary>
    public int StatusCode { get; }

    /// <summary>The RFC 9457 problem the server answered with.</summary>
    public ProblemDetails Problem { get; }

    private static string Describe(int statusCode, ProblemDetails problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"The Crypto Service API answered HTTP {statusCode} ({problem.Type}: {problem.Title}); see Problem for the details.");
    }
}
