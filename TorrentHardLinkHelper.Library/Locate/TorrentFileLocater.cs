using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TorrentHardLinkHelper.Torrents;

namespace TorrentHardLinkHelper.Locate
{
    public class TorrentFileLocater
    {
        private class FileLinkPiece
        {
            public TorrentFileLink FileLink { get; set; }
            public ulong StartPos { get; set; }
            public ulong ReadLength { get; set; }

            public override string ToString()
            {
                return this.FileLink + ", startpos: " + this.StartPos + ", length: " + this.ReadLength;
            }
        }

        private class HashFileLinkPieces
        {
            private int _result;
            private readonly IList<FileLinkPiece> _fileLinkPieces;
            private readonly Torrent _torrent;
            private readonly int _pieceIndex;
            private string _validPattern;

            public HashFileLinkPieces(Torrent torrent, int pieceIndex, IList<FileLinkPiece> fileLinkPieces)
            {
                this._pieceIndex = pieceIndex;
                this._fileLinkPieces = fileLinkPieces;
                this._result = -1;
                this._torrent = torrent;
            }

            public string Run()
            {
                this.Run(0, "");
                return this._validPattern;
            }

            private void Run(int index, string pattern)
            {
                if (this._result != -1)
                {
                    return;
                }
                if (index < this._fileLinkPieces.Count)
                {
                    if (this._fileLinkPieces[index].FileLink.FsFileInfos.Count == 0)
                    {
                        this._result = 0;
                        return;
                    }
                    for (int i = 0; i < this._fileLinkPieces[index].FileLink.FsFileInfos.Count; i++)
                    {
                        string nextPattern = pattern + i + ',';
                        int nextIndex = index + 1;
                        Run(nextIndex, nextPattern);
                    }
                }
                else
                {
                    var pieceStream = new MemoryStream(this._torrent.PieceLength);
                    for (int i = 0; i < this._fileLinkPieces.Count; i++)
                    {
                        int fileIndex = int.Parse(pattern.Split(',')[i]);
                        if (this._fileLinkPieces[i].FileLink.FsFileInfos[fileIndex].Located)
                        {
                            return;
                        }
                        FileStream fileStream =
                            File.OpenRead(this._fileLinkPieces[i].FileLink.FsFileInfos[fileIndex].FilePath);
                        var buffer = new byte[this._fileLinkPieces[i].ReadLength];
                        fileStream.Position = (long)this._fileLinkPieces[i].StartPos;
                        fileStream.Read(buffer, 0, buffer.Length);
                        pieceStream.Write(buffer, 0, buffer.Length);
                    }

                    var sha1 = HashAlgoFactory.Create<SHA1>();
                    byte[] hash = sha1.ComputeHash(pieceStream.ToArray());
                    if (this._torrent.Pieces.IsValid(hash, this._pieceIndex))
                    {
                        this._result = 1;
                        this._validPattern = pattern;
                    }
                }
            }
        }


        private readonly IList<FileSystemFileInfo> _fsfiFileInfos;
        private readonly IList<TorrentFileLink> _torrentFileLinks;
        private readonly Dictionary<int, bool> _pieceCheckedReusltsDictionary;
        private readonly Torrent _torrent;

        public TorrentFileLocater()
        {
            this._torrentFileLinks = new List<TorrentFileLink>();
        }

        public TorrentFileLocater(Torrent torrent)
            : this()
        {
            this._torrent = torrent;
            this._pieceCheckedReusltsDictionary = new Dictionary<int, bool>(this._torrent.Pieces.Count);
        }

        public TorrentFileLocater(Torrent torrent, IList<FileSystemFileInfo> fsfiFileInfos)
            : this(torrent)
        {
            this._fsfiFileInfos = fsfiFileInfos;
        }

        public LocateResult Locate()
        {
            if (this._torrent == null)
            {
                throw new ArgumentException("Torrent cannot be null.");
            }
            if (this._fsfiFileInfos == null || this._fsfiFileInfos.Count == 0)
            {
                throw new ArgumentException("FsFileInfos cannot be null or zero");
            }

            this.FindTorrentFileLinks();
            this.ConfirmFileSystemFiles();
            return new LocateResult(this._torrentFileLinks);
        }

        private void FindTorrentFileLinks()
        {
            foreach (TorrentFile torrentFile in this._torrent.Files)
            {
                var fileLink = new TorrentFileLink(torrentFile);
                foreach (FileSystemFileInfo fsFileInfo in this._fsfiFileInfos)
                {
                    if (fsFileInfo.Length == torrentFile.Length)
                    {
                        fileLink.FsFileInfos.Add(fsFileInfo);
                    }
                }
                if (fileLink.FsFileInfos.Count > 1)
                {
                    if (fileLink.FsFileInfos.Any(f => f.FileName == Path.GetFileName(torrentFile.Path)))
                    {
                        fileLink.FsFileInfos =
                            fileLink.FsFileInfos.Where(f => f.FileName == Path.GetFileName(torrentFile.Path)).ToList();
                        if (fileLink.FsFileInfos.Count > 1)
                        {
                            if (fileLink.FsFileInfos.Any(f => f.FilePath.Contains(torrentFile.Path)))
                            {
                                fileLink.FsFileInfos =
                                    fileLink.FsFileInfos.Where(f => f.FilePath.Contains(torrentFile.Path))
                                        .ToList();
                            }
                        }
                    }
                }
                if (fileLink.FsFileInfos.Count == 1)
                {
                    fileLink.State = LinkState.Located;
                    fileLink.LinkedFsFileIndex = 0;
                }
                else if (fileLink.FsFileInfos.Count > 1)
                {
                    fileLink.State = LinkState.NeedConfirm;
                }
                else
                {
                    fileLink.State = LinkState.Fail;
                }

                this._torrentFileLinks.Add(fileLink);
            }
        }

        private void ConfirmFileSystemFiles()
        {
            foreach (TorrentFileLink fileLink in this._torrentFileLinks)
            {
                if (fileLink.State != LinkState.NeedConfirm)
                {
                    continue;
                }
                for (int i = fileLink.TorrentFile.StartPieceIndex; i <= fileLink.TorrentFile.EndPieceIndex; i++)
                {
                    if (!CheckPiece(i))
                    {
                        break;
                    }
                    if (fileLink.State == LinkState.Located)
                    {
                        break;
                    }
                }
            }
        }

        private bool CheckPiece(int pieceIndex)
        {
            bool result;
            if (this._pieceCheckedReusltsDictionary.TryGetValue(pieceIndex, out result))
            {
                return result;
            }
            ulong startPos = (ulong)pieceIndex * (ulong)this._torrent.PieceLength;
            ulong pos = 0;
            ulong writenLength = 0;
            var filePieces = new List<FileLinkPiece>();
            foreach (TorrentFileLink fileLink in this._torrentFileLinks)
            {
                if (pos + (ulong)fileLink.TorrentFile.Length >= startPos)
                {
                    ulong readPos = startPos - pos;
                    ulong readLength = (ulong)fileLink.TorrentFile.Length - readPos;
                    if (writenLength + readLength > (ulong)this._torrent.PieceLength)
                    {
                        readLength = (ulong)this._torrent.PieceLength - writenLength;
                    }

                    var filePiece = new FileLinkPiece
                    {
                        FileLink = fileLink,
                        ReadLength = readLength,
                        StartPos = readPos
                    };
                    filePieces.Add(filePiece);

                    writenLength += readLength;
                    startPos += readLength;
                    if (writenLength == (ulong)this._torrent.PieceLength)
                    {
                        break;
                    }
                }
                pos += (ulong)fileLink.TorrentFile.Length;
            }

            var hash = new HashFileLinkPieces(this._torrent, pieceIndex, filePieces);
            string pattern = hash.Run();
            if (string.IsNullOrEmpty(pattern))
            {
                foreach (FileLinkPiece piece in filePieces)
                {
                    piece.FileLink.State = LinkState.Fail;
                }
                this._pieceCheckedReusltsDictionary.Add(pieceIndex, false);
                return false;
            }
            for (int i = 0; i < filePieces.Count; i++)
            {
                if (filePieces[i].FileLink.State == LinkState.NeedConfirm)
                {
                    filePieces[i].FileLink.LinkedFsFileIndex = int.Parse(pattern.Split(',')[i]);
                    filePieces[i].FileLink.LinkedFsFileInfo.Located = true;
                    filePieces[i].FileLink.State = LinkState.Located;
                }
            }
            this._pieceCheckedReusltsDictionary.Add(pieceIndex, true);
            return true;
        }
    }
}