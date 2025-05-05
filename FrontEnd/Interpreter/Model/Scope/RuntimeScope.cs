using System.Diagnostics;
using KukuLang.Parser.Models.CustomTask;
using KukuLang.Parser.Models.CustomType;

namespace KukuLang.Interpreter.Model.Scope
{
    public class RuntimeScope(Dictionary<string, CustomTypeBase> declaredTypes,
        Dictionary<string, CustomTaskBase> declaredTasks, RuntimeScope? parentScope) : IDisposable
    {
        public Dictionary<string, CustomTypeBase> DeclaredTypes { get; } = declaredTypes;
        public Dictionary<string, CustomTaskBase> DeclaredTasks { get; } = declaredTasks;
        public Dictionary<string, RuntimeObj.RuntimeObj> CreatedObjects { get; } = [];
        public RuntimeScope? ParentScope { get; } = parentScope;

        public void UpdateScopeVariable(string varName, RuntimeObj.RuntimeObj instance)
        {
            Stack<RuntimeScope> stack = new();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var currentScope = stack.Pop();
                if (currentScope.CreatedObjects.TryGetValue(varName, out RuntimeObj.RuntimeObj? value))
                {
                    if (value.RuntimeObjType != instance.RuntimeObjType)
                    {
                        throw new Exception($"{varName} is of type {value.RuntimeObjType} but attemped to assign type {instance.RuntimeObjType}");
                    }
                    currentScope.CreatedObjects[varName] = instance;
                    Console.WriteLine($"Updating variable {varName} : {instance.Val}");
                    return;
                }
                if (currentScope.ParentScope != null)
                {
                    stack.Push(currentScope.ParentScope);
                }
            }

            CreatedObjects.Add(varName, instance);
        }

        public RuntimeObj.RuntimeObj? GetVariable(string name)
        {
            if (CreatedObjects.TryGetValue(name, out RuntimeObj.RuntimeObj? value))
                return value;
            if (ParentScope != null)
            {
                return ParentScope.GetVariable(name);
            }
            return null;
        }

        public CustomTypeBase? GetCustomType(string typeName)
        {
            if (DeclaredTypes.TryGetValue(typeName, out CustomTypeBase? value))
            {
                return value;
            }
            if (ParentScope != null)
            {
                return ParentScope.GetCustomType(typeName);
            }
            return null;
        }

        public CustomTaskBase? GetCustomTask(string taskName)
        {
            if (DeclaredTasks.TryGetValue(taskName, out CustomTaskBase? value))
            {
                return value;
            }
            if (ParentScope != null)
            {
                return ParentScope.GetCustomTask(taskName);
            }
            return null;
        }

        public override string ToString()
        {
            var declaredTypesStr = string.Join(", ", DeclaredTypes.Select(kv => $"{kv.Key}: {kv.Value}"));
            var declaredTasksStr = string.Join(", ", DeclaredTasks.Select(kv => $"{kv.Key}: {kv.Value}"));
            var createdObjectsStr = string.Join(", ", CreatedObjects.Select(kv => $"{kv.Key}: {kv.Value}"));

            return $"RuntimeScope(DeclaredTypes: [{declaredTypesStr}], DeclaredTasks: [{declaredTasksStr}], CreatedObjects: [{createdObjectsStr}])";
        }

        public void Dispose()
        {
            Debug.WriteLine($"Disposing scope {this}");
            // Release managed resources here if needed
            DeclaredTypes.Clear();
            DeclaredTasks.Clear();
            CreatedObjects.Clear();
        }

        ~RuntimeScope()
        {
            Debug.WriteLine($"Destroying scope {this}");
            ((IDisposable)this).Dispose();
        }
    }
}
