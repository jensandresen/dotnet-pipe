using pipe.test.TestDoubles;

namespace pipe.test.Builders
{
    public class VariableHelperBuilder
    {
        private IEnvironmentVariableProvider _environmentVariableProvider;

        public VariableHelperBuilder()
        {
            _environmentVariableProvider = new StubEnvironmentVariableProvider();
        }
        
        public VariableHelperBuilder WithEnvironmentVariableProvider(IEnvironmentVariableProvider environmentVariableProvider)
        {
            _environmentVariableProvider = environmentVariableProvider;
            return this;
        }
        
        public VariableHelper Build()
        {
            return new VariableHelper(_environmentVariableProvider);
        }
    }
}