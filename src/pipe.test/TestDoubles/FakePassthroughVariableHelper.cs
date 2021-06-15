using System.Collections.Generic;

namespace pipe.test.TestDoubles
{
    public class FakePassthroughVariableHelper : IVariableHelper
    {
        public string ExpandVariables(Dictionary<string, string> variables, string action)
        {
            return action;
        }
    }
}