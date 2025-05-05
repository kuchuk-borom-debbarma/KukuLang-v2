using FrontEnd.Parser.Models.Stmt;
using KukuLang.Interpreter.Model.RuntimeObj;
using KukuLang.Interpreter.Model.Scope;
using KukuLang.Parser.Models.Expressions;
using KukuLang.Parser.Models.Expressions.Literals;
using KukuLang.Parser.Models.Stmt;
using System.Diagnostics;

namespace KukuLang.Interpreter.Service
{
    public static class StatementProcessor
    {
        public static void ProcessStatement(StmtBase statement, RuntimeScope scope)
        {

            switch (statement)
            {
                case SetToStmt setToStmt:
                    ProcessSetToStatement(setToStmt, scope);
                    break;
                case IfStmt ifStmt:
                    ProcessIfStatement(ifStmt, scope);
                    break;
                case FunctionCallStmt funcCallStmt:
                    ProcessFunctionCallStatement(funcCallStmt, scope);
                    break;
                case LoopStmt loopStmt:
                    ProcessLoopStatement(loopStmt, scope);
                    break;
                case PrintStmt printStmt:
                    ProcessPrintStatement(printStmt, scope);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported statement type: {statement.GetType().Name}");
            }

        }

        private static void ProcessPrintStatement(PrintStmt printStmt, RuntimeScope scope)
        {
            var runtimeObj = ProcessExpressionStmt(printStmt.Expression, scope);
            Console.WriteLine(runtimeObj.Val);
        }

        private static void ProcessLoopStatement(LoopStmt loopStmt, RuntimeScope scope)
        {
            var conditionObj = ProcessExpressionStmt(loopStmt.Condition, scope);
            if (conditionObj == null || conditionObj.RuntimeObjType != "bool") throw new Exception("Invalid Conditional expression");
            RuntimeScope loopScope = new([], [], scope);
            if (loopStmt.IsUntil)
            {
                conditionObj.Val = !conditionObj.Val;
            }
            using (loopScope) //Destroys the loopScope once this block is not used
            {
                while (loopStmt.IsUntil ? !ProcessExpressionStmt(loopStmt.Condition, scope).Val : ProcessExpressionStmt(loopStmt.Condition, scope).Val)
                {
                    foreach (var s in loopStmt.Scope.Statements)
                    {
                        ProcessStatement(s, loopScope);
                    }
                }
            }


        }

        private static void ProcessSetToStatement(SetToStmt stmt, RuntimeScope scope)
        {
            var variableToSet = stmt.VariableToSet;
            var valueToSet = stmt.VarVal;
            /*
             * If the variable we are setting has no next node then
             * It is either a reference to an existing variable or creating a new variable
             */
            if (variableToSet.NextNode == null)
            {
                AssignSimpleVariable(variableToSet.VarName, valueToSet, scope);
            }
            /*
             * If it is nested then it is a sub variable or nested variable such as :-
             * "set kuku's age to 12." which basically translates to "kuku.age = 12;" in c#
             */
            else
            {
                AssignNestedVariable(variableToSet, valueToSet, scope);
            }
        }

        private static void AssignSimpleVariable(string variableName, ExpressionStmt value, RuntimeScope scope)
        {
            RuntimeObj? val = ProcessExpressionStmt(value, scope) ?? throw new Exception($"value of var {variableName} is null");
            var isUpdate = scope.GetVariable(variableName) != null; //Boolean to determine if we are creating or updating an existing variable value
            scope.UpdateScopeVariable(variableName, val);
            Debug.WriteLine($"{(isUpdate ? "Updated" : "Created")} variable '{variableName}' with value '{val}'");
        }

        private static void AssignNestedVariable(NestedVariableExp nestedVar, ExpressionStmt value, RuntimeScope scope)
        {
            //Get the root variable first.
            var currentVar = scope.GetVariable(nestedVar.VarName)
                                  ?? throw new Exception($"Variable not found: {nestedVar.VarName}");
            //parentVar is used to store the "currentVar" before updating "currentVar".
            //This is necessary because for nested variable such as (student.human.age) currentVal will end up at age and parentVar will end up at human.
            RuntimeObj? parentVar = currentVar;

            // Build the full path dynamically
            var fullPath = nestedVar.VarName;

            //Traverse the nestedNode and update variables
            while (nestedVar.NextNode != null)
            {
                nestedVar = nestedVar.NextNode;
                if (currentVar.Val is not Dictionary<string, RuntimeObj> nestedVars)
                {
                    throw new Exception($"Variable {currentVar.Val} is not type nested object");
                }
                if (!nestedVars.ContainsKey(nestedVar.VarName))
                {
                    throw new Exception($"Nested variable not found: {nestedVar.VarName}");

                }

                parentVar = currentVar; //Store currentVar in parentVar
                currentVar = nestedVars[nestedVar.VarName]; //Update currentVar by accessing dictionary

                // Append to the path
                fullPath += $".{nestedVar.VarName}";
            }

            var updatedValue = ProcessExpressionStmt(value, scope);
            if (updatedValue == null) throw new Exception($"value of var {updatedValue} is null");
            (parentVar.Val as Dictionary<string, RuntimeObj>)[nestedVar.VarName] = updatedValue;
            Debug.WriteLine($"Updated nested variable '{fullPath}' with value '{updatedValue}'");
        }


        /// <summary>
        /// Process expression statements which may or may not return an object.
        /// Expressions such as function call will not return any Runtime Object if it doesn't.
        /// </summary>
        private static RuntimeObj? ProcessExpressionStmt(ExpressionStmt exp, RuntimeScope scope)
        {
            return exp switch
            {
                IntLiteral intLiteral => new RuntimeObj((int)intLiteral.Val),
                FloatLiteral floatLiteral => new RuntimeObj((float)floatLiteral.Val),
                BoolLiteral boolLiteral => new RuntimeObj((bool)boolLiteral.Val),
                TextLiteral textLiteral => new RuntimeObj(textLiteral.Val),
                NestedVariableExp nestedVar => ResolveNestedVariable(nestedVar, scope),
                FuncCallExp funcCallExp => ResolveFunctionCallExp(funcCallExp, scope),
                BinaryExp binaryExp => ResolveBinaryExp(binaryExp, scope),
                InputExp => ResolveInputExp(),
                _ => throw new NotSupportedException($"Unsupported expression type: {exp.GetType().Name}")
            }; ;
        }

        private static RuntimeObj? ResolveInputExp()
        {
            dynamic? val = Console.ReadLine();
            if (val == null) throw new Exception("Invalid input");

            //attempt to cast to a float
            if (float.TryParse(val, out float res))
            {
                int i = (int)res;
                if (i == res)
                {
                    return new RuntimeObj(i);
                }
                return new RuntimeObj((float)res);
            }
            //attempt to cast to an int
            if (int.TryParse(val, out int j))
            {
                return new RuntimeObj(j);
            }
            if (bool.TryParse(val, out bool b))
            {
                return new RuntimeObj(b);
            }
            return new RuntimeObj((string)val);

        }

        private static RuntimeObj? ResolveBinaryExp(BinaryExp binaryExp, RuntimeScope scope)
        {
            RuntimeObj? leftObj = ProcessExpressionStmt(binaryExp.Left, scope);
            RuntimeObj? rightObj = ProcessExpressionStmt(binaryExp.Right, scope);

            if (leftObj == null || rightObj == null)
            {
                throw new NotSupportedException($"left and/or right binary exp is invalid {binaryExp}");
            }
            //TODO: Update logic to calculate type dynamically
            switch (binaryExp.Op)
            {
                case "+":
                    return new RuntimeObj(leftObj.Val + rightObj.Val);
                case "-":
                    return new RuntimeObj(leftObj.Val - rightObj.Val);
                case "*":
                    return new RuntimeObj(leftObj.Val * rightObj.Val);
                case "/":
                    return new RuntimeObj(leftObj.Val / rightObj.Val);
                case "%":
                    return new RuntimeObj(leftObj.Val % rightObj.Val);
                case "is":
                    return new RuntimeObj(leftObj.Val == rightObj.Val);
                case "is_not":
                    return new RuntimeObj(leftObj.Val != rightObj.Val);
                case "is_greater_than":
                    return new RuntimeObj(leftObj.Val > rightObj.Val);
                case "is_greater_or_is":
                    return new RuntimeObj(leftObj.Val >= rightObj.Val);
                case "is_less_than":
                    return new RuntimeObj(leftObj.Val < rightObj.Val);
                case "is_less_or_is":
                    return new RuntimeObj(leftObj.Val <= rightObj.Val);
                case "and":
                    return new RuntimeObj(leftObj.Val && rightObj.Val);
                case "or":
                    return new RuntimeObj(leftObj.Val || rightObj.Val);
            }
            return null;
        }

        private static RuntimeObj? ResolveFunctionCallExp(FuncCallExp funcCallExp, RuntimeScope scope)
        {
            //Get task in scope
            var task = scope.GetCustomTask(funcCallExp.FunctionName) ?? throw new Exception($"Task {funcCallExp.FunctionName} not found");

            //Validate if all params exist and their types are valid, add them to dictionary too.
            Dictionary<string, RuntimeObj> paramObj = [];
            if (task.ParamNameParamTypePair != null)
            {
                if (funcCallExp.ParamAndValPair == null) throw new Exception("Invalid Task parameters");
            }
            if (task.ParamNameParamTypePair != null && task.ParamNameParamTypePair.Count != funcCallExp.ParamAndValPair.Count) throw new Exception("Invalid Task parameters Count");
            if (task.ParamNameParamTypePair != null)
                foreach (var kv in task.ParamNameParamTypePair)
                {
                    string paramName = kv.Key;
                    if (!funcCallExp.ParamAndValPair.TryGetValue(paramName, out ExpressionStmt? value)) throw new Exception($"Function call {funcCallExp.FunctionName} Missing param {paramName}");
                    var runtimeObj = ProcessExpressionStmt(value, scope);
                    if (runtimeObj.RuntimeObjType != kv.Value)
                    {
                        //if the type required is text we can cast the runtimObj.val
                        throw new Exception($"Param {kv.Key} requires type {kv.Value} but got {runtimeObj.RuntimeObjType} val : {runtimeObj.Val}");

                    }
                    paramObj.Add(paramName, runtimeObj);
                }
            var statements = task.TaskScope;
            var funcScope = new RuntimeScope([], [], scope);
            //Add param as variables to the funcScope
            foreach (var item in paramObj)
            {
                funcScope.CreatedObjects.Add(item.Key, item.Value);
            }
            Debug.WriteLine($"Executing Task {funcCallExp.ToString(0)}");
            using (funcScope) //Ensures that the scope is destroyed once used.
            {
                foreach (var s in statements.Statements)
                {
                    if (s is ReturnStmt returnStmt)
                    {
                        var returnObject = ProcessExpressionStmt(returnStmt.Expression, funcScope);
                        if (returnObject.RuntimeObjType != task.TaskReturnType) throw new Exception($"Function needs to return {task.TaskReturnType} but returned {returnObject.RuntimeObjType}");
                        return returnObject;
                    }
                    else
                    {
                        ProcessStatement(s, funcScope);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Process all sorts of identifiers and identifier chains
        /// </summary>
        /// <param name="nestedVar"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static RuntimeObj? ResolveNestedVariable(NestedVariableExp nestedVar, RuntimeScope scope)
        {
            /*
             * If nestedVar is null it is either an existing variable, creating new runtime object of that type or a function call
             */
            if (nestedVar.NextNode == null)
            {
                //Check if variable exist
                RuntimeObj? varRef = scope.GetVariable(nestedVar.VarName);
                if (varRef != null)
                {
                    return varRef;
                }

                //if not existing var then try to see if its creation of new custom type
                var type = scope.GetCustomType(nestedVar.VarName);
                if (type != null)
                {
                    return CustomTypeHelper.CreateObjectFromCustomType(scope.GetCustomType(nestedVar.VarName), scope);
                }
                throw new Exception(nestedVar.VarName + " is not an existing variable nor a custom type");
            }
            /*
             * If it is nested then it has to be a reference to existing nested variable.
             */
            var currentVar = scope.GetVariable(nestedVar.VarName)
                              ?? throw new Exception($"Variable not found: {nestedVar.VarName}");

            //Traversing the nested variable
            while (nestedVar.NextNode != null)
            {
                nestedVar = nestedVar.NextNode;
                var nestedVars = currentVar.Val as Dictionary<string, RuntimeObj> ?? throw new Exception($"Variable {currentVar.Val} is not type nested object");
                if (!nestedVars.TryGetValue(nestedVar.VarName, out currentVar))
                {
                    throw new Exception($"Nested variable not found: {nestedVar.VarName}");
                }
            }

            return currentVar;
        }

        private static void ProcessIfStatement(IfStmt stmt, RuntimeScope scope)
        {
            // Implementation for handling If statements
            RuntimeObj? condition = ProcessExpressionStmt(stmt.Condition, scope);
            if (condition == null || condition.RuntimeObjType != "bool")
            {
                throw new Exception($"Invalid condition for statement {stmt}");
            }
            List<StmtBase> scopeStmt = [];
            if (condition.Val == true)
            {
                scopeStmt = stmt.Scope.Statements;
            }
            else if (stmt.ElseScope != null)
            {
                scopeStmt = stmt.ElseScope.Statements;
            }

            RuntimeScope ifScope = new([], [], scope);
            using (ifScope) //Will destroy the scope once used
            {
                foreach (var statement in scopeStmt)
                {
                    if (statement is ReturnStmt)
                    {
                        return;
                    }
                    ProcessStatement(statement, ifScope);
                }
            }
        }

        /// <summary>
        /// Handles function call and disregards it's returned value if any.
        /// </summary>
        /// <param name="stmt"></param>
        /// <param name="scope"></param>
        /// <exception cref="Exception"></exception>
        private static void ProcessFunctionCallStatement(FunctionCallStmt stmt, RuntimeScope scope)
        {
            //Check if function exists
            _ = scope.GetCustomTask(stmt.FunctionName) ?? throw new Exception($"Function {stmt.FunctionName} not found");
            var functionExp = stmt.FunctionExp;
            ProcessExpressionStmt(functionExp, scope); //Call the function.
        }
    }
}
