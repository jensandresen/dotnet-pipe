using System;
using System.IO;

namespace pipe
{
    public class RealFileSystem : IFileSystem
    {
        public string GetPathForLocalFile(string fileName) => Path.Combine(Environment.CurrentDirectory, fileName);
        public bool DoesFileExists(string filePath) => File.Exists(filePath);
        public string[] ReadFileContents(string filePath) => File.ReadAllLines(filePath);
    }
}