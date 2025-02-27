﻿
namespace LumaSharp.Runtime.Reflection
{
    public sealed class MetaLibraryName
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
        public MetaLibraryName(string path)
        {
            this.hintPath = path;
            this.name = Path.GetFileName(path);
            this.version = new Version();
        }

        public MetaLibraryName(string name, Version version, string hintPath = null)
        {
            this.name = name;
            this.version = version;
            this.hintPath = hintPath;
        }

        internal MetaLibraryName(BinaryReader reader, string hintPath = null)
        {
            this.name = reader.ReadString();
            this.version = new Version(
                reader.ReadInt32(),
                reader.ReadInt32());
            this.hintPath = hintPath;
        }
    }
}
