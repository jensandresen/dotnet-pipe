namespace pipe.Shells
{
    public interface ICommandFactory
    {
        Command Create(string name);
    }
}