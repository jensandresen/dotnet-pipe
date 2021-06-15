namespace pipe
{
    public class RealOperatingSystemTypeProvider : IOperatingSystemTypeProvider
    {
        public OperatingSystemType Get()
        {
            if (System.OperatingSystem.IsWindows())
            {
                return OperatingSystemType.Windows;
            }
            
            if (System.OperatingSystem.IsLinux())
            {
                return OperatingSystemType.Linux;
            }

            if (System.OperatingSystem.IsMacOS())
            {
                return OperatingSystemType.Mac;
            }

            return OperatingSystemType.Unknown;
        }
    }
}