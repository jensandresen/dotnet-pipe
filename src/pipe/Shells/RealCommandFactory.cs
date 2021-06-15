using System;

namespace pipe.Shells
{
    public class RealCommandFactory : ICommandFactory
    {
        private readonly IOperatingSystemTypeProvider _operatingSystemTypeProvider;

        public RealCommandFactory(IOperatingSystemTypeProvider operatingSystemTypeProvider)
        {
            _operatingSystemTypeProvider = operatingSystemTypeProvider;
        }
        
        private string GetDefaultOSShell()
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
        
        public Command Create(string name)
        {
            if (name == "?")
            {
                name = GetDefaultOSShell();
            }
            
            switch (name)
            {
                case "sh":
                    return new Command("sh", action => $"-c (\"{action}\")");
                case "bash":
                    return new Command("bash", action => $"-c (\"{action}\")");
                case "pwsh":
                case "powershell":
                    return new Command(
                        shell: _operatingSystemTypeProvider.Get() == OperatingSystemType.Windows
                            ? "powershell"
                            : "pwsh",
                        prepper: action => $"-Command \"& {{ {action} }}\""
                    );
                default:
                    throw new ArgumentException($"Unknown shell with name {name}.");
            }
        }
    }
}