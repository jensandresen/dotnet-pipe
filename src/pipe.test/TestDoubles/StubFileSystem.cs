namespace pipe.test.TestDoubles
{
    public class StubFileSystem : IFileSystem
    {
        private readonly string _localFile;
        private readonly bool _fileExists;
        private readonly string[] _fileContents;

        public StubFileSystem(string localFile = "", bool fileExists = false, string[] fileContents = null)
        {
            _localFile = localFile;
            _fileExists = fileExists;
            _fileContents = fileContents;
        }
        
        public string GetPathForLocalFile(string fileName) => _localFile;
        public bool DoesFileExists(string filePath) => _fileExists;
        public string[] ReadFileContents(string filePath) => _fileContents;
    }
}