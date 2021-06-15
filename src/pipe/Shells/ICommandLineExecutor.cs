namespace pipe.Shells
{
    public interface ICommandLineExecutor
    {
        void Execute(string shell, string arguments);
    }
}