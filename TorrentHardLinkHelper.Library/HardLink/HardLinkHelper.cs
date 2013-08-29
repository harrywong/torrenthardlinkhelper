using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TorrentHardLinkHelper.Locate;

namespace TorrentHardLinkHelper.HardLink
{
    public class HardLinkHelper
    {
        private readonly string _outputBaseFolder;
        private readonly string _folderName;
        private readonly IList<TorrentFileLink> _links;
        private readonly long _copyLimitSize;

        public HardLinkHelper(IList<TorrentFileLink> links, int copyLimitSize)
        {
            this._links = links;
            this._copyLimitSize = copyLimitSize*1024L;
        }

        public HardLinkHelper(IList<TorrentFileLink> links, int copyLimitSize, string folderName, string baseFolder)
            : this(links, copyLimitSize)
        {
            this._folderName = folderName;
            this._outputBaseFolder = baseFolder;
        }

        public void HardLink()
        {
            string rootFolder = Path.Combine(this._outputBaseFolder, this._folderName);
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }
            foreach (var link in this._links)
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
                        Directory.CreateDirectory(targetPath);
                    }
                }
                string targetFile = Path.Combine(rootFolder, link.TorrentFile.Path);

                if (link.TorrentFile.Length > this._copyLimitSize)
                {
                    CreateHarkLink(link.LinkedFsFileInfo.FilePath, targetFile);
                }
                else
                {
                    Copy(link.LinkedFsFileInfo.FilePath, targetFile);
                }
            }
        }

        private void CreateHarkLink(string source, string target)
        {
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
    }
}