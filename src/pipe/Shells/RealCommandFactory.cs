using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace pipe.Shells
{
    public class RealCommandFactory : ICommandFactory
    {
        private readonly IOperatingSystemTypeProvider _operatingSystemTypeProvider;

        public RealCommandFactory(IOperatingSystemTypeProvider operatingSystemTypeProvider)
        {
            _operatingSystemTypeProvider = operatingSystemTypeProvider;
        }

        public string GetDefaultOSShell()
        {
            var os = _operatingSystemTypeProvider.Get();
            switch (os)
            {
                case OperatingSystemType.Windows:
                    return "powershell";
                case OperatingSystemType.Linux:
                    return "sh";
                case OperatingSystemType.Mac:
                    return "bash";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private Command CreateCustomCommand(string name)
        {
            var match = Regex.Match(name, @"^\!(?<exe>.*?),\s*(?<args>.*?)$");
            var definedExe = match.Groups["exe"].Value;
            var definedArgs = match.Groups["args"].Value;

            const string argsPlaceholder = "<args>";
            Func<string, string> replacer = input => definedArgs.Replace(argsPlaceholder, input);
            
            var argMatches = Regex.Match(definedArgs, @"^(?<replacement>\[.=.*?\])*(?<args>.*?)$");
            if (argMatches.Groups["replacement"].Success)
            {
                replacer = input =>
                {
                    var alteredInput = input;
                    foreach (Capture capture in argMatches.Groups["replacement"].Captures)
                    {
                        var temp = Regex.Match(capture.Value, @"^\[(?<key>.)=(?<value>.*?)\]$");
                        alteredInput = alteredInput.Replace(temp.Groups["key"].Value, temp.Groups["value"].Value);
                    }

                    return argMatches.Groups["args"].Value.Replace(argsPlaceholder, alteredInput);
                };
            }

            return new Command(definedExe, replacer);
        }
        
        public Command Create(string name)
        {
            if (name == "?")
            {
                name = GetDefaultOSShell();
            }

            if (name.StartsWith("!"))
            {
                return CreateCustomCommand(name);
            }
            
            switch (name)
            {
                case "sh":
                    return new Command("sh", action => $"-c \"{action.Replace("\"", "\\\"")}\"");
                case "bash":
                    return new Command("bash", action => $"-c \"{action.Replace("\"", "\\\"")}\"");
                case "pwsh":
                case "powershell":
                    return new Command(
                        shell: _operatingSystemTypeProvider.Get() == OperatingSystemType.Windows
                            ? "powershell"
                            : "pwsh",
                        prepper: action => $"-Command \"& {{ {action.Replace("\"", "`\"")} }}\""
                    );
                default:
                    throw new ArgumentException($"Unknown shell with name {name}.");
            }
        }
    }
}