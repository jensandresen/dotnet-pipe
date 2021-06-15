using System.Collections.Generic;

namespace pipe.test.TestDoubles
{
    public class FakeEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        private readonly Dictionary<string, string> _results;

        public FakeEnvironmentVariableProvider(Dictionary<string, string> results)
        {
            _results = results;
        }
        
        public string Get(string name)
        {
            return _results[name];
        }
    }
}