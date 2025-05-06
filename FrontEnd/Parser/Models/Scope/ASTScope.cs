using System.Text.Json;
using FrontEnd.Parser.Services;
using KukuLang.Parser.Models.CustomTask;
using KukuLang.Parser.Models.CustomType;

namespace KukuLang.Parser.Models.Scope;

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

    public string ToJson(int indentLevel = 0)
    {
        var scopeObject = new
        {
            ScopeName,
            CustomTypes = CustomTypes.Select(type => new
            {
                TypeName = type.TypeName,
                Properties = type.VarNameVarTypePair.Select(prop => new
                {
                    Name = prop.Key,
                    Type = prop.Value
                }).ToList()
            }).ToList(),
            CustomTasks = CustomTasks.Select(task => new
            {
                TaskName = task.TaskName,
                ReturnType = task.TaskReturnType,
                Parameters = task.ParamNameParamTypePair.Select(param => new
                {
                    Name = param.Key,
                    Type = param.Value
                }).ToList(),
                Scope = task.TaskScope.ScopeName
                // Avoiding recursive reference by just including the scope name
            }).ToList(),
            Statements = Statements.Select(stmt => new
            {
                Type = stmt.GetType().Name,
                // Clean description to avoid invalid control characters
                Description = CleanStringForJson(stmt.ToString(0))
            }).ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return JsonSerializer.Serialize(scopeObject, options);
    }

// Helper method to clean strings for JSON serialization
    private string CleanStringForJson(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Replace problematic escape sequences and control characters
        string result = input
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("\"", "\\\"")
            .Replace("\\", "\\\\");

        // Remove any remaining control characters
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in result)
        {
            if (!char.IsControl(c) || c == '\n' || c == '\r' || c == '\t')
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public string ToMermaid()
    {
        var mermaid = new System.Text.StringBuilder();

        // Start the flowchart definition with LR direction and class definitions
        mermaid.AppendLine("flowchart TB");
        mermaid.AppendLine("    classDef scope fill:#f9f,stroke:#333,stroke-width:2px");
        mermaid.AppendLine("    classDef type fill:#bbf,stroke:#33f,stroke-width:1px");
        mermaid.AppendLine("    classDef task fill:#bfb,stroke:#3f3,stroke-width:1px");
        mermaid.AppendLine("    classDef stmt fill:#fbb,stroke:#f33,stroke-width:1px");
        mermaid.AppendLine("    classDef property fill:#ddf,stroke:#33f,stroke-width:1px");
        mermaid.AppendLine("    classDef parameter fill:#dfd,stroke:#3f3,stroke-width:1px");

        // Add the scope node
        string scopeNodeId = SanitizeForMermaid(ScopeName);
        mermaid.AppendLine($"    {scopeNodeId}[\"Scope: {ScopeName}\"]");
        mermaid.AppendLine($"    class {scopeNodeId} scope");

        // Types section
        if (CustomTypes.Any())
        {
            string typesNodeId = $"{scopeNodeId}_Types";
            mermaid.AppendLine($"    {typesNodeId}[\"Custom Types\"]");
            mermaid.AppendLine($"    {scopeNodeId} --> {typesNodeId}");

            foreach (var type in CustomTypes)
            {
                string typeNodeId = $"{typesNodeId}_{SanitizeForMermaid(type.TypeName)}";
                mermaid.AppendLine($"    {typeNodeId}[\"Type: {type.TypeName}\"]");
                mermaid.AppendLine($"    {typesNodeId} --> {typeNodeId}");
                mermaid.AppendLine($"    class {typeNodeId} type");

                foreach (var prop in type.VarNameVarTypePair)
                {
                    string propNodeId = $"{typeNodeId}_{SanitizeForMermaid(prop.Key)}";
                    mermaid.AppendLine($"    {propNodeId}[\"Property: {prop.Key}: {prop.Value}\"]");
                    mermaid.AppendLine($"    {typeNodeId} --> {propNodeId}");
                    mermaid.AppendLine($"    class {propNodeId} property");
                }
            }
        }

        // Tasks section
        if (CustomTasks.Any())
        {
            string tasksNodeId = $"{scopeNodeId}_Tasks";
            mermaid.AppendLine($"    {tasksNodeId}[\"Custom Tasks\"]");
            mermaid.AppendLine($"    {scopeNodeId} --> {tasksNodeId}");

            foreach (var task in CustomTasks)
            {
                string taskNodeId = $"{tasksNodeId}_{SanitizeForMermaid(task.TaskName)}";
                mermaid.AppendLine($"    {taskNodeId}[\"Task: {task.TaskName}: {task.TaskReturnType}\"]");
                mermaid.AppendLine($"    {tasksNodeId} --> {taskNodeId}");
                mermaid.AppendLine($"    class {taskNodeId} task");

                // Parameters
                if (task.ParamNameParamTypePair.Any())
                {
                    string paramsNodeId = $"{taskNodeId}_Params";
                    mermaid.AppendLine($"    {paramsNodeId}[\"Parameters\"]");
                    mermaid.AppendLine($"    {taskNodeId} --> {paramsNodeId}");

                    foreach (var param in task.ParamNameParamTypePair)
                    {
                        string paramNodeId = $"{paramsNodeId}_{SanitizeForMermaid(param.Key)}";
                        mermaid.AppendLine($"    {paramNodeId}[\"Param: {param.Key}: {param.Value}\"]");
                        mermaid.AppendLine($"    {paramsNodeId} --> {paramNodeId}");
                        mermaid.AppendLine($"    class {paramNodeId} parameter");
                    }
                }

                // Task scope - Recursively show the task scope structure
                string taskScopeNodeId = $"{taskNodeId}_Scope";
                mermaid.AppendLine($"    {taskScopeNodeId}[\"Scope: {task.TaskScope.ScopeName}\"]");
                mermaid.AppendLine($"    {taskNodeId} --> {taskScopeNodeId}");
                mermaid.AppendLine($"    class {taskScopeNodeId} scope");

                // Recursively add task scope statements
                if (task.TaskScope.Statements.Any())
                {
                    string taskStmtsNodeId = $"{taskScopeNodeId}_Statements";
                    mermaid.AppendLine($"    {taskStmtsNodeId}[\"Statements\"]");
                    mermaid.AppendLine($"    {taskScopeNodeId} --> {taskStmtsNodeId}");

                    for (int i = 0; i < task.TaskScope.Statements.Count; i++)
                    {
                        var stmt = task.TaskScope.Statements[i];
                        string stmtNodeId = $"{taskStmtsNodeId}_{i}";
                        string stmtType = stmt.GetType().Name;

                        mermaid.AppendLine($"    {stmtNodeId}[\"Statement {i + 1}: {stmtType}\"]");
                        mermaid.AppendLine($"    {taskStmtsNodeId} --> {stmtNodeId}");
                        mermaid.AppendLine($"    class {stmtNodeId} stmt");
                    }
                }
            }
        }

        // Statements section
        if (Statements.Any())
        {
            string stmtsNodeId = $"{scopeNodeId}_Statements";
            mermaid.AppendLine($"    {stmtsNodeId}[\"Statements\"]");
            mermaid.AppendLine($"    {scopeNodeId} --> {stmtsNodeId}");

            for (int i = 0; i < Statements.Count; i++)
            {
                ProcessStatement(mermaid, stmtsNodeId, Statements[i], i);
            }
        }

        return mermaid.ToString();
    }

// Helper method to recursively process statements
    private void ProcessStatement(System.Text.StringBuilder mermaid, string parentNodeId,
        FrontEnd.Parser.Models.Stmt.StmtBase stmt, int index)
    {
        string stmtNodeId = $"{parentNodeId}_{index}";
        string stmtType = stmt.GetType().Name;
        string cleanLabel = SanitizeContentForMermaid(stmt.ToString(0).Split('\n')[0]);

        // Limit label length for readability
        if (cleanLabel.Length > 30)
        {
            cleanLabel = cleanLabel.Substring(0, 27) + "...";
        }

        mermaid.AppendLine($"    {stmtNodeId}[\"{stmtType}: {cleanLabel}\"]");
        mermaid.AppendLine($"    {parentNodeId} --> {stmtNodeId}");
        mermaid.AppendLine($"    class {stmtNodeId} stmt");

        // Special handling for specific statement types
        if (stmtType == "LoopStmt")
        {
            // Add loop body node
            string loopBodyNodeId = $"{stmtNodeId}_body";
            mermaid.AppendLine($"    {loopBodyNodeId}[\"Loop Body\"]");
            mermaid.AppendLine($"    {stmtNodeId} --> {loopBodyNodeId}");

            // Add condition node
            string conditionNodeId = $"{stmtNodeId}_condition";
            mermaid.AppendLine($"    {conditionNodeId}[\"Condition\"]");
            mermaid.AppendLine($"    {stmtNodeId} --> {conditionNodeId}");
        }
        else if (stmtType == "ConditionalStmt")
        {
            // Add condition node
            string conditionNodeId = $"{stmtNodeId}_condition";
            mermaid.AppendLine($"    {conditionNodeId}[\"Condition\"]");
            mermaid.AppendLine($"    {stmtNodeId} --> {conditionNodeId}");

            // Add then branch
            string thenNodeId = $"{stmtNodeId}_then";
            mermaid.AppendLine($"    {thenNodeId}[\"Then Branch\"]");
            mermaid.AppendLine($"    {stmtNodeId} --> {thenNodeId}");

            // Add else branch if present
            string elseNodeId = $"{stmtNodeId}_else";
            mermaid.AppendLine($"    {elseNodeId}[\"Else Branch\"]");
            mermaid.AppendLine($"    {stmtNodeId} --> {elseNodeId}");
        }
    }

// Helper method to sanitize strings for Mermaid diagram IDs
    private string SanitizeForMermaid(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "empty";

        // Replace characters that would break Mermaid syntax
        return input
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace(">", "To")
            .Replace("<", "From")
            .Replace(".", "_dot_")
            .Replace(":", "_colon_")
            .Replace(",", "_comma_")
            .Replace(";", "_semicolon_")
            .Replace("(", "_openParen_")
            .Replace(")", "_closeParen_")
            .Replace("[", "_openBracket_")
            .Replace("]", "_closeBracket_")
            .Replace("{", "_openBrace_")
            .Replace("}", "_closeBrace_")
            .Replace("\"", "_quote_")
            .Replace("'", "_apostrophe_")
            .Replace("/", "_slash_")
            .Replace("\\", "_backslash_")
            .Replace("=", "_equals_")
            .Replace("+", "_plus_")
            .Replace("-", "_minus_")
            .Replace("*", "_star_")
            .Replace("&", "_amp_")
            .Replace("^", "_caret_")
            .Replace("%", "_percent_")
            .Replace("$", "_dollar_")
            .Replace("#", "_hash_")
            .Replace("@", "_at_")
            .Replace("!", "_exclamation_")
            .Replace("?", "_question_");
    }

// Helper method to sanitize content for Mermaid node labels
    private string SanitizeContentForMermaid(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input
            .Replace("\"", "'")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

// Method to generate a PNG from Mermaid diagram
    public async Task SaveMermaidAsPngAsync(string outputPath)
    {
        string mermaidCode = ToMermaid();
        await GenerateMermaidPngAsync(mermaidCode, outputPath);
    }

// Helper method to use a Mermaid rendering service to generate a PNG
    private async Task GenerateMermaidPngAsync(string mermaidCode, string outputPath)
    {
        // This requires installing the NuGet package: System.Net.Http and System.Net.Http.Json
        using var httpClient = new System.Net.Http.HttpClient();

        try
        {
            // Option 1: Using the mermaid.ink service
            string encodedDiagram = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(mermaidCode))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            string url = $"https://mermaid.ink/img/{encodedDiagram}?type=png";
            byte[] imageBytes = await httpClient.GetByteArrayAsync(url);
            await System.IO.File.WriteAllBytesAsync(outputPath, imageBytes);

            /*
            // Option 2: Using the Kroki API (alternative if mermaid.ink doesn't work)
            var payload = new
            {
                diagram_source = mermaidCode,
                diagram_type = "mermaid",
                output_format = "png"
            };

            var response = await httpClient.PostAsJsonAsync("https://kroki.io/", payload);
            response.EnsureSuccessStatusCode();

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            await System.IO.File.WriteAllBytesAsync(outputPath, imageBytes);
            */
        }
        catch (Exception ex)
        {
            // If web service fails, save the Mermaid code to a text file
            await System.IO.File.WriteAllTextAsync(
                System.IO.Path.ChangeExtension(outputPath, ".mmd"),
                mermaidCode);

            throw new Exception(
                $"Failed to generate PNG. Mermaid code saved to {System.IO.Path.ChangeExtension(outputPath, ".mmd")}",
                ex);
        }
    }
}