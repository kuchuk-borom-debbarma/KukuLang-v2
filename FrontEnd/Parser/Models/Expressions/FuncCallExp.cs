using FrontEnd.Parser.Services;

namespace KukuLang.Parser.Models.Expressions;

/// <summary>
/// Represents a function call
/// </summary>
public class FuncCallExp(string functionName, Dictionary<string, ExpressionStmt>? paramAndValPair)
    : ExpressionStmt("Function Call Exp")
{
    public Dictionary<string, ExpressionStmt>? ParamAndValPair { get; set; } = paramAndValPair;
    public string FunctionName { get; set; } = functionName;

    public override string ToString(int indentLevel = 0)
    {
        var result = $"{base.ToString(indentLevel)}\n" +
                     $"{IndentHelper.Indent($"Function Name: {FunctionName}", indentLevel + 2)}\n";

        result += FormatParameters(indentLevel + 2);

        return result;
    }

    private string FormatParameters(int indentLevel)
    {
        if (ParamAndValPair == null || ParamAndValPair.Count == 0)
        {
            return IndentHelper.Indent("Parameters: None", indentLevel) + "\n";
        }

        var result = IndentHelper.Indent("Parameters:", indentLevel) + "\n";
        foreach (var param in ParamAndValPair)
        {
            result += FormatParameter(param, indentLevel + 2);
        }

        return result;
    }

    private static string FormatParameter(KeyValuePair<string, ExpressionStmt> param, int indentLevel)
    {
        return IndentHelper.Indent($"{param.Key}: {param.Value}", indentLevel) + "\n";
    }
}