// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

namespace CryptoService.Sdk;

/// <summary>
/// No usable answer arrived: the connection failed, an attempt or the
/// call's deadline timed out, the server tried to redirect, or a 2xx
/// answer could not be decoded. The cause is in
/// <see cref="Exception.InnerException"/>.
/// </summary>
public sealed class CryptoTransportException : CryptoServiceException
{
    /// <summary>Creates an exception with a default message.</summary>
    public CryptoTransportException()
    {
    }

    /// <summary>Creates an exception with the given message.</summary>
    /// <param name="message">What went wrong.</param>
    public CryptoTransportException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and its cause.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public CryptoTransportException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates an exception with a message, its cause and whether it timed out.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    /// <param name="timedOut">Whether an attempt or the deadline timed out.</param>
    public CryptoTransportException(string message, Exception? innerException, bool timedOut)
        : base(message, innerException)
    {
        TimedOut = timedOut;
    }

    /// <summary>Whether the per-attempt timeout or the call's deadline expired.</summary>
    public bool TimedOut { get; }
}
