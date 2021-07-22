namespace pipe
{
    public interface ILogger
    {
        void EnableVerbosity();
        void Log(string message);
        void LogHeadline(string message);
    }
}