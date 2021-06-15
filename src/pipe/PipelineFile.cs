using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace pipe
{
    public class PipelineFile
    {
        private PipelineFile(IEnumerable<Step> steps, IReadOnlyDictionary<string, string> variables)
        {
            Steps = steps;
            Variables = variables;
        }
        
        public IEnumerable<Step> Steps { get; }
        public IReadOnlyDictionary<string, string> Variables { get; }

        private static bool TryParseAction(string text, out string action)
        {
            var actionMatch = Regex.Match(text, @"^\s+(?<action>.*?)$");
            if (!actionMatch.Success)
            {
                action = null;
                return false;
            }

            action = actionMatch.Groups["action"].Value;
            return true;
        }

        private static LinkedList<string> JoinMultilineActions(LinkedList<string> actions)
        {
            var result = new LinkedList<string>();

            var joinedAction = "";
            var cur = actions.First;
            while (cur != null)
            {
                if (cur.Value.EndsWith(@"\"))
                {
                    joinedAction += cur.Value.TrimEnd('\\');
                }
                else
                {
                    joinedAction += cur.Value;
                    result.AddLast(joinedAction);

                    joinedAction = "";
                }
                
                cur = cur.Next;
            }

            return result;
        }

        private static bool TryParseStep(string[] lines, ref int index, out Step step)
        {
            var line = lines[index];
            var stepMatch = Regex.Match(line, @"^(?<name>\w+)\:\s*(?<potential_pre_step_names>.*?)?$");

            if (!stepMatch.Success)
            {
                step = null;
                return false;
            }
            
            step = new Step(stepMatch.Groups["name"].Value);
            
            var actions = new LinkedList<string>();
            while (index < lines.Length-1)
            {
                if (TryParseAction(lines[index+1], out var action))
                {
                    actions.AddLast(action);
                    index++;
                }
                else
                {
                    break;
                }
            }
            
            foreach (var action in JoinMultilineActions(actions))
            {
                step.AddAction(action);
            }

            var preStepNames = stepMatch.Groups["potential_pre_step_names"].Value
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();
            
            foreach (var preStepName in preStepNames)
            {
                step.AddPreStepName(preStepName);
            }
            
            return true;
        }

        private static bool TryParseVariable(string[] lines, ref int index, out KeyValuePair<string, string> variable)
        {
            var line = lines[index];
            var variableMatch = Regex.Match(line, @"^(?<key>[a-zA-Z0-9\-_]+)\s*=\s*(?<value>.*?)\s*(#.*?)?$");

            if (!variableMatch.Success)
            {
                variable = default;
                return false;
            }

            variable = new KeyValuePair<string, string>(
                key: variableMatch.Groups["key"].Value,
                value: variableMatch.Groups["value"].Value.Trim('"')
            );
            
            return true;
        }

        public static PipelineFile Parse(string[] text)
        {
            var lines = text
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !Regex.IsMatch(x, @"^\s+#"))
                .ToArray();

            var variables = new Dictionary<string, string>();
            var steps = new LinkedList<Step>();

            var index = 0;
            while (index < lines.Length)
            {
                if (TryParseVariable(lines, ref index, out var variable))
                {
                    variables[variable.Key] = variable.Value;
                }
                
                if (TryParseStep(lines, ref index, out var step))
                {
                    steps.AddLast(step);
                }

                index++;
            }
            
            return new PipelineFile(steps, variables);
        }
    }
}