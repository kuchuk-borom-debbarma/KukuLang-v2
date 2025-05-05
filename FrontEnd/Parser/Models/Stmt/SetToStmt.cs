using KukuLang.Parser.Models.Expressions;

namespace FrontEnd.Parser.Models.Stmt
{
    public class SetToStmt(NestedVariableExp variableName, ExpressionStmt valueExp) : StmtBase("Set To")
    {
        public NestedVariableExp VariableToSet { get; } = variableName;
        public ExpressionStmt VarVal { get; } = valueExp;

        public override string ToString(int indentLevel = 0)
        {
            string variableNameStr = VariableToSet.ToString(indentLevel);
            string variableValStr = VarVal.ToString(indentLevel + 2);

            return $"{variableNameStr} :\n{variableValStr}";
        }
    }



}