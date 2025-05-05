using FrontEnd.Parser.Models.Stmt;
using FrontEnd.Parser.Services;

namespace KukuLang.Parser.Models.CustomType;

/// <summary>
/// Represents data types. int, float and text will be present by default.
/// </summary>
/// <param name="typeName"></param>
/// <param name="varNameVarTypePair"></param>
public class CustomTypeBase(string typeName, Dictionary<string, string> varNameVarTypePair) : StmtBase($"CT:{typeName}")
{
    public string TypeName { get; } = typeName;
    public Dictionary<string, string> VarNameVarTypePair { get; } = varNameVarTypePair;

    public override string ToString(int indentLevel = 0)
    {
        string typeNamstr = $"{IndentHelper.Indent(TypeName, indentLevel)}\n";
        string paramsStr = $"{IndentHelper.Indent("Params: ", indentLevel + 2)}\n";
        foreach (var kvp in VarNameVarTypePair)
        {
            paramsStr += $"{IndentHelper.Indent($"{kvp.Key} : {kvp.Value}, ", indentLevel + 4)}\n";
        }
        var finalString = typeNamstr + paramsStr;
        return finalString + "\n";
    }
}