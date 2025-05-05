using KukuLang.Interpreter.Model.RuntimeObj;
using KukuLang.Interpreter.Model.Scope;
using System.Diagnostics;
using KukuLang.Parser.Models.CustomType;

namespace KukuLang.Interpreter.Service
{
    public static class CustomTypeHelper
    {
        public static RuntimeObj CreateObjectFromCustomType(CustomTypeBase customType, RuntimeScope scope)
        {
            var variables = new Dictionary<string, RuntimeObj>();

            foreach (var (varName, varType) in customType.VarNameVarTypePair)
            {
                RuntimeObj runtimeObj = varType switch
                {
                    "int" => new RuntimeObj(0),
                    "float" => new RuntimeObj(0.0f),
                    "bool" => new RuntimeObj(false),
                    "text" => new RuntimeObj(string.Empty),
                    "list" => new RuntimeObj(new List<dynamic>()),
                    _ => CreateObjectFromCustomType(scope.GetCustomType(varType), scope)
                };

                variables[varName] = runtimeObj;
                Debug.WriteLine($"Creating variable object '{varName}' of type '{varType}' in custom type '{customType.TypeName}'");
            }

            Debug.WriteLine($"Created custom object of type '{customType.TypeName}' with variables: {string.Join(", ", variables.Keys)}");

            return new RuntimeObj(customType.TypeName, variables);
        }
    }
}
