namespace WinBox.Model
{
    public class Folder
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public Folder(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}