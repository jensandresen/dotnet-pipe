using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace pipe
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pipe!");
            
            var (steps, variables, filePath) = CommandLineParser.Parse(args);
            if (steps.Length == 0 && variables.Length > 0)
            {
                Console.WriteLine("Please specify a step when passing variable overrides.");
                Environment.Exit(-1);
            }

            var pipelineFilePath =  filePath ?? Path.Combine(Environment.CurrentDirectory, "pipe.line");
            if (!File.Exists(pipelineFilePath))
            {
                Console.WriteLine($"Unable to locate a pipeline file at {pipelineFilePath}");
                Environment.Exit(-2);
            }

            var fileContent = File.ReadAllLines(pipelineFilePath);
            var pipelineFile = PipelineFile.Parse(fileContent);

            var executionPipeline = new LinkedList<Step>();
            foreach (var step in steps)
            {
                var existingStep = pipelineFile.Steps.SingleOrDefault(x => x.Name == step);
                if (existingStep == null)
                {
                    Console.WriteLine($"Error! Requested step \"{step}\" is not defined in file.");
                    Environment.Exit(-3);
                }

                executionPipeline.AddLast(existingStep);
            }

            // default to first step defined in file if nothing has been specified on command line
            if (executionPipeline.Count == 0)
            {
                executionPipeline.AddLast(pipelineFile.Steps.First());
            }           
            
            foreach (var step in executionPipeline)
            {
                foreach (var stepAction in step.Actions)
                {
                    Console.WriteLine(stepAction);

                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = "sh";
                        process.StartInfo.Arguments = $"-c (\"{stepAction}\")";
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.RedirectStandardOutput = false;
                        process.Start();

                        process.WaitForExit();
                    }
                }
            }
        }
    }
}
