

using FrontEnd.Commons.Tokens;
using FrontEnd.Parser.Models.Stmt;
using FrontEnd.Parser.Parsers;
using FrontEnd.Parser.Parsers.Pratt;
using KukuLang.Parser.Models.Stmt;
using System.Data;
using KukuLang.Parser.Models.CustomTask;
using KukuLang.Parser.Models.CustomType;
using KukuLang.Parser.Models.Exceptions;
using KukuLang.Parser.Models.Expressions;
using KukuLang.Parser.Models.Scope;

namespace FrontEnd.Parser.Services
{
    /** <summary>
     * Evaluates token instructions.
     * Each instruction statement must consume the terminator or scope completely.
     * </summary>
     */
    class TokenEvaluatorService
    {
        public static void EvaluateToken<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope)
        {
            //Each sub-evaluate method needs to consume the . or }
            switch (parser.CurrentToken.Type)
            {
                case TokenType.Define:
                    EvaluateDefineToken(parser, scope);
                    break;
                case TokenType.Set:
                    EvaluateSetToken(parser, scope);
                    break;
                case TokenType.Identifier:
                    EvaluateFunctionCall(parser, scope);
                    break;
                case TokenType.If:
                    EvaluateIfToken(parser, scope);
                    break;
                case TokenType.Return:
                    EvaluateReturnToken(parser, scope);
                    break;
                case TokenType.Until:
                    EvaluateLoopToken(parser, scope);
                    break;
                case TokenType.AsLongAs:
                    EvaluateLoopToken(parser, scope);
                    break;
                case TokenType.Print:
                    EvaluatePrint(parser, scope);
                    break;
                default:
                    throw new UnknownTokenException(parser.CurrentToken);
            }
        }

        //print with <expression>
        private static void EvaluatePrint<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope)
        {
            Console.WriteLine("Evaluating Print Token");
            parser.Advance(); //advance to with
            TokenValidatorService.ValidateToken(TokenType.With, parser.CurrentToken);
            parser.Advance(); //advance to the start of expression
            var pratt = new PrattParser(parser.Tokens, parser._Pos);
            var exp = pratt.Parse();
            parser._Pos = pratt._Pos;
            parser.Advance(); //consume the  ;
            scope.Statements.Add(new PrintStmt(exp));
        }

        private static void EvaluateLoopToken<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope)
        {
            Console.WriteLine("Evaluating Loop statement");

            bool isUntil = parser.CurrentToken.Type == TokenType.Until;
            parser.Advance(); //advance to the start of condition expression
            var pratt = new PrattParser(parser.Tokens, parser._Pos);
            var condition = pratt.Parse();
            parser._Pos = pratt._Pos;
            //We are at repeat
            TokenValidatorService.ValidateToken(TokenType.Repeat, parser.CurrentToken);
            parser.Advance(); //Advance to the {
            TokenValidatorService.ValidateToken(TokenType.CurlyBracesOpening, parser.CurrentToken);
            parser.Advance(); //Advance to the first statement
            var loopScope = new AstScope(scope.ScopeName + "->LoopScope");

            // Parse the if block
            while (parser.CurrentToken.Type != TokenType.CurlyBracesClosing)
            {
                EvaluateToken(parser, loopScope);
            }
            scope.Statements.Add(new LoopStmt(condition, isUntil, loopScope));
            parser.Advance(); // Consume the }

        }

        private static void EvaluateReturnToken<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope)
        {
            Console.WriteLine("Evaluating Return Statement");
            parser.Advance(); //Advance to start of expression OR .
            if (parser.CurrentToken.Type == TokenType.Semicolon)
            {
                scope.Statements.Add(new ReturnStmt(null));
                parser.Advance(); //Consume the ;
                return;
            }
            else
            {
                var pratt = new PrattParser(parser.Tokens, parser._Pos);
                var returnExp = pratt.Parse();
                parser._Pos = pratt._Pos;
                scope.Statements.Add(new ReturnStmt(returnExp));
            }
            parser.Advance(); //Consume the ;
        }

        private static void EvaluateFunctionCall<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope)
        {
            Console.WriteLine("Evaluating Function call");

            var functionNameToken = parser.CurrentToken;
            FuncCallExp? funcCallExp;

            var pratt = new PrattParser(parser.Tokens, parser._Pos);
            funcCallExp = pratt.Parse() as FuncCallExp;
            parser._Pos = pratt._Pos;

            var functionCallStmt = new FunctionCallStmt(functionNameToken.Value, funcCallExp);
            scope.Statements.Add(functionCallStmt);
            parser.Advance(); //consume ;
        }

        // Handles "define" tokens
        private static void EvaluateDefineToken<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope)
        {
            Console.WriteLine("Evaluating Define Statement");
            parser.Advance(); // Advance to the identifier token
            TokenValidatorService.ValidateToken(TokenType.Identifier, parser.CurrentToken);
            Token taskNameToken = parser.ConsumeCurrentToken(); // Store identifier and advance to "returning" or "with"

            switch (parser.CurrentToken.Type)
            {
                case TokenType.Returning:
                    EvaluateDefineTaskToken(parser, scope, taskNameToken);
                    break;

                case TokenType.With:
                    EvaluateDefineWithToken(parser, scope, taskNameToken);
                    break;

                default:
                    throw new UnexpectedTokenException(
                        [new Token(TokenType.With, "with", -1, -1), new Token(TokenType.Returning, "returning", -1, -1)],
                        parser.CurrentToken
                    );
            }
        }

        // Handles "define ... returning" tokens
        private static void EvaluateDefineTaskToken<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope, Token taskNameToken)
        {
            parser.Advance(); // Advance to "nothing" or return type
            var returnTypeToken = parser.ConsumeCurrentToken(); // Store return type and advance to "with" or "{"
            TokenValidatorService.ValidateToken([TokenType.Identifier, TokenType.Nothing], returnTypeToken);

            // Check for parameters
            Dictionary<string, string>? paramTypeVariables = null;
            if (parser.CurrentToken.Type == TokenType.With)
            {
                parser.Advance(); // Advance to the first parameter
                paramTypeVariables = StoreArgs(parser);
            }


            // Parse the task block
            TokenValidatorService.ValidateToken(TokenType.CurlyBracesOpening, parser.CurrentToken);
            parser.Advance(); // Consume the '{'
            var taskScope = new AstScope($"{scope.ScopeName}->{taskNameToken.Value}");
            while (parser.CurrentToken.Type != TokenType.CurlyBracesClosing)
            {
                EvaluateToken(parser, taskScope);
            }

            // Create and store the custom task
            CustomTaskBase customTask = new(taskNameToken.Value, returnTypeToken.Value, paramTypeVariables, taskScope);
            scope.CustomTasks.Add(customTask);
            parser.Advance(); // Consume the '}'
        }

        // Handles "define ... with" tokens
        private static void EvaluateDefineWithToken<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope, Token taskNameToken)
        {
            parser.Advance(); // Advance to the first property
            TokenValidatorService.ValidateToken(TokenType.Identifier, parser.CurrentToken);
            Dictionary<string, string>? typeVariables = StoreArgs(parser);
            if (typeVariables.Count <= 0) throw new NoPropertyException(parser.CurrentToken);

            // Create and store the custom type
            TokenValidatorService.ValidateToken(TokenType.Semicolon, parser.CurrentToken);
            CustomTypeBase customType = new(taskNameToken.Value, typeVariables);
            scope.CustomTypes.Add(customType);
            parser.Advance(); // Consume the '.'
        }

        // Handles "set" tokens
        private static void EvaluateSetToken<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope)
        {
            Console.WriteLine("Evaluating Set Statement");
            TokenValidatorService.ValidateToken(TokenType.Set, parser.CurrentToken);
            parser.Advance(); // Advance to the variable name

            var variableNameToken = parser.CurrentToken;
            var prattParser = new PrattParser(parser.Tokens, parser._Pos);
            var variableExp = prattParser.Parse() as NestedVariableExp
                ?? throw new Exception($"Variable Expression was evaluated to null for token {variableNameToken}");

            parser._Pos = prattParser._Pos; // Update the main parser's _pos
            TokenValidatorService.ValidateToken(TokenType.To, parser.CurrentToken);

            parser.Advance(); // Advance to the start of the value expression
            prattParser = new PrattParser(parser.Tokens, parser._Pos);
            var value = prattParser.Parse();
            parser._Pos = prattParser._Pos; // Update the main parser's _pos

            // Create and store the set statement
            var setToStmt = new SetToStmt(variableExp, value);
            scope.Statements.Add(setToStmt);
            parser.Advance(); // Consume the "."
        }

        // Handles "if" token
        private static void EvaluateIfToken<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser, AstScope scope)
        {
            Console.WriteLine("Evaluating If Statement");

            parser.Advance(); // Advance to the conditional expression

            var prattParser = new PrattParser(parser.Tokens, parser._Pos);
            var condition = prattParser.Parse() ?? throw new InvalidExpressionException("Conditional expression isn't valid");
            parser._Pos = prattParser._Pos; // Update the position of the main parser

            parser.Advance(); // Advance to the "{"
            TokenValidatorService.ValidateToken(TokenType.CurlyBracesOpening, parser.CurrentToken);

            var ifScope = new AstScope($"{scope.ScopeName}->Conditional");
            parser.Advance(); // Advance to the first statement

            // Parse the if block
            while (parser.CurrentToken.Type != TokenType.CurlyBracesClosing)
            {
                EvaluateToken(parser, ifScope);
            }

            // Create and store the if statement
            var ifStmt = new IfStmt(condition, ifScope);
            parser.Advance(); // Consume the "}"
            scope.Statements.Add(ifStmt);

            if (parser.CurrentToken.Type != TokenType.Else)
            {
                return;
            }

            //Else block exists
            TokenValidatorService.ValidateToken(TokenType.Else, parser.CurrentToken);
            parser.Advance(); //Advance to {
            TokenValidatorService.ValidateToken(TokenType.CurlyBracesOpening, parser.CurrentToken);
            var elseScope = new AstScope($"{scope.ScopeName}->Conditional");
            parser.Advance(); //advance to the start of the statement
            while (parser.CurrentToken.Type != TokenType.CurlyBracesClosing)
            {
                EvaluateToken(parser, elseScope);
            }
            parser.Advance(); //consume the } of else block
            ifStmt.ElseScope = elseScope;


        }

        /** <summary>
         * Returns a map of argument and value for the given token.
         * The initial token needs to be the FIRST identifier.
         * Eg age(int)
         * Eg name("name")
         * Eg name(kuku.name)
         * </summary>
         */
        public static Dictionary<string, string> StoreArgs<ParserReturnType, ParserArgument>(ParserBase<ParserReturnType, ParserArgument> parser)
        {
            var args = new Dictionary<string, string>();

            // Keep iterating until we reach ; or {
            while (parser.CurrentToken.Type != TokenType.Semicolon && parser.CurrentToken.Type != TokenType.CurlyBracesOpening)
            {
                // For params that come after the first, advance past the ","
                if (parser.CurrentToken.Type == TokenType.Comma)
                {
                    parser.Advance();
                }
                var propertyToken = parser.ConsumeCurrentToken(); // Store property name and advance to "("

                TokenValidatorService.ValidateToken(TokenType.RoundBracketsOpening, parser.CurrentToken);
                parser.Advance(); // Advance to the property type

                // Validate and store the property type based on ValType

                string strVal = parser.CurrentToken.Value;
                args.Add(propertyToken.Value, strVal);
                parser.Advance(2); // Advance to the ) then ,
            }

            return args;
        }

    }
}
