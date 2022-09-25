using System;

namespace Functional.Core;

public static class Error
{
    public static UnexpectedError Unexpected(string message)
    {
        return new UnexpectedError(message);
    }
    
}