using System.Collections.Generic;
using System.Text.RegularExpressions;
using pipe.Exceptions;

namespace pipe
{
    public class VariableHelper : IVariableHelper
    {
        private readonly IEnvironmentVariableProvider _environmentVariableProvider;

        public VariableHelper(IEnvironmentVariableProvider environmentVariableProvider)
        {
            _environmentVariableProvider = environmentVariableProvider;
        }
        
        private string ExpandVariableInSingleVariable(string variableName, Dictionary<string, string> variables, HashSet<string> alreadyVisited = null)
        {
            if (!variables.TryGetValue(variableName, out var variableValue))
            {
                throw new VariableNotDeclaredException($"The variable \"{variableName}\" has not been declared.");
            }
            
            var matches = Regex.Matches(variableValue, "\\$\\((?<name>.+?)\\)");
                
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var name = match.Groups["name"].Value;
                    // call recurse
                    if (alreadyVisited == null)
                    {
                        alreadyVisited = new HashSet<string>();
                    }

                    if (alreadyVisited.Contains(name))
                    {
                        throw new CircularVariableReferenceException($"Error! Circular reference in variables detected. Circle starts in variable \"{variableName}\" which references \"{match.Value}\"");
                    }
                    
                    alreadyVisited.Add(variableName);
                    var expandedValue = ExpandVariableInSingleVariable(name, variables, alreadyVisited);

                    variableValue = variableValue.Replace($"$({name})", expandedValue);
                }
            }

            return variableValue;
        }
        
        private Dictionary<string, string> ExpandVariablesInVariables(Dictionary<string, string> variables)
        {
            foreach (var key in variables.Keys)
            {
                variables[key] = ExpandVariableInSingleVariable(key, variables);
            }

            return variables;
        }
        
        public string ExpandVariables(Dictionary<string,string> variables, string action)
        {
            var finalVariables = ExpandVariablesInVariables(variables);
            foreach (var (key, value) in finalVariables)
            {
                action = action.Replace($"$({key})", value);
            }

            // expand environment variables
            var matches = Regex.Matches(action, "\\$\\{(?<name>.+?)\\}");
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var envVarName = match.Groups["name"]?.Value;
                    var envVarValue = _environmentVariableProvider.Get(envVarName);

                    if (!string.IsNullOrWhiteSpace(envVarValue))
                    {
                        action = action.Replace($"${{{envVarName}}}", envVarValue);
                    }
                }
            }

            return action;
        }
    }
}