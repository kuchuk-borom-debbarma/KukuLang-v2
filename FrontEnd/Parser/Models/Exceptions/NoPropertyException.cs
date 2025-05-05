using FrontEnd.Commons.Tokens;

namespace KukuLang.Parser.Models.Exceptions;

public class NoPropertyException(Token token) : Exception
{
    private readonly string _msg = $"No Property or Parameter for Definition : {token.Value} at position {token.Position}";
    public override string Message => _msg;
}