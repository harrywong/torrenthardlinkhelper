using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TorrentHardLinkHelper.Test
{
    [TestClass]
    public class HardLinkTest
    {
        [TestMethod]
        public void TestHardTest()
        {
            ProcessStartInfo procStartInfo =
                new ProcessStartInfo("cmd", "/c " + @"fsutil hardlink create D:\b.cs D:\a.cs");

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