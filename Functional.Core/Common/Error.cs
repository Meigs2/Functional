using System;

namespace Functional.Core;

public record Error : UnexpectedError
{
    public Error(string message) : base(message)
    {
    }
}
