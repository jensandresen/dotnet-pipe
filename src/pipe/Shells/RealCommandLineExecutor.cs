using System.Diagnostics;

namespace pipe.Shells
{
    public class RealCommandLineExecutor : ICommandLineExecutor
    {
        private readonly ILogger _logger;

        public RealCommandLineExecutor(ILogger logger)
        {
            _logger = logger;
        }
        
        public void Execute(string shell, string arguments)
        {
            _logger.Log($"Executing: {shell} {arguments}...");
            _logger.Log("");
            _logger.Log("Output:");
            _logger.Log("");
            
            using (var process = new Process())
            {
                process.StartInfo.FileName = shell;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.RedirectStandardOutput = false;

                var stopwatch = Stopwatch.StartNew();

                process.Start();
                process.WaitForExit();

                _logger.Log("");
                _logger.Log($"Exit code: {process.ExitCode} - Elapsed time: {stopwatch.Elapsed}");
                
                if (process.ExitCode != 0)
                {
                    throw new CommandLineExecutorException($"Executing {shell} {arguments} exited with non-zero exitcode of {process.ExitCode}.");
                }
            }
        }
    }
}