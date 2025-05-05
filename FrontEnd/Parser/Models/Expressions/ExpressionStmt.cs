using FrontEnd.Parser.Services;

namespace KukuLang.Parser.Models.Expressions;

public abstract class ExpressionStmt(string type)
{
    public virtual string ToString(int indentLevel = 0)
    {
        return IndentHelper.Indent($"{type}", indentLevel);
    }
}