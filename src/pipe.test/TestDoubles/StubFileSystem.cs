namespace pipe.test.TestDoubles
{
    public class StubFileSystem : IFileSystem
    {
        private readonly string _localFile;
        private readonly bool _fileExists;
        private readonly string[] _fileContents;

        public StubFileSystem(string localFile = null, string[] fileContents = null)
        {
            _fileContents = fileContents;
            _localFile = localFile ?? (fileContents != null ? "dummy-file" : null);
            _fileExists = fileContents != null;
        }
        
        public string GetPathForLocalFile(string fileName) => _localFile;
        public bool DoesFileExists(string filePath) => _fileExists;
        public string[] ReadFileContents(string filePath) => _fileContents;
    }
}