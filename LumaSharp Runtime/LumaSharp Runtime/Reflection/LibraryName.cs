
namespace LumaSharp.Runtime.Reflection
{
    public sealed class LibraryName
    {
        // Private
        private string name = "";
        private string hintPath = "";
        private Version version = null;

        // Properties
        public string Name
        {
            get { return name; }
        }

        public string HintPath
        {
            get { return hintPath; }
        }

        public Version Version
        {
            get { return version; } 
        }

        // Constructor
        public LibraryName(string path)
        {
            this.hintPath = path;
            this.name = Path.GetFileName(path);
            this.version = new Version();
        }

        public LibraryName(string name, Version version, string hintPath = null)
        {
            this.name = name;
            this.version = version;
            this.hintPath = hintPath;
        }
    }
}
