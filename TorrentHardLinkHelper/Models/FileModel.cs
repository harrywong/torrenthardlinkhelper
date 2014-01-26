using System.IO;
using TorrentHardLinkHelper.Torrents;

namespace TorrentHardLinkHelper.Models
{
    public sealed class FileModel : EntityModel
    {
        public FileModel()
        {
            this.Set(() => this.Located, ref this._locked, false);
            this.Set(() => this.Type, ref this._type, "File");
        }

        public FileModel(string fullName)
            : this()
        {
            this.Set(() => this.Name, ref this._name, Path.GetFileName(fullName));
            this.Set(() => this.FullName, ref this._fullName, fullName);
        }

        public FileModel(TorrentFile torrentFile)
            : this()
        {
            this.Set(() => this.Name, ref this._name, Path.GetFileName(torrentFile.Path));
            this.Set(() => this.FullName, ref this._fullName, torrentFile.Path);
        }
    }
}