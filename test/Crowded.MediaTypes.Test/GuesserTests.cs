using System.IO;
using Xunit;
using static Crowded.MediaTypes.Guesser;

namespace Crowded.MediaTypes.Test
{
    public class GuesserTests
    {
        private static string TestFile =>
            Path.Combine("content", "image.jpg");
        
        [Fact]
        public void GuessMediaTypeFromFilePath()
        {
            var expected = "image/jpeg";
            var actual = GuessMediaType(TestFile);
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GuessMediaTypeFromBuffer()
        {
            var buffer = File.ReadAllBytes(TestFile);
            var expected = "image/jpeg";
            var actual = GuessMediaType(buffer);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GuessMediaTypeFromStream()
        {
            using (var stream = File.OpenRead(TestFile))
            {
                var expected = "image/jpeg";
                var actual = GuessMediaType(stream);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void GuessMediaTypeFromFileInfo()
        {
            var fileInfo = new FileInfo(TestFile);
            var expected = "image/jpeg";
            var actual = fileInfo.GuessMediaType();

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void GuessExtensionFromFilePath()
        {
            var expected = "jpeg";
            var actual = GuessExtension(TestFile);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GuessExtensionFromBuffer()
        {
            var buffer = File.ReadAllBytes(TestFile);
            var expected = "jpeg";
            var actual = GuessExtension(buffer);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GuessExtensionFromStream()
        {
            using (var stream = File.OpenRead(TestFile))
            {
                var expected = "jpeg";
                var actual = GuessExtension(stream);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void GuessExtensionFromFileInfo()
        {
            var fileInfo = new FileInfo(TestFile);
            var expected = "jpeg";
            var actual = fileInfo.GuessExtension();

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void GuessFileTypeFromFilePath()
        {
            var expected = new ContentType("image/jpeg", "jpeg");
            var actual = GuessContentType(TestFile);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GuessContentTypeFromBuffer()
        {
            var buffer = File.ReadAllBytes(TestFile);
            var expected = new ContentType("image/jpeg", "jpeg");
            var actual = GuessContentType(buffer);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GuessContentTypeFromStream()
        {
            using (var stream = File.OpenRead(TestFile))
            {
                var expected = new ContentType("image/jpeg", "jpeg");
                var actual = GuessContentType(stream);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void GuessContentTypeFromFileInfo()
        {
            var fileInfo = new FileInfo(TestFile);
            
            var expected = new ContentType("image/jpeg", "jpeg");
            var actual = fileInfo.GuessContentType();

            Assert.Equal(expected, actual);
        }
    }
}
