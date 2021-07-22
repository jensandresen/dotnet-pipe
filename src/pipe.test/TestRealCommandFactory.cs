using pipe.Shells;
using pipe.test.TestDoubles;
using Xunit;

namespace pipe.test
{
    public class TestRealCommandFactory
    {
        [Theory]
        [InlineData(OperatingSystemType.Windows, "powershell")]
        [InlineData(OperatingSystemType.Linux, "sh")]
        [InlineData(OperatingSystemType.Mac, "bash")]
        public void returns_expected_default_shell(OperatingSystemType operatingSystemType, string expected)
        {
            var sut = new RealCommandFactory(new StubOperatingSystemTypeProvider(operatingSystemType));
            var result = sut.GetDefaultOSShell();
            
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(OperatingSystemType.Windows, "powershell")]
        [InlineData(OperatingSystemType.Linux, "sh")]
        [InlineData(OperatingSystemType.Mac, "bash")]
        public void returns_expected_command_shell_when_shell_is_not_defined(OperatingSystemType operatingSystemType, string expected)
        {
            var sut = new RealCommandFactory(new StubOperatingSystemTypeProvider(operatingSystemType));
            var result = sut.Create("?");
            
            Assert.Equal(expected, result.Shell);
        }

        [Theory]
        [InlineData("sh", "-c \"foo\"", "foo", OperatingSystemType.Linux)]
        [InlineData("sh", "-c \"foo\"", "foo", OperatingSystemType.Mac)]
        [InlineData("sh", "-c \"foo\"", "foo", OperatingSystemType.Windows)]
        [InlineData("bash", "-c \"foo\"", "foo", OperatingSystemType.Linux)]
        [InlineData("bash", "-c \"foo\"", "foo", OperatingSystemType.Mac)]
        [InlineData("bash", "-c \"foo\"", "foo", OperatingSystemType.Windows)]
        [InlineData("powershell", "-Command \"& { foo }\"", "foo", OperatingSystemType.Windows)]
        [InlineData("pwsh", "-Command \"& { foo }\"", "foo", OperatingSystemType.Linux)]
        [InlineData("pwsh", "-Command \"& { foo }\"", "foo", OperatingSystemType.Mac)]
        [InlineData("sh", "-c \"echo \\\"foo\\\"\"", "echo \"foo\"", OperatingSystemType.Linux)]
        [InlineData("sh", "-c \"echo \\\"foo\\\"\"", "echo \"foo\"", OperatingSystemType.Mac)]
        [InlineData("sh", "-c \"echo \\\"foo\\\"\"", "echo \"foo\"", OperatingSystemType.Windows)]
        [InlineData("bash", "-c \"echo \\\"foo\\\"\"", "echo \"foo\"", OperatingSystemType.Linux)]
        [InlineData("bash", "-c \"echo \\\"foo\\\"\"", "echo \"foo\"", OperatingSystemType.Mac)]
        [InlineData("bash", "-c \"echo \\\"foo\\\"\"", "echo \"foo\"", OperatingSystemType.Windows)]
        [InlineData("powershell", "-Command \"& { echo `\"foo`\" }\"", "echo \"foo\"", OperatingSystemType.Windows)]
        [InlineData("pwsh", "-Command \"& { echo `\"foo`\" }\"", "echo \"foo\"", OperatingSystemType.Linux)]
        [InlineData("pwsh", "-Command \"& { echo `\"foo`\" }\"", "echo \"foo\"", OperatingSystemType.Mac)]
        public void returns_expected_command(string expectedShell, string expectedPreparedArgument, string inputAction, OperatingSystemType operatingSystemType)
        {
            var stubOperatingSystemTypeProvider = new StubOperatingSystemTypeProvider(operatingSystemType);
            var sut = new RealCommandFactory(stubOperatingSystemTypeProvider);
            var result = sut.Create(expectedShell);
            
            Assert.Equal(expectedShell, result.Shell);
            Assert.Equal(expectedPreparedArgument, result.PrepareArguments(inputAction));
        }
    }
}