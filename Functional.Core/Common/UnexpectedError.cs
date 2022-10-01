using System;
using System.Diagnostics.Contracts;

namespace Functional.Core;

public record UnexpectedError : Error
{
    public UnexpectedError(string Message, int Code = 0, Option<Error> Inner = default)
    {
        this.Message = Message;
        this.Code = Code;
        this.Inner = Inner;
    }
    
    public override string Message { get; }

    public override int Code { get; }
    
    public override Option<Error> Inner { get; }
    
    public override string ToString() => 
        Message;

    public override ErrorException ToErrorException() => 
        new ExceptionalException(Message, Code);

    public override bool Is<E>() =>
        false;
    
    public override bool IsExceptional =>
        false;
}

