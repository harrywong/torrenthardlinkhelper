using System;
using System.Collections.Generic;
using System.IO;

namespace TorrentHardLinkHelper.Locate
{
    public class FileSystemFileSearcher
    {
        public static IList<FileSystemFileInfo> SearchFolder(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(path + " cannot be found.");
            }

            var fsFileInfos = new List<FileSystemFileInfo>();
            foreach (string fileName in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(fileName);
                fsFileInfos.Add(new FileSystemFileInfo(fileInfo));
            }
            return fsFileInfos;
        }
    }
}