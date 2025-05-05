using KukuLang.Parser.Models.Expressions;

namespace FrontEnd.Parser.Models.Stmt
{
    public class FunctionCallStmt(string functionName, FuncCallExp functionExp) : StmtBase("Function call stmt")
    {
        public string FunctionName { get; } = functionName;
        public readonly FuncCallExp FunctionExp = functionExp;

        public override string ToString(int indentLevel = 0)
        {
            string indent = new(' ', indentLevel);
            string functionExpStr = FunctionExp.ToString(indentLevel + 2);

            return $"{indent}Call {FunctionName} with:\n{functionExpStr}";
        }
    }
}
