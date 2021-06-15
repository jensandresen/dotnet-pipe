namespace pipe
{
    public interface IFileSystem
    {
        string GetPathForLocalFile(string fileName);
        bool DoesFileExists(string filePath);
        string[] ReadFileContents(string filePath);
    }
}