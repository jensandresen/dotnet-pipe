using System.Collections.Generic;

namespace pipe
{
    public interface IVariableHelper
    {
        string ExpandVariables(Dictionary<string,string> variables, string action);
    }
}