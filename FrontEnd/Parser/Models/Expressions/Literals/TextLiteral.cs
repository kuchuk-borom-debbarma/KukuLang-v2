namespace KukuLang.Parser.Models.Expressions.Literals
{
    internal class TextLiteral(string text) : LiteralExp("Text Literal")
    {
        public readonly string Val = text;

        public override string ToString()
        {
            return $"TextLiteral: \"{Val}\"";
        }
    }
}
