using KukuLang.Parser.Models.Expressions;
using KukuLang.Parser.Models.Scope;

namespace FrontEnd.Parser.Models.Stmt
{
    public class IfStmt(ExpressionStmt condition, AstScope scope, AstScope? elseScope = null) : StmtBase("If Statement")
    {
        public ExpressionStmt Condition { get; } = condition;
        public AstScope Scope { get; } = scope;
        public AstScope? ElseScope { get; set; } = elseScope;

        public override string ToString(int indentLevel = 0)
        {
            string indent = new(' ', indentLevel);
            string conditionIndent = new(' ', indentLevel + 2);

            string conditionStr = Condition.ToString(indentLevel + 2);
            string scopeStr = Scope.ToString(indentLevel + 4);
            string elseStr = ElseScope != null ? $"\n{indent}else\n{ElseScope.ToString(indentLevel + 4)}" : string.Empty;

            return $"{indent}if\n{conditionIndent}{conditionStr}\n{conditionIndent}then\n{scopeStr}{elseStr}";
        }
    }
}
