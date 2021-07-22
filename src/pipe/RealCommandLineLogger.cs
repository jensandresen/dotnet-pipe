using System;

namespace pipe
{
    public class RealCommandLineLogger : ILogger
    {
        private bool _isVerbose = false;
        
        public void EnableVerbosity()
        {
            _isVerbose = true;
        }

        public void Log(string message)
        {
            if (_isVerbose)
            {
                var defaultColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine(message);
                
                Console.ForegroundColor = defaultColor;
            }
        }

        public void LogHeadline(string message)
        {
            if (_isVerbose)
            {
                var defaultForegroudColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                
                Console.WriteLine(message);
                
                Console.ForegroundColor = defaultForegroudColor;
            }
        }
    }
}