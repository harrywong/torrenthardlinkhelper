using System.IO;

namespace TorrentHardLinkHelper.Locate
{
    public class FileSystemFileInfo
    {
        private string _fileName;
        private string _filePath;
        private long _length;
        private bool _located;

        public FileSystemFileInfo()
        {
            this._located = false;
        }

        public FileSystemFileInfo(FileInfo fileInfo)
        {
            this._fileName = fileInfo.Name;
            this._filePath = fileInfo.FullName;
            this._length = fileInfo.Length;
        }

        public string FileName
        {
            get { return this._fileName; }
            internal set { this._fileName = value; }
        }

        public string FilePath
        {
            get { return this._filePath; }
            internal set { this._filePath = value; }
        }

        public long Length
        {
            get { return this._length; }
            internal set { this._length = value; }
        }

        public bool Located
        {
            get { return this._located; }
            set { this._located = value; }
        }

        public override string ToString()
        {
            return this._filePath + ", length: " + this._length;
        }
    }
}