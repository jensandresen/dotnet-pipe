using System.Collections.Generic;
using System.IO;
using System.Linq;
using pipe.Exceptions;
using pipe.Shells;

namespace pipe
{
    public class Engine
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandLineExecutor _commandLineExecutor;
        private readonly IVariableHelper _variableHelper;

        public Engine(IFileSystem fileSystem, ICommandFactory commandFactory, ICommandLineExecutor commandLineExecutor,
            IVariableHelper variableHelper)
        {
            _fileSystem = fileSystem;
            _commandFactory = commandFactory;
            _commandLineExecutor = commandLineExecutor;
            _variableHelper = variableHelper;
        }
        
        public void Run(string[] args)
        {
            var (requestedSteps, variableOverrides, filePath) = CommandLineParser.Parse(args);
            if (requestedSteps.Length == 0 && variableOverrides.Length > 0)
            {
                throw new MissingRequiredStepException("Please specify a step when passing variable overrides.");
            }
            
            var pipelineFilePath =  filePath ?? _fileSystem.GetPathForLocalFile("pipe.line");
            if (!_fileSystem.DoesFileExists(pipelineFilePath))
            {
                throw new FileNotFoundException($"Unable to locate a pipeline file at {pipelineFilePath}");
            }
            
            var pipelineFile = ReadPipelineFileFrom(pipelineFilePath);
            var executionPipeline = BuildExecutionPipelineFrom(requestedSteps, pipelineFile);
            var finalVariables = BuildFinalVariablesFrom(variableOverrides, pipelineFile);
            var command = CreateCommand(finalVariables);

            foreach (var step in executionPipeline)
            {
                foreach (var action in step.Actions)
                {
                    var shell = command.Shell;

                    var finalAction = _variableHelper.ExpandVariables(finalVariables, action);
                    var arguments = command.PrepareArguments(finalAction);
                    
                    _commandLineExecutor.Execute(shell, arguments);
                }
            }
        }

        private Command CreateCommand(Dictionary<string, string> finalVariables)
        {
            var shell = finalVariables.GetValueOrDefault("SHELL", "?");
            return _commandFactory.Create(shell);
        }

        private static Dictionary<string, string> BuildFinalVariablesFrom(IEnumerable<KeyValuePair<string, string>> variables, PipelineFile pipelineFile)
        {
            var finalVariables = new Dictionary<string, string>(variables);
            foreach (var (key, value) in pipelineFile.Variables)
            {
                if (!finalVariables.ContainsKey(key))
                {
                    finalVariables.Add(key, value);
                }
            }

            return finalVariables;
        }

        private static IEnumerable<Step> BuildExecutionPipelineFrom(IEnumerable<string> requestedSteps, PipelineFile pipelineFile)
        {
            var executionPipeline = new LinkedList<Step>();
            
            foreach (var step in requestedSteps)
            {
                var existingStep = pipelineFile.Steps.SingleOrDefault(x => x.Name == step);
                if (existingStep == null)
                {
                    throw new StepNotDefinedException($"Error! Requested step \"{step}\" is not defined in the file.");
                }

                GenerateExecutionLineForStep(existingStep, pipelineFile, executionPipeline);
            }

            // default to first step defined in file if nothing has been specified on command line
            if (executionPipeline.Count == 0)
            {
                executionPipeline.AddLast(pipelineFile.Steps.First());
            }

            return executionPipeline;
        }
        
        private static void GenerateExecutionLineForStep(Step step, PipelineFile pipelineFile, LinkedList<Step> currentExecutionLine, ISet<string> pathsTaken = null)
        {
            if (pathsTaken == null)
            {
                pathsTaken = new HashSet<string>();
            }
            
            foreach (var preStepName in step.PreStepNames)
            {
                if (preStepName == step.Name)
                {
                    throw new StepSelfReferencesException($"Error! step \"{step.Name}\" references it self a a pre-step.");
                }
                
                var preStep = pipelineFile.Steps.SingleOrDefault(x => x.Name == preStepName);
                if (preStep == null)
                {
                    throw new StepNotDefinedException($"Error! step \"{step.Name}\" references pre-step \"{preStepName}\" which is not defined in the file.");
                }

                var path = string.Join("-->", step.Name, preStep.Name);
                if (pathsTaken.Contains(path))
                {
                    throw new StepCircularReferenceException($"Error! Circular step reference caused by \"{step.Name}\" referencing pre-step \"{preStep.Name}\" which ends up re-entroducing step \"{step.Name}\" and its pre-steps.");
                }

                pathsTaken.Add(path);
                
                GenerateExecutionLineForStep(preStep, pipelineFile, currentExecutionLine, pathsTaken);
            }

            currentExecutionLine.AddLast(step);
    
        }

        private PipelineFile ReadPipelineFileFrom(string pipelineFilePath)
        {
            var fileContent = _fileSystem.ReadFileContents(pipelineFilePath);
            return PipelineFile.Parse(fileContent);
        }
    }
}