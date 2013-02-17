using System.IO;

namespace WinBox.Model
{
    public class UploadData
    {
        public string Path { get; private set; }
        public Stream FileData { get; private set; }
        public string Name { get; private set; }

        public UploadData(string path, Stream fileData, string name)
        {
            Path = path;
            FileData = fileData;
            Name = name;
        }
    }
}