using System.Collections.Generic;
using pipe.Shells;

namespace pipe.test.TestDoubles
{
    public class SpyCommandLineExecutor : ICommandLineExecutor
    {
        public string executedShell;
        public List<string> executedArguments = new List<string>();
            
        public void Execute(string shell, string arguments)
        {
            executedShell = shell;
            executedArguments.Add(arguments);
        }
    }
}