using System.Text;
using FrontEnd.Commons.Tokens;

namespace KukuLang.Parser.Models.Exceptions;

public class UnexpectedTokenException : Exception
{
    private readonly string _msg = "";
    public UnexpectedTokenException(TokenType expectedTokenType, Token actualToken)
    {
        _msg = $"Expected Token of type: {expectedTokenType} but got token: {actualToken}";
    }
    public UnexpectedTokenException(TokenType expectedTokenType, dynamic expectedTokenVal, Token actualToken)
    {
        _msg = $"Expected Token of type: {new Token(expectedTokenType, expectedTokenVal, -1, -1)} but got token: {actualToken}";
    }
    public UnexpectedTokenException(Token expectedToken, Token actualToken)
    {
        _msg = $"Expected Token: {expectedToken} but got token: {actualToken}";
    }

    public UnexpectedTokenException(List<Token> expectedTokens, Token actualToken)
    {
        var sb = new StringBuilder();
        foreach (var t in expectedTokens)
        {
            sb.Append(t + " | ");
        }
        _msg = $"Expected Tokens: {sb} but got token: {actualToken}";
    }
    public override string Message => _msg;
}
