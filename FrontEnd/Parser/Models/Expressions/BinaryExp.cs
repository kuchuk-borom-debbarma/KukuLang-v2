using FrontEnd.Parser.Services;

namespace KukuLang.Parser.Models.Expressions;

public class BinaryExp(ExpressionStmt left, string op, ExpressionStmt right) : ExpressionStmt("BinaryExp")
{
    public ExpressionStmt Left { get; } = left;
    public string Op { get; } = op;
    public ExpressionStmt Right { get; } = right;

    public override string ToString(int indentLevel = 0)
    {
        return $"{base.ToString(indentLevel)}\n" +
               $"{IndentHelper.Indent($"Operator: {Op}", indentLevel + 2)}\n" +
               $"{IndentHelper.Indent($"Left:", indentLevel + 2)}\n{Left.ToString(indentLevel + 4)}\n" +
               $"{IndentHelper.Indent($"Right:", indentLevel + 2)}\n{Right.ToString(indentLevel + 4)}";
    }
}