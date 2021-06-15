using pipe.Shells;
using pipe.test.TestDoubles;

namespace pipe.test.Builders
{
    public class RealCommandFactoryBuilder
    {
        private IOperatingSystemTypeProvider _operatingSystemTypeProvider;

        public RealCommandFactoryBuilder()
        {
            _operatingSystemTypeProvider = Dummy.Of<IOperatingSystemTypeProvider>();
        }

        public RealCommandFactoryBuilder WithOperatingSystemTypeProvider(IOperatingSystemTypeProvider operatingSystemTypeProvider)
        {
            _operatingSystemTypeProvider = operatingSystemTypeProvider;
            return this;
        }

        public RealCommandFactory Build()
        {
            return new RealCommandFactory(
                operatingSystemTypeProvider: _operatingSystemTypeProvider 
            );
        }
    }
}