using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TorrentHardLinkHelper.Locate
{
    public class FileLinkPiece
    {
        public TorrentFileLink FileLink { get; set; }
        public ulong StartPos { get; set; }
        public ulong ReadLength { get; set; }

        public override string ToString()
        {
            return this.FileLink + ", startpos: " + this.StartPos + ", length: " + this.ReadLength;
        }
    }
}
