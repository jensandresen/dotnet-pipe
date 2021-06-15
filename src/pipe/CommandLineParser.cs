using System.Collections.Generic;
using System.Linq;
using pipe.Exceptions;

namespace pipe
{
    public static class CommandLineParser
    {
        public static ParseResult Parse(string[] args)
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

                steps.AddLast(a);
                index++;
            }

            var variables = args
                .Where(x => x.Contains("="))
                .Select(x => x.Split("="))
                .Select(kv => new KeyValuePair<string, string>(kv[0], kv[1]))
                .ToArray();

            var filePath = (string) null;
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
            }
            
            return new ParseResult(steps.ToArray(), variables, filePath);
        }

        public record ParseResult(string[] Steps, KeyValuePair<string, string>[] Variables, string FilePath);
    }
}