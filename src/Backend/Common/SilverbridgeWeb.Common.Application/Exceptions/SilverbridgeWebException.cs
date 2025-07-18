﻿using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Common.Application.Exceptions;

public sealed class SilverbridgeWebException : Exception
{
    public SilverbridgeWebException(string requestName, Error? error = default, Exception? innerException = default)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }

    public Error? Error { get; }
}
