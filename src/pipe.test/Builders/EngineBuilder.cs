using pipe.Shells;
using pipe.test.TestDoubles;

namespace pipe.test.Builders
{
    public class EngineBuilder
    {
        private IFileSystem _fileSystem;
        private ICommandFactory _commandFactory;
        private ICommandLineExecutor _commandLineExecutor;
        private IVariableHelper _variableHelper;

        public EngineBuilder()
        {
            _fileSystem = Dummy.Of<IFileSystem>();
            _commandFactory = new StubCommandFactory(new Command("dummy", passThroughArgs => passThroughArgs));
            _commandLineExecutor = Dummy.Of<ICommandLineExecutor>();
            _variableHelper = new FakePassthroughVariableHelper();
        }

        public EngineBuilder WithFileSystem(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            return this;
        }

        public EngineBuilder WithCommandFactory(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
            return this;
        }

        public EngineBuilder WithCommandLineExecutor(ICommandLineExecutor commandLineExecutor)
        {
            _commandLineExecutor = commandLineExecutor;
            return this;
        }

        public EngineBuilder WithVariableHelper(IVariableHelper variableHelper)
        {
            _variableHelper = variableHelper;
            return this;
        }
        
        public Engine Build()
        {
            return new Engine(
                fileSystem: _fileSystem,
                commandFactory: _commandFactory,
                commandLineExecutor: _commandLineExecutor,
                variableHelper: _variableHelper,
                logger: new NullLogger(),
                splashScreen: Dummy.Of<ISplashScreen>()
            );
        }
    }
}