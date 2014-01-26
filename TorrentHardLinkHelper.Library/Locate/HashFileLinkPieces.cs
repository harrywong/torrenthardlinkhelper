using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using TorrentHardLinkHelper.Torrents;

namespace TorrentHardLinkHelper.Locate
{
    internal class HashFileLinkPieces
    {
        private class CheckResult
        {
            public string Pattern { get; set; }
            public bool Matched { get; set; }
        }

        private int _result;
        private readonly IList<FileLinkPiece> _fileLinkPieces;
        private readonly Torrent _torrent;
        private readonly int _pieceIndex;
        private string _validPattern;
        private IDictionary<int, IList<FileSystemFileInfo>> _cleanedFileInfos;

        private delegate CheckResult CheckPatternFunc(string pattern);

        public HashFileLinkPieces(Torrent torrent, int pieceIndex,
            IList<FileLinkPiece> fileLinkPieces)
        {
            this._pieceIndex = pieceIndex;
            this._fileLinkPieces = fileLinkPieces;
            this._result = -1;
            this._torrent = torrent;
        }

        public string Run()
        {
            if (_fileLinkPieces.Any(c => c.FileLink.FsFileInfos.Count == 0))
            {
                return null;
            }
            this.CleanDuplicateFiles();
            this.Run(0, "");
            return this._validPattern;
        }

        private void CleanDuplicateFiles()
        {
            this._cleanedFileInfos = new Dictionary<int, IList<FileSystemFileInfo>>();
            for (int i = 0; i < this._fileLinkPieces.Count; i++)
            {
                var piece = this._fileLinkPieces[i];
                var hashes = new List<string>();
                this._cleanedFileInfos.Add(i, new List<FileSystemFileInfo>(piece.FileLink.FsFileInfos.Count));

                foreach (FileSystemFileInfo fileInfo in piece.FileLink.FsFileInfos)
                {
                    if (piece.FileLink.TorrentFile.StartPieceIndex == this._pieceIndex &&
                        piece.FileLink.TorrentFile.EndPieceIndex == this._pieceIndex)
                    {
                        if (fileInfo.Located)
                        {
                            continue;
                        }
                    }
                    string hash = this.HashFilePiece(fileInfo.FilePath,
                        piece.StartPos, piece.ReadLength);
                    if (hashes.Contains(hash))
                    {
                        continue;
                    }
                    hashes.Add(hash);
                    this._cleanedFileInfos[i].Add(fileInfo);
                }
            }
        }

        private void GroupFiles()
        {
            
        }

        private void Run(int index, string pattern)
        {
            if (this._result == 1)
            {
                return;
            }
            if (index < this._fileLinkPieces.Count)
            {
                for (int i = 0; i < this._cleanedFileInfos[index].Count; i++)
                {
                    string nextPattern = pattern + i + ',';
                    int nextIndex = index + 1;
                    Run(nextIndex, nextPattern);
                }
            }
            else
            {
                using (var pieceStream = new MemoryStream(this._torrent.PieceLength))
                {
                    for (int i = 0; i < this._fileLinkPieces.Count; i++)
                    {
                        int fileIndex = int.Parse(pattern.Split(',')[i]);
                        using (FileStream fileStream =
                            File.OpenRead(this._cleanedFileInfos[i][fileIndex].FilePath))
                        {
                            var buffer = new byte[this._fileLinkPieces[i].ReadLength];
                            fileStream.Position = (long)this._fileLinkPieces[i].StartPos;
                            fileStream.Read(buffer, 0, buffer.Length);
                            pieceStream.Write(buffer, 0, buffer.Length);
                        }
                    }

                    var sha1 = HashAlgoFactory.Create<SHA1>();
                    byte[] hash = sha1.ComputeHash(pieceStream.ToArray());
                    if (this._torrent.Pieces.IsValid(hash, this._pieceIndex))
                    {
                        this._validPattern = pattern;
                        this._result = 1;
                    }
                }
            }
        }

        private string HashFilePiece(string path, ulong start, ulong length)
        {
            var sha1 = HashAlgoFactory.Create<SHA1>();
            FileStream fileStream =
                File.OpenRead(path);
            var buffer = new byte[length];
            fileStream.Position = (long)start;
            fileStream.Read(buffer, 0, buffer.Length);
            byte[] hash = sha1.ComputeHash(buffer);
            fileStream.Close();
            return Convert.ToBase64String(hash);
        }
    }
}