namespace KukuLang.Parser.Models.Expressions.Literals;

public abstract class LiteralExp(string type) : ExpressionStmt(type)
{
    private readonly string _type = type;

    public override string ToString()
    {
        return $"{GetType().Name}: {_type}";
    }
}