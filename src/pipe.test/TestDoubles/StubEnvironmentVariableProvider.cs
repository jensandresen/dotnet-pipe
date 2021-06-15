using System;

namespace pipe.test.TestDoubles
{
    public class StubEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        private readonly Func<string> _result;

        public StubEnvironmentVariableProvider(string result = null) : this(() => result)
        {
            
        }

        public StubEnvironmentVariableProvider(Func<string> resultProvider)
        {
            _result = resultProvider;
        }
        
        public string Get(string name)
        {
            return _result();
        }
    }
}