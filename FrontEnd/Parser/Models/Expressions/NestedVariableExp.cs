using FrontEnd.Parser.Services;

namespace KukuLang.Parser.Models.Expressions;

/**
 * <summary>
 * Represents A property or multi level property variable such as
 * kuku or kuku.name
 * </summary>
 */
public class NestedVariableExp(string currentVariable, NestedVariableExp? nextNode) : ExpressionStmt("Nested Variable Exp")
{
    public string VarName { get; } = currentVariable;
    public NestedVariableExp? NextNode { get; } = nextNode;

    public override string ToString(int indentLevel = 0)
    {
        if (NextNode != null)
            return IndentHelper.Indent($"{VarName}.{NextNode.ToString()}", indentLevel);
        return IndentHelper.Indent($"{VarName}", indentLevel);
    }
}