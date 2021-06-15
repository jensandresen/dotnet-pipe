using System;

namespace pipe.Shells
{
    public sealed class Command
    {
        private readonly Func<string, string> _prepper;

        public Command(string shell, Func<string, string> prepper)
        {
            Shell = shell;
            _prepper = prepper;
        }

        public string Shell { get; }

        public string PrepareArguments(string action)
        {
            return _prepper(action);
        }
    }
}