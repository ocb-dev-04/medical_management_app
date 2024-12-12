namespace Shared.Global.Sources.Exceptions;

public sealed class TooManyRequestsException
    : Exception
{
    public int StatusCode { get; } = 429;

    public TooManyRequestsException(string message)
        : base(message)
    {
    }
}