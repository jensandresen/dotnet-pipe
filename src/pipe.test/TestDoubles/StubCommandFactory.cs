using pipe.Shells;

namespace pipe.test.TestDoubles
{
    public class StubCommandFactory : ICommandFactory
    {
        private readonly Command _result;

        public StubCommandFactory(Command result)
        {
            _result = result;
        }
        
        public Command Create(string name)
        {
            return _result;
        }
    }
}