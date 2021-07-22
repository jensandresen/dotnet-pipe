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
        private readonly ILogger _logger;
        private readonly ISplashScreen _splashScreen;

        public Engine(IFileSystem fileSystem, ICommandFactory commandFactory, ICommandLineExecutor commandLineExecutor,
            IVariableHelper variableHelper, ILogger logger, ISplashScreen splashScreen)
        {
            _fileSystem = fileSystem;
            _commandFactory = commandFactory;
            _commandLineExecutor = commandLineExecutor;
            _variableHelper = variableHelper;
            _logger = logger;
            _splashScreen = splashScreen;
        }
        
        public void Run(string[] args)
        {
            var (requestedSteps, variableOverrides, filePath, isVerbose) = CommandLineParser.Parse(args);
            if (isVerbose)
            {
                _logger.EnableVerbosity();
            }
            
            _splashScreen.Show();
            
            if (requestedSteps.Length == 0 && variableOverrides.Length > 0)
            {
                throw new MissingRequiredStepException("Please specify a step when passing variable overrides.");
            }
            
            _logger.LogHeadline("Input:");
            _logger.Log($"Requested steps:");
            _logger.Log($"  {string.Join(" -> ", requestedSteps)}");
            _logger.Log("Variable overrides:");
            foreach (var (k, v) in variableOverrides)
            {
                _logger.Log($"  {string.Join("=", k, v)}");
            }
            _logger.Log($"Pipeline file override: '{filePath}'");
            _logger.Log("");
            
            _logger.LogHeadline("Pipeline file:");
            var pipelineFilePath =  filePath; // ?? _fileSystem.GetPathForLocalFile("pipe.line");

            if (!string.IsNullOrWhiteSpace(pipelineFilePath))
            {
                _logger.Log($"Looking for pipeline file at '{pipelineFilePath}'");
                if (!_fileSystem.DoesFileExists(pipelineFilePath))
                {
                    throw new FileNotFoundException($"Unable to locate a pipeline file at {pipelineFilePath}");
                }
            }
            else
            {
                var defaultFilePaths = new[]
                {
                    _fileSystem.GetPathForLocalFile("Pipeline"), 
                    _fileSystem.GetPathForLocalFile("pipeline"), 
                };
                
                pipelineFilePath = defaultFilePaths.FirstOrDefault(path => _fileSystem.DoesFileExists(path));
                if (string.IsNullOrWhiteSpace(pipelineFilePath))
                {
                    throw new FileNotFoundException($"Unable to locate a pipeline file at default locations {string.Join(" or ", defaultFilePaths)}");
                }

                _logger.Log($"Looking for pipeline file at '{pipelineFilePath}'");
            }
            
            _logger.Log("");
            
            var pipelineFile = ReadPipelineFileFrom(pipelineFilePath);
            var executionPipeline = BuildExecutionPipelineFrom(requestedSteps, pipelineFile).ToArray();
            var finalVariables = BuildFinalVariablesFrom(variableOverrides, pipelineFile);
            var command = CreateCommand(finalVariables);

            _logger.LogHeadline("Variables:");
            foreach (var (k, v) in finalVariables)
            {
                _logger.Log(string.Join("=", k, v));
            }
            _logger.Log("");
            
            _logger.LogHeadline("Steps to execute:");
            _logger.Log(string.Join(" -> ", executionPipeline.Select(x => x.Name)));
            _logger.Log("");

            foreach (var step in executionPipeline)
            {
                _logger.LogHeadline($"Executing step '{step.Name}':");
                
                foreach (var action in step.Actions)
                {
                    var shell = command.Shell;
                    
                    _logger.Log($"shell={shell}");
                    _logger.Log($"action={action}");
                    
                    var finalAction = _variableHelper.ExpandVariables(finalVariables, action);
                    var arguments = command.PrepareArguments(finalAction);
                    
                    _commandLineExecutor.Execute(shell, arguments);
                    _logger.Log("");
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