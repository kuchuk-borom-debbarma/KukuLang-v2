using FrontEnd.Parser.Models.Stmt;
using KukuLang.Parser.Models.Expressions;

namespace KukuLang.Parser.Models.Stmt
{
    public class PrintStmt(ExpressionStmt expression) : StmtBase("Print Statement")
    {
        public ExpressionStmt Expression { get; } = expression;

        public override string ToString(int indentLevel = 0)
        {
            string indent = new(' ', indentLevel);
            string expressionStr = Expression.ToString(indentLevel + 2);

            return $"{indent}Print:\n{expressionStr}";
        }
    }
}
