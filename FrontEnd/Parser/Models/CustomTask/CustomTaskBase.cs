using FrontEnd.Parser.Models.Stmt;
using FrontEnd.Parser.Services;
using KukuLang.Parser.Models.Scope;

namespace KukuLang.Parser.Models.CustomTask;

public class CustomTaskBase(string taskName, string taskReturnType, Dictionary<string, string> paramNameParamTypePair, AstScope taskScope) : StmtBase(taskName)
{
    public string TaskName { get; } = taskName;
    public string TaskReturnType { get; } = taskReturnType;
    public Dictionary<string, string> ParamNameParamTypePair { get; } = paramNameParamTypePair;

    public readonly AstScope TaskScope = taskScope;

    public override string ToString(int indentLevel = 0)
    {
        string taskNamestr = $"{IndentHelper.Indent(TaskName, indentLevel)}\n";

        string parameters = IndentHelper.Indent("Params:", indentLevel + 2) + "\n";
        foreach (var kv in ParamNameParamTypePair)
        {
            parameters += IndentHelper.Indent($"{kv.Key} : {kv.Value}", indentLevel + 4) + "\n";
        }
        parameters += "\n";
        string returnTypeStr = $"{IndentHelper.Indent("Returns : " + TaskReturnType, indentLevel)}\n";
        string body = IndentHelper.Indent("Statements:", indentLevel + 2) + "\n";
        body += TaskScope.ToString(indentLevel + 4);
        string finalString = taskNamestr + returnTypeStr + parameters + body;
        return finalString + "\n";
    }
}