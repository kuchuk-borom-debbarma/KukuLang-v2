using FrontEnd.Parser.Services;
using KukuLang.Parser.Models.CustomTask;
using KukuLang.Parser.Models.CustomType;

namespace KukuLang.Parser.Models.Scope
{
    public class AstScope(string scopeName)
    {
        public readonly string ScopeName = scopeName;
        public List<CustomTypeBase> CustomTypes { get; } = [];
        public List<CustomTaskBase> CustomTasks { get; } = [];
        public List<FrontEnd.Parser.Models.Stmt.StmtBase> Statements { get; } = [];

        public string ToString(int indentLevel = 0)
        {
            string scopeNameStr = IndentHelper.Indent(ScopeName + ":", indentLevel) + "\n";
            string typesStr = $"{IndentHelper.Indent("Types:", indentLevel + 2)}\n";
            foreach (var t in CustomTypes)
            {
                typesStr += t.ToString(indentLevel + 4);
            }
            typesStr += "\n";
            string tasksStr = $"{IndentHelper.Indent("Tasks:", indentLevel + 2)}\n";
            foreach (var t in CustomTasks)
            {
                tasksStr += t.ToString(indentLevel + 4);
            }
            tasksStr += "\n";
            string stmtStr = $"{IndentHelper.Indent("Statements:", indentLevel + 2)}\n";
            foreach (var t in Statements)
            {
                stmtStr += t.ToString(indentLevel + 4) + "\n\n";
            }
            string final = scopeNameStr + typesStr + tasksStr + stmtStr;
            return final + "\n";
        }
    }
}