// SPDX-License-Identifier: Apache-2.0 OR MIT

namespace CryptoService.Sdk;

/// <summary>
/// The base of every error the client raises for a call that did not
/// succeed: <see cref="CryptoApiException"/> when the server answered
/// outside 2xx, <see cref="CryptoTransportException"/> when no usable
/// answer arrived.
/// </summary>
/// <remarks>
/// Messages never carry a credential, a key, a plaintext or a body, so
/// they are safe to log. Cancelling a call through its
/// <see cref="CancellationToken"/> raises
/// <see cref="OperationCanceledException"/> instead.
/// </remarks>
public abstract class CryptoServiceException : Exception
{
    /// <summary>Creates an exception with a default message.</summary>
    protected CryptoServiceException()
        : base("The Crypto Service call failed.")
    {
    }

    /// <summary>Creates an exception with the given message.</summary>
    /// <param name="message">What went wrong.</param>
    protected CryptoServiceException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and its cause.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    protected CryptoServiceException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
