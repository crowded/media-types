using System;
using System.IO;

namespace Crowded.MediaTypes
{
    /// <summary>
    /// Describes a media type and a matching extension of some content.
    /// </summary>
    public struct ContentType : IEquatable<ContentType>
    {
        /// <summary>
        /// MediaType of some content.
        /// </summary>
        public string MediaType { get; }
        /// <summary>
        /// Extensions that best fits MediaType.
        /// </summary>
        public string Extension { get; }
        
        /// <param name="mediaType"></param>
        /// <param name="extension"></param>
        public ContentType(string mediaType, string extension)
        {
            MediaType = mediaType;
            Extension = extension;
        }

        /// <inheritdoc />
        public bool Equals(ContentType other)
        {
            return string.Equals(MediaType, other.MediaType) && string.Equals(Extension, other.Extension);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ContentType other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((MediaType != null ? MediaType.GetHashCode() : 0) * 397) ^ (Extension != null ? Extension.GetHashCode() : 0);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return MediaType + "; extension=" + Extension;
        }
    }
    
#pragma warning disable 1591

    /// <summary>
    /// Simple static methods for guessing media types and extensions
    /// </summary>
    public static class Guesser
    {
        public static string GuessMediaType(string filepath)
            => Magic.DefaultInstance.Read(filepath);
        
        public static string GuessMediaType(FileInfo fi)
            => GuessMediaType(fi.FullName);
        
        public static string GuessMediaType(byte[] buffer, int bufferSize = 1024)
            => Magic.DefaultInstance.Read(buffer, bufferSize);

        public static string GuessMediaType(Stream stream, int bufferSize = 1024)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException(
                    "Stream is not seekable, call the overload with a byte buffer read from a non seekable stream instead.");
            }

            var originalPosition = stream.Position;
            try
            {
                var buffer = new byte[bufferSize];
                int num;
                for (var offset = 0; offset < bufferSize; offset += num)
                {
                    num = stream.Read(buffer, offset, bufferSize - offset);
                    if (num <= 0)
                        break;
                }
                return GuessMediaType(buffer, bufferSize);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        public static string GuessExtension(string filePath)
            => ReadExtension(filePath);

        public static string GuessExtension(FileInfo fi)
            => GuessExtension(fi.FullName);

        public static string GuessExtension(byte[] buffer, int bufferSize = 1024)
            => ReadExtension(buffer, bufferSize);

        public static string GuessExtension(Stream stream, int bufferSize = 1024)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException(
                    "Stream is not seekable, call the overload with a byte buffer read from a non seekable stream instead.");
            }

            var originalPosition = stream.Position;
            try
            {
                var buffer = new byte[bufferSize];
                int num;
                for (var offset = 0; offset < bufferSize; offset += num)
                {
                    num = stream.Read(buffer, offset, bufferSize - offset);
                    if (num <= 0)
                        break;
                }
                return GuessExtension(buffer, bufferSize);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }
        
        public static ContentType GuessContentType(string filePath)
        {
            var mime = GuessMediaType(filePath);
            var extension = GuessExtension(filePath);
            return new ContentType(mime, extension);
        }

        public static ContentType GuessContentType(FileInfo fi)
            => GuessContentType(fi.FullName);
        
        public static ContentType GuessContentType(byte[] buffer, int bufferSize = 1024)
        {
            var mime = GuessMediaType(buffer, bufferSize);
            var extension = GuessExtension(buffer, bufferSize);
            return new ContentType(mime, extension);
        }

        public static ContentType GuessContentType(Stream stream, int bufferSize = 1024)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException(
                    "Stream is not seekable, call the overload with a byte buffer read from a non seekable stream instead.");
            }

            var originalPosition = stream.Position;
            try
            {
                var buffer = new byte[bufferSize];
                int num;
                for (var offset = 0; offset < bufferSize; offset += num)
                {
                    num = stream.Read(buffer, offset, bufferSize - offset);
                    if (num <= 0)
                        break;
                }
                var mime = GuessMediaType(buffer, bufferSize);
                var extension = GuessExtension(buffer, bufferSize);
                return new ContentType(mime, extension);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private static string ReadExtension(string filepath)
        {
            var instance = Magic.DefaultInstance;
            try
            {
                instance.SetFlags((Magic.DefaultMagicFlags & ~MagicFlags.MAGIC_MIME_TYPE) | MagicFlags.MAGIC_EXTENSION);
                var extensions = instance.Read(filepath);
                var slash = extensions.IndexOf('/');
                return slash < 0 ? extensions : extensions.Substring(0, slash);
            }
            finally
            {
                instance.SetFlags(Magic.DefaultMagicFlags);
            }
        }
        
        private static string ReadExtension(byte[] buffer, int bufferSize = 1024)
        {
            var instance = Magic.DefaultInstance;
            try
            {
                instance.SetFlags((Magic.DefaultMagicFlags & ~MagicFlags.MAGIC_MIME_TYPE) | MagicFlags.MAGIC_EXTENSION);
                var extensions = instance.Read(buffer, bufferSize);
                var slash = extensions.IndexOf('/');
                return slash < 0 ? extensions : extensions.Substring(0, slash);
            }
            finally
            {
                instance.SetFlags(Magic.DefaultMagicFlags);
            }
        }
    }
    
    public static class GuesserExtensions
    {
        public static string GuessMediaType(this FileInfo fi)
            => Guesser.GuessMediaType(fi);
        
        public static string GuessExtension(this FileInfo fi)
            => Guesser.GuessExtension(fi);
        
        public static ContentType GuessContentType(this FileInfo fi)
            => Guesser.GuessContentType(fi);
    }
}
