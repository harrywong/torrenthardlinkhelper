using System.Diagnostics;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ookii.Dialogs.Wpf;
using TorrentHardLinkHelper.HardLink;

namespace TorrentHardLinkHelper.ViewModels
{
    public class HardLinkToolViewModel : ViewModelBase
    {
        private string _sourceFolder;
        private string _parentFolder;
        private string _folderName;

        private RelayCommand _selectSourceFolderCommand;
        private RelayCommand _selectParentFolderCommand;
        private RelayCommand _defaultCommand;
        private RelayCommand _linkCommand;

        public HardLinkToolViewModel()
        {
            this.InitCommands();
        }

        public void InitCommands()
        {
            this._selectSourceFolderCommand = new RelayCommand(() =>
            {
                var dialog = new VistaFolderBrowserDialog();
                dialog.ShowNewFolderButton = true;
                dialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    this.Set(() => this.SourceFolder, ref this._sourceFolder, dialog.SelectedPath);
                    this.Set(() => this.ParentFolder, ref this._parentFolder, Directory.GetParent(dialog.SelectedPath).FullName);
                    this.Set(() => this.FolderName, ref this._folderName, dialog.SelectedPath + "_Copy");
                }
            });

            this._selectParentFolderCommand = new RelayCommand(() =>
            {
                var dialog = new VistaFolderBrowserDialog();
                dialog.ShowNewFolderButton = true;
                dialog.ShowDialog();
                if (dialog.SelectedPath != null)
                {
                    this.Set(() => this.ParentFolder, ref this._parentFolder, dialog.SelectedPath);
                }
            });

            this._defaultCommand = new RelayCommand(() =>
            {
                if (string.IsNullOrWhiteSpace(this._sourceFolder))
                {
                    this.Set(() => this.FolderName, ref this._folderName, "");
                }
                else
                {
                    this.Set(() => this.FolderName, ref this._folderName,
                        Path.GetDirectoryName(_folderName) + "_HLinked");
                }
            });

            this._linkCommand = new RelayCommand(() =>
            {
                var helper = new HardLinkHelper();
                helper.HardLink(this._sourceFolder, this._parentFolder, this._folderName, 1024000);
                Process.Start("explorer.exe", Path.Combine(this._parentFolder, this._folderName));
            }, () => !string.IsNullOrWhiteSpace(this._sourceFolder) && !string.IsNullOrWhiteSpace(this._parentFolder) && !string.IsNullOrWhiteSpace(this._folderName));
        }

        public string SourceFolder
        {
            get { return _sourceFolder; }
            set { _sourceFolder = value; }
        }

        public string ParentFolder
        {
            get { return _parentFolder; }
            set { _parentFolder = value; }
        }

        public string FolderName
        {
            get { return _folderName; }
            set { _folderName = value; }
        }

        public RelayCommand SelectSourceFolderCommand
        {
            get { return _selectSourceFolderCommand; }
            set { _selectSourceFolderCommand = value; }
        }

        public RelayCommand SelectParentFolderCommand
        {
            get { return _selectParentFolderCommand; }
            set { _selectParentFolderCommand = value; }
        }

        public RelayCommand DefaultCommand
        {
            get { return _defaultCommand; }
            set { _defaultCommand = value; }
        }

        public RelayCommand LinkCommand
        {
            get { return _linkCommand; }
            set { _linkCommand = value; }
        }
    }
}