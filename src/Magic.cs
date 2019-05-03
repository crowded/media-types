using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Crowded.MediaTypes
{
    /// <summary>
    /// Implementation wrapper around native libmagic.
    /// </summary>
    /// <remarks>Class is not thread safe</remarks>
    public sealed class Magic : IDisposable
    {
        /// <summary>
        /// Libmagic open flags for getting file type
        /// </summary>
        public static MagicFlags DefaultMagicFlags { get; } = 
            MagicFlags.MAGIC_ERROR | 
            MagicFlags.MAGIC_MIME_TYPE |
            MagicFlags.MAGIC_NO_CHECK_COMPRESS |
            MagicFlags.MAGIC_NO_CHECK_ELF |
            MagicFlags.MAGIC_NO_CHECK_APPTYPE;
        
        /// <summary>
        /// Probing paths for finding libmagic magic.mgc database file.
        /// </summary>
        public static IEnumerable<string> DefaultMagicFileProbingPaths { get; set; } = new[]
        {
            // If we are running as a published app we should have magic.mgc in our assembly folder.
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "magic.mgc"),
            // If we are running in a dev environment or with a runtime package store then
            // we should have magic.mgc in our content folder, working back from lib/netstandardX.X/lib.dll
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../content/magic.mgc")
        }.Select(Path.GetFullPath).ToArray();

        private static string _magicFilePath;
        
        // Fix the path to a specific value by capturing it explicitly.
        // The ThreadLocal shouldn't ever have instances with different paths.
        private static Func<Magic> CreateInstanceFactory(string path)
            => () => new Magic(DefaultMagicFlags, path);
        
        /// <summary>
        /// Path to libmagic magic.mgc database file
        /// </summary>
        public static string DefaultMagicFilePath
        {
            get => _magicFilePath ?? DefaultMagicFileProbingPaths.FirstOrDefault(File.Exists);
            set
            {
                _magicFilePath = value;
                _instance.Dispose();
                _instance = new ThreadLocal<Magic>(CreateInstanceFactory(DefaultMagicFilePath));
            }
        }

        private static ThreadLocal<Magic> _instance
            = new ThreadLocal<Magic>(CreateInstanceFactory(DefaultMagicFilePath));

        internal static Magic DefaultInstance => _instance.Value; 
        
        private IntPtr _cookie;

        /// <summary>
        /// Flags that are enabled for this libmagic instance.
        /// </summary>
        public MagicFlags ActiveFlags { get; private set; }
        
        /// <summary>
        /// Returns the version of the used libmagic library.
        /// </summary>
        public static int Version => Native.magic_version();

        private string LastError
        {
            get
            {
                var err = Marshal.PtrToStringAnsi(Native.magic_error(_cookie));
                if (err == null) return null;
                return char.ToUpper(err[0]) + err.Substring(1);
            }
        }
        
        /// <param name="flags"></param>
        /// <param name="magicFilePath"></param>
        /// <exception cref="MagicException"></exception>
        public Magic(MagicFlags flags, string magicFilePath)
        {
            if (!File.Exists(magicFilePath))
            {
                throw new ArgumentException("File does not exist at path: " + magicFilePath, nameof(magicFilePath));
            }
            
            _cookie = Native.magic_open(flags);
            if (_cookie == IntPtr.Zero)
            {
                throw new MagicException(LastError, "Could not create magic cookie.");
            }

            if (Native.magic_load(_cookie, magicFilePath) != 0)
            {
                throw new MagicException(LastError, "Could not load magic database file.");
            }
            ActiveFlags = flags;
        }

        /// <summary>
        /// Change active flags for current instance.
        /// </summary>
        /// <param name="flags"></param>
        /// <exception cref="MagicException"></exception>
        public void SetFlags(MagicFlags flags)
        {
            var exit = Native.magic_setflags(_cookie, flags);
            if (exit != 0)
            {
                throw new MagicException("Could not change flags.");
            }
            ActiveFlags = flags;
        }
        
        /// <summary>
        /// Reads file at <paramref name="filePath"/> to find magic bytes for a media type guess.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="MagicException"></exception>
        public string Read(string filePath)
        {
            var str = Marshal.PtrToStringAnsi(Native.magic_file(_cookie, filePath));
            if (str == null)
            {
                throw new MagicException(LastError);
            }
            return str;
        }

        /// <summary>
        /// Reads <paramref name="bufferSize"/> or <paramref name="buffer"/>.Length (whichever is smaller) from <paramref name="buffer"/> to find magic bytes for a media type guess.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        /// <exception cref="MagicException"></exception>
        public string Read(byte[] buffer, int bufferSize)
        {
            bufferSize = Math.Min(buffer.Length, bufferSize);
            var str = Marshal.PtrToStringAnsi(Native.magic_buffer(_cookie, buffer, bufferSize));
            if (str == null)
            {
                throw new MagicException(LastError);
            }
            return str;
        }
        
        /// <inheritdoc />
        ~Magic()
        {
            Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_cookie != IntPtr.Zero)
            {
                Native.magic_close(_cookie);
                _cookie = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }

        private static class Native
        {
            private const string LibPath = "libmagic-1";

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr magic_open(MagicFlags flags);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
            public static extern int magic_load(IntPtr magic_cookie, string filename);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
            public static extern void magic_close(IntPtr magic_cookie);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr magic_file(IntPtr magic_cookie, string filename);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr magic_buffer(IntPtr magic_cookie, byte[] buffer, int length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr magic_error(IntPtr magic_cookie);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
            public static extern int magic_setflags(IntPtr magic_cookie, MagicFlags flags);
            
            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
            public static extern int magic_version();
        }
    }

    /// <inheritdoc />
    public class MagicException : Exception
    {
        /// <inheritdoc />
        public MagicException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public MagicException(string message, string additionalInfo) : base(message ?? additionalInfo)
        {
        }
    }
    
    /// <summary>
    /// Open flags for libmagic 
    /// </summary>
    [Flags]
    public enum MagicFlags
    {
        /// <summary>
        /// No special handling.
        /// </summary>
        MAGIC_NONE = 0x000000,

        /// <summary>
        /// Print debugging messages to stderr.
        /// </summary>
        MAGIC_DEBUG = 0x000001,

        /// <summary>
        /// If the file queried is a symlink, follow it.
        /// </summary>
        MAGIC_SYMLINK = 0x000002,

        /// <summary>
        /// If the file is compressed, unpack it and look at the contents.
        /// </summary>
        MAGIC_COMPRESS = 0x000004,

        /// <summary>
        /// If the file is a block or character special device, then 
        /// open the device and try to look in its contents.
        /// </summary>
        MAGIC_DEVICES = 0x000008,

        /// <summary>
        /// Return a MIME type string, instead of a textual description.
        /// </summary>
        MAGIC_MIME_TYPE = 0x000010,

        /// <summary>
        /// Return all matches, not just the first.
        /// </summary>
        MAGIC_CONTINUE = 0x000020,

        /// <summary>
        /// Check the magic database for consistency and print warnings to stderr.
        /// </summary>
        MAGIC_CHECK = 0x000040,

        /// <summary>
        /// On systems that support utime(3) or utimes(2), attempt to 
        /// preserve the access time of files analysed.
        /// </summary>
        MAGIC_PRESERVE_ATIME = 0x000080,

        /// <summary>
        /// Don't translate unprintable characters to a \ooo octal representation.
        /// </summary>
        MAGIC_RAW = 0x000100,

        /// <summary>
        /// Treat operating system errors while trying to open files 
        /// and follow symlinks as real errors, instead of printing 
        /// them in the magic buffer.
        /// </summary>
        MAGIC_ERROR = 0x000200,

        /// <summary>
        /// Return a MIME encoding, instead of a textual description.
        /// </summary>
        MAGIC_MIME_ENCODING = 0x000400,

        /// <summary>
        /// A shorthand for MAGIC_MIME_TYPE | MAGIC_MIME_ENCODING.
        /// </summary>
        MAGIC_MIME = (MAGIC_MIME_TYPE|MAGIC_MIME_ENCODING),

        /// <summary>
        /// Return the Apple creator and type.
        /// </summary>
        MAGIC_APPLE = 0x000800,

        /// <summary>
        /// Return a slash-separated list of extensions for this file type.
        /// </summary>
        MAGIC_EXTENSION = 0x1000000,

        /// <summary>
        /// Don't report on compression, only report about the uncompressed data.
        /// </summary>
        MAGIC_COMPRESS_TRANSP = 0x200000,

        /// <summary>
        /// A shorthand for (MAGIC_EXTENSION|MAGIC_MIME|MAGIC_APPLE)
        /// </summary>
        MAGIC_NODESC = (MAGIC_EXTENSION|MAGIC_MIME|MAGIC_APPLE),

        /// <summary>
        /// Don't look inside compressed files.
        /// </summary>
        MAGIC_NO_CHECK_COMPRESS = 0x001000,

        /// <summary>
        /// Don't examine tar files.
        /// </summary>
        MAGIC_NO_CHECK_TAR = 0x002000,

        /// <summary>
        /// Don't consult magic files.
        /// </summary>
        MAGIC_NO_CHECK_SOFT = 0x004000,

        /// <summary>
        /// Don't check for EMX application type (only on EMX).
        /// </summary>
        MAGIC_NO_CHECK_APPTYPE = 0x008000,

        /// <summary>
        /// Don't print ELF details.
        /// </summary>
        MAGIC_NO_CHECK_ELF = 0x010000,

        /// <summary>
        /// Don't check for various types of text files.
        /// </summary>
        MAGIC_NO_CHECK_TEXT = 0x020000,

        /// <summary>
        /// Don't get extra information on MS Composite Document Files.
        /// </summary>
        MAGIC_NO_CHECK_CDF = 0x040000,

        /// <summary>
        /// Don't look for known tokens inside ascii files.
        /// </summary>
        MAGIC_NO_CHECK_TOKENS = 0x100000,

        /// <summary>
        /// Don't check text encodings.
        /// </summary>
        MAGIC_NO_CHECK_ENCODING = 0x200000
    }
}
