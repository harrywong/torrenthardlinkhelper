namespace TorrentHardLinkHelper.Models
{
    public sealed class FolderModel : EntityModel
    {
        public FolderModel()
        {
            this.Set(() => this.Located, ref this._locked, false);
            this.Set(() => this.Type, ref this._type, "Folder");
        }

        public FolderModel(string folderName)
            : this()
        {
            this.Set(() => this.Name, ref this._name, folderName);
        }
    }
}