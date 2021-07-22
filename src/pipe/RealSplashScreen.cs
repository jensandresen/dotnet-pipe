using System.IO;
using System.Reflection;
using System.Text;

namespace pipe
{
    public class RealSplashScreen : ISplashScreen
    {
        private readonly ILogger _logger;

        public RealSplashScreen(ILogger logger)
        {
            _logger = logger;
        }
        
        public void Show()
        {
            var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("pipe.splash.txt");
            
            if (stream == null)
            {
                return;
            }
            
            using (stream)
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var text = reader.ReadToEnd();
                    _logger.Log("Welcome to...");
                    _logger.Log(text);
                    _logger.Log("");
                }
            }
        }
    }
}