using pipe.Shells;

namespace pipe.test.TestDoubles
{
    public class SpyCommandFactory : ICommandFactory
    {
        public string wasCreatedWith;
        private readonly ICommandFactory _inner;
        
        public SpyCommandFactory(ICommandFactory inner)
        {
            _inner = inner;
        }
        
        public Command Create(string name)
        {
            wasCreatedWith = name;
            return _inner.Create(name);
        }
    }
}