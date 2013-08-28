using System.Collections.Generic;
using TorrentHardLinkHelper.Torrents;

namespace TorrentHardLinkHelper.Locate
{
    public class TorrentFileLink
    {
        private TorrentFile _torrentFile;
        private IList<FileSystemFileInfo> _fsFileInfos;
        private int _linkedFsFileIndex;
        private LinkState _state;

        public TorrentFileLink()
        {
            this._fsFileInfos = new List<FileSystemFileInfo>();
            this._linkedFsFileIndex = -1;
            this._state = LinkState.None;
        }

        public TorrentFileLink(TorrentFile torrentFile)
            : this()
        {
            this._torrentFile = torrentFile;
        }

        public TorrentFile TorrentFile
        {
            get { return this._torrentFile; }
            internal set { this._torrentFile = value; }
        }

        public IList<FileSystemFileInfo> FsFileInfos
        {
            get { return this._fsFileInfos; }
            internal set { this._fsFileInfos = value; }
        }

        public int LinkedFsFileIndex
        {
            get { return this._linkedFsFileIndex; }
            internal set { this._linkedFsFileIndex = value; }
        }

        public FileSystemFileInfo LinkedFsFileInfo
        {
            get
            {
                if (this._linkedFsFileIndex < 0 || this._linkedFsFileIndex >= this._fsFileInfos.Count)
                {
                    return null;
                }
                return this._fsFileInfos[this._linkedFsFileIndex];
            }
        }

        public LinkState State
        {
            get { return this._state; }
            internal set { this._state = value; }
        }

        public override string ToString()
        {
            return this._torrentFile.FullPath + ", count: " + this._fsFileInfos.Count + ", state: " + this._state;
        }
    }
}