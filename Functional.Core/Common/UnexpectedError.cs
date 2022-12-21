namespace Meigs2.Functional.Common;

public record UnexpectedError(string Message, int Code = 0, Option<Error> Inner = default) : Error
{
    public override string Message { get; init; } = Message;
    public override int Code { get; init; } = Code;
    public override Option<Error> Inner { get; } = Inner;
    public override string ToString() => Message;
    public override ErrorException ToErrorException() => new ExceptionalException(Message, Code);
    public override bool Is<E>() => false;
    public override bool IsExceptional => false;
}