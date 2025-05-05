using KukuLang.Parser.Models.Expressions;

namespace FrontEnd.Parser.Models.Stmt
{
    public class ReturnStmt(ExpressionStmt? expression) : StmtBase("Return StmtBase")
    {
        public ExpressionStmt? Expression { get; } = expression;

        public override string ToString(int indentLevel = 0)
        {
            string indent = new(' ', indentLevel);
            string expressionStr = Expression?.ToString(indentLevel + 2) ?? "null";

            return $"{indent}Return:\n{expressionStr}";
        }
    }
}
