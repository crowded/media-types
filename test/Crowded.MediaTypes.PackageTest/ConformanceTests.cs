using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Crowded.MediaTypes.PackageTest
{
    public class ConformanceTests
    {
        [Fact]
        public void LoadLibWithNonExistingDbThrows()
        {
            Assert.Throws<ArgumentException>(() => new Magic(Magic.DefaultMagicFlags, null));
        }

        [Fact]
        public void LoadLibSucceeds()
        {
            Assert.NotNull(new Magic(Magic.DefaultMagicFlags, Magic.DefaultMagicFilePath));
        }

        private const string PublishedFolderMissing =
            "Some IDE test runners don't start these through MSBuild, make sure you ran a dotnet publish before testing in such cases.";

        [Fact]
        public void HasPublishedMagicMgc()
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "publish", "magic.mgc"
            );
            Assert.True(File.Exists(path), PublishedFolderMissing);
        }

        [Fact]
        public void HasPublishedLibMagicLinuxRuntime()
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "publish", "runtimes", "linux-x64", "native", "libmagic-1.so"
            );
            Assert.True(File.Exists(path), PublishedFolderMissing);
        }

        [Fact]
        public void HasPublishedLibMagicMacOsRuntime()
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "publish", "runtimes", "osx-x64", "native", "libmagic-1.dylib"
            );
            Assert.True(File.Exists(path), PublishedFolderMissing);
        }

        [Fact]
        public void HasPublishedLibMagicWindowsRuntime()
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "publish", "runtimes", "win-x64", "native", "libmagic-1.dll"
            );
            Assert.True(File.Exists(path), PublishedFolderMissing);

            var path2 = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "publish", "runtimes", "win-x64", "native", "libgnurx-0.dll"
            );
            Assert.True(File.Exists(path2), PublishedFolderMissing);
        }
    }
}
