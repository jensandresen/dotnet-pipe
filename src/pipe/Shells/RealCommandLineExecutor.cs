using System.Diagnostics;

namespace pipe.Shells
{
    public class RealCommandLineExecutor : ICommandLineExecutor
    {
        public void Execute(string shell, string arguments)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = shell;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.Start();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new CommandLineExecutorException($"Executing {shell} {arguments} exited with non-zero exitcode of {process.ExitCode}.");
                }
            }
        }
    }
}