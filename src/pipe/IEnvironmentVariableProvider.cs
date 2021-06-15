namespace pipe
{
    public interface IEnvironmentVariableProvider
    {
        string Get(string name);
    }
}