using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TorrentHardLinkHelper.Locate;

namespace TorrentHardLinkHelper.HardLink
{
    public class HardLinkHelper
    {
        private StringBuilder _builder;
        private List<string> _createdFolders;

        public void HardLink(string sourceFolder, string targetParentFolder, string folderName, int copyLimitSize)
        {

            this._builder = new StringBuilder();
            this._builder.AppendLine("chcp 65001");
            this._builder.AppendLine("::==============================================::");
            this._builder.AppendLine(":: Torrent Hard-Link Helper v0.8");
            this._builder.AppendLine(":: By Harry Wong(harrywong@live.com), 2013-2014");
            this._builder.AppendLine("::");
            this._builder.AppendLine(":: Created at " + DateTime.Now);
            this._builder.AppendLine("::==============================================::");
            this._builder.AppendLine("::.");

            string rootFolder = Path.Combine(targetParentFolder, folderName);
            if (!Directory.Exists(rootFolder))
            {
                CreateFolder(rootFolder);
            }
            if (!Directory.Exists(targetParentFolder))
            {
                CreateFolder(targetParentFolder);
            }
            this.SearchFolder(sourceFolder, rootFolder, copyLimitSize);
            var utf8bom = new UTF8Encoding(false);
            File.WriteAllText(Path.Combine(rootFolder, "!hard-link.cmd"), this._builder.ToString(), utf8bom);
        }

        private void SearchFolder(string folder, string targetParentFolder, int copyLimitSize)
        {
            if (this._createdFolders == null)
            {
                this._createdFolders = new List<string>();
            }
            if (this._createdFolders.Contains(folder))
            {
                return;
            }
            foreach (var file in Directory.GetFiles(folder))
            {
                string targetFile = Path.Combine(targetParentFolder, Path.GetFileName(file));
                var fileInfo = new FileInfo(file);
                if (fileInfo.Length >= copyLimitSize)
                {
                    CreateHarkLink(file, targetFile);
                }
                else
                {
                    Copy(file, targetFile);
                }
            }
            foreach (var subFolder in Directory.GetDirectories(folder))
            {
                string targetSubFolder = Path.Combine(targetParentFolder, Path.GetFileName(subFolder));
                if (!Directory.Exists(targetSubFolder))
                {
                    this.CreateFolder(targetSubFolder);
                    this._createdFolders.Add(targetSubFolder);
                }
                SearchFolder(subFolder, targetSubFolder, copyLimitSize);
            }
        }

        public void HardLink(IList<TorrentFileLink> links, int copyLimitSize, string folderName, string baseFolde)
        {
            string rootFolder = Path.Combine(baseFolde, folderName);

            this._builder = new StringBuilder();
            this._builder.AppendLine("chcp 65001");
            this._builder.AppendLine("::==============================================::");
            this._builder.AppendLine(":: Torrent Hard-Link Helper v0.8");
            this._builder.AppendLine(":: By Harry Wong(harrywong@live.com), 2013-2014");
            this._builder.AppendLine("::");
            this._builder.AppendLine(":: Created at " + DateTime.Now);
            this._builder.AppendLine("::==============================================::");
            this._builder.AppendLine("::.");
            if (!Directory.Exists(rootFolder))
            {
                this.CreateFolder(rootFolder);
            }
            foreach (var link in links)
            {
                if (link.LinkedFsFileInfo == null)
                {
                    continue;
                }
                string[] pathParts = link.TorrentFile.Path.Split('\\');
                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    var targetPathParts = new string[i + 2];
                    targetPathParts[0] = rootFolder;
                    Array.Copy(pathParts, 0, targetPathParts, 1, i + 1);
                    string targetPath = Path.Combine(targetPathParts);
                    if (!Directory.Exists(targetPath))
                    {
                        this.CreateFolder(targetPath);
                    }
                }
                string targetFile = Path.Combine(rootFolder, link.TorrentFile.Path);

                if (link.TorrentFile.Length >= copyLimitSize)
                {
                    CreateHarkLink(link.LinkedFsFileInfo.FilePath, targetFile);
                }
                else
                {
                    Copy(link.LinkedFsFileInfo.FilePath, targetFile);
                }
            }
            File.WriteAllText(Path.Combine(rootFolder, "!hard-link.cmd"), this._builder.ToString(), Encoding.UTF8);
        }

        private void CreateHarkLink(string source, string target)
        {
            this._builder.AppendLine(string.Format("fsutil hardlink create \"{0}\" \"{1}\"", target, source));
            var procStartInfo =
                new ProcessStartInfo("cmd",
                    "/c " + string.Format("fsutil hardlink create \"{0}\" \"{1}\"", target, source));

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }

        private void Copy(string source, string target)
        {
            this._builder.AppendLine(string.Format("copy /y \"{0}\" \"{1}\"", source, target));
            var procStartInfo =
                new ProcessStartInfo("cmd",
                    "/c " + string.Format("copy /y \"{0}\" \"{1}\"", source, target));

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }

        private void CreateFolder(string path)
        {
            this._builder.AppendLine(string.Format("mkdir  \"{0}\"", path));
            Directory.CreateDirectory(path);
        }
    }
}