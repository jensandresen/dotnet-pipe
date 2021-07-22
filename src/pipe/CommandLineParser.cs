using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using pipe.Exceptions;

namespace pipe
{
    public static class CommandLineParser
    {
        public static ParseResult Parse(string[] args)
        {
            var parsers = new ArgParser[]
            {
                new FileParser(),
                new VerbosityParser(),
                new VariableParser(),
                new StepParser(),
            };

            var result = ParseResult.Empty;
            var index = 0;
            
            while (index < args.Length)
            {
                var couldParse = false;

                foreach (var parser in parsers)
                {
                    if (parser.TryParse(args, index, out var tempResult, out var tempIndex))
                    {
                        couldParse = true;
                        result += tempResult;
                        index = tempIndex;

                        break;
                    }
                }

                if (!couldParse)
                {
                    throw new InvalidArgumentException($"Invalid argument \"{args[index]}\" or invalid placement of it as input on the command line.");
                }
            }

            return result;
        }

        public static ParseResult Parse_old(string[] args)
        {
            var steps = new LinkedList<string>();
            var index = 0;
            while (index<args.Length)
            {
                var a = args[index];
                if (a.Contains("="))
                {
                    index++;
                    continue;
                }

                if (a.StartsWith("-"))
                {
                    index += 2;
                    continue;
                }

                if (index > 0 && args[index-1].StartsWith("-f"))
                {
                    index += 1;
                    continue;
                }
                
                steps.AddLast(a);
                index++;
            }

            var variables = args
                .Where(x => x.Contains("="))
                .Select(x => x.Split("="))
                .Select(kv => new KeyValuePair<string, string>(kv[0], kv[1]))
                .ToArray();

            var filePath = (string) null;
            var isVerbose = false;
            
            for (var i = 0; i < args.Length; i++)
            {
                var element = args[i];
                if (element == "-f")
                {
                    if (i == args.Length-1)
                    {
                        throw new CommandLineParsingException("Error! Missing path to file.");
                    }
                    
                    filePath = args[i + 1];
                    if (filePath.Contains("="))
                    {
                        throw new CommandLineParsingException("Error! Path to file is a variable override.");
                    }
                }
                else if (element == "-v")
                {
                    isVerbose = true;
                }
            }
            
            return new ParseResult(steps.ToArray(), variables, filePath, isVerbose);
        }

        #region arg parsers

        private abstract class ArgParser
        {
            public abstract bool TryParse(string[] args, int argIndex, out ParseResult result, out int nextIndex);
        }
        
        private class VariableParser : ArgParser
        {
            public override bool TryParse(string[] args, int argIndex, out ParseResult result, out int nextIndex)
            {
                var arg = args[argIndex];

                if (arg.Contains("="))
                {
                    var kv = arg.Split("=");
                    result = new ParseResult(
                        variables: new []
                        {
                            new KeyValuePair<string, string>(kv[0], kv[1])
                        }
                    );

                    nextIndex = argIndex + 1;
                    
                    return true;
                }

                result = ParseResult.Empty;
                nextIndex = argIndex;
                
                return false;
            }
        }
        
        private class VerbosityParser : ArgParser
        {
            public override bool TryParse(string[] args, int argIndex, out ParseResult result, out int nextIndex)
            {
                var a = args[argIndex];

                if (a == "-v")
                {
                    result = new ParseResult(isVerbose: true);
                    nextIndex = argIndex + 1;
                    return true;
                }

                result = ParseResult.Empty;
                nextIndex = argIndex;
                return false;
            }
        }

        private class FileParser : ArgParser
        {
            public override bool TryParse(string[] args, int argIndex, out ParseResult result, out int nextIndex)
            {
                var a = args[argIndex];

                if (a == "-f")
                {
                    if (argIndex == args.Length-1)
                    {
                        throw new CommandLineParsingException("Error! Missing path to file.");
                    }
                    
                    var filePath = args[argIndex + 1];
                    if (filePath.Contains("="))
                    {
                        throw new CommandLineParsingException("Error! Path to file is a variable override.");
                    }

                    result = new ParseResult(filePath: filePath);
                    nextIndex = argIndex + 2;
                    return true;
                }

                result = ParseResult.Empty;
                nextIndex = argIndex;
                return false;
            }
        }

        private class StepParser : ArgParser
        {
            public override bool TryParse(string[] args, int argIndex, out ParseResult result, out int nextIndex)
            {
                var a = args[argIndex];

                if (!string.IsNullOrWhiteSpace(a) && !a.Contains("=") && !a.StartsWith("-"))
                {
                    if (argIndex > 0 && args[argIndex-1].StartsWith("-f"))
                    {
                        result = ParseResult.Empty;
                        nextIndex = argIndex;
                        return false;
                    }
                    
                    result = new ParseResult(steps: new[] {a});
                    nextIndex = argIndex + 1;
                    return true;
                }

                result = ParseResult.Empty;
                nextIndex = argIndex;
                return false;
            }
        }

        #endregion

        public class ParseResult
        {
            public static readonly ParseResult Empty = new ParseResult();
            
            public ParseResult(string[] steps = null, KeyValuePair<string, string>[] variables = null, string filePath = null, bool isVerbose = false)
            {
                Steps = steps ?? Array.Empty<string>();
                Variables = variables ?? Array.Empty<KeyValuePair<string, string>>();
                FilePath = filePath;
                IsVerbose = isVerbose;
            }

            public string[] Steps { get; }
            public KeyValuePair<string, string>[] Variables { get; }
            public string FilePath { get; }
            public bool IsVerbose { get; }

            #region operators

            public static ParseResult operator +(ParseResult left, ParseResult right)
            {
                var steps = Enumerable
                    .Concat(left.Steps, right.Steps)
                    .Distinct();

                var variables = new Dictionary<string, string>();
                foreach (var (k, v) in left.Variables)
                {
                    variables[k] = v;
                }
                foreach (var (k, v) in right.Variables)
                {
                    variables[k] = v;
                }

                var filePath = left.FilePath;
                if (!string.IsNullOrWhiteSpace(right.FilePath))
                {
                    filePath = right.FilePath;
                }

                var isVerbose = new[] {left.IsVerbose, right.IsVerbose}.Any(x => x == true);

                return new ParseResult(
                    steps: steps.ToArray(),
                    variables: variables.ToArray(),
                    filePath: filePath,
                    isVerbose: isVerbose
                );
            }

            #endregion
            
            public void Deconstruct(out string[] steps, out KeyValuePair<string, string>[] variables, out string filePath, out bool isVerbose)
            {
                steps = Steps;
                variables = Variables;
                filePath = FilePath;
                isVerbose = IsVerbose;
            }
        }
    }
}