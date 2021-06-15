namespace pipe.test.TestDoubles
{
    public class StubOperatingSystemTypeProvider : IOperatingSystemTypeProvider
    {
        private readonly OperatingSystemType _result;

        public StubOperatingSystemTypeProvider(OperatingSystemType result)
        {
            _result = result;
        }
            
        public OperatingSystemType Get()
        {
            return _result;
        }
    }
}