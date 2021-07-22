using System;
using pipe.Shells;

namespace pipe
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new RealCommandLineLogger();
            
            var engine = new Engine(
                fileSystem: new RealFileSystem(),
                commandFactory: new RealCommandFactory(new RealOperatingSystemTypeProvider()),
                commandLineExecutor: new RealCommandLineExecutor(logger),
                variableHelper: new VariableHelper(new RealEnvironmentVariableProvider()),
                logger: logger,
                splashScreen: new RealSplashScreen(logger)
            );

            try
            {
                engine.Run(args);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Environment.Exit(-1);
            }
        }
    }
}
