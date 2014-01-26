using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TorrentHardLinkHelper.Locate;
using TorrentHardLinkHelper.Torrents;

namespace TorrentHardLinkHelper.Test
{
    [TestClass]
    public class LocateTest
    {
        /// <summary>
        /// 文件名字改动
        /// </summary>
        [TestMethod]
        public void TestLocate1()
        {
            string sourceFolder = @"I:\[BDMV][アニメ] ココロコネクト";
            string torrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestTorrents", "[U2].13680.torrent");

            IList<FileSystemFileInfo> fileInfos = GetFileSystemInfos(sourceFolder);
            var torrent = Torrent.Load(torrentPath);
            var locater = new TorrentFileLocater(torrent, fileInfos);

            var result = locater.Locate();
        }

        [TestMethod]
        public void TestLocate2()
        {
            string sourceFolder = @"I:\[BDMV][アニメ] ココロコネクト";
            string torrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestTorrents", "[U2].13680.torrent");

            IList<FileSystemFileInfo> fileInfos = GetFileSystemInfos(sourceFolder);
            var torrent = Torrent.Load(torrentPath);

            var locater = new PrivateObject(typeof(TorrentFileLocater), torrent, fileInfos);
            locater.Invoke("FindTorrentFileLinks");
            var result = (bool)locater.Invoke("CheckPiece", 10);

            Assert.IsTrue(result);
        }

        private IList<FileSystemFileInfo> GetFileSystemInfos(string path)
        {
            var fileInfos =
                Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                    .Select(file => new FileInfo(file))
                    .Select(f => new FileSystemFileInfo(f))
                    .ToList();
            return fileInfos;
        }
    }
}
