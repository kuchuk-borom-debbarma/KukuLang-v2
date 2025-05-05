using FrontEnd.Commons.Tokens;
using FrontEnd.Parser.Parsers;
using FrontEnd.Parser.Services;
using KukuLang.Parser.Models.Scope;

namespace KukuLang.Parser.Parsers.Recursive_MAIN_;

public class RecursiveDescentParser(List<Token> tokens, int startingPosition = 0)
    : ParserBase<AstScope, dynamic>(tokens, startingPosition)
{
    private readonly AstScope _currentScope = new("ASTRootScope");

    //Parameter not used so we set it as null to keep ParserBase happy.
    public override AstScope Parse(dynamic? arg)
    {
        while (CurrentToken.Type != TokenType.EOF)
        {
            //Handle Custom Type declaration
            TokenEvaluatorService.EvaluateToken(this, _currentScope);
            //Handle Set Statements
        }

        return _currentScope;
    }
}