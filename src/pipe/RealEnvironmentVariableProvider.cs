using System;

namespace pipe
{
    public class RealEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        public string Get(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }
    }
}