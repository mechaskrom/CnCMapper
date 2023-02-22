using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Intermediary raw unparsed file content used to parse into specific file formats.
    class FileProto
    {
        private readonly string mName;
        private readonly string mNameUpper; //Name in upper case.
        private readonly string mId; //Name without extension in upper case.
        private readonly string mExt; //Extension in upper case.
        private readonly Stream mStream; //Keep open and let creator or program exit close it.
        private readonly long mStart; //Stream start position.
        private readonly long mLength; //Stream/file length.
        private readonly string mArchivePath; //Directory path including any parent archive. Shouldn't end with a path separator.

        public FileProto(string name, Stream stream, string archivePath)
            : this(name, stream, 0, stream.Length, archivePath)
        {
        }

        public FileProto(string name, Stream stream, long start, long length, string archivePath)
        {
            mName = name;
            getIdAndExt(name, out mNameUpper, out mId, out mExt);
            mStream = stream;
            mStart = start;
            mLength = length;
            mArchivePath = archivePath;
        }

        public FileProto(FileProto copyFrom) //Makes a deep copy. Whole stream is read to memory.
        {
            mName = copyFrom.mName;
            mNameUpper = copyFrom.mNameUpper;
            mId = copyFrom.mId;
            mExt = copyFrom.mExt;
            byte[] fileBytes = copyFrom.readAllBytes();
            mStream = new MemoryStream(fileBytes);
            mStart = 0;
            mLength = fileBytes.Length;
            mArchivePath = "InternalCopy";
        }

        public static FileProto create(string filePath) //Throws any exception.
        {
            return create(filePath, false);
        }

        public static FileProto create(string filePath, bool placeInMemory) //Throws any exception.
        {
            Stream stream = null;
            try
            {
                if (placeInMemory) //Read whole file to memory i.e. don't lock the file on disk?
                {
                    stream = new MemoryStream(File.ReadAllBytes(filePath));
                }
                else
                {
                    stream = File.OpenRead(filePath);
                }
                string name = Path.GetFileName(filePath);
                string archivePath = Path.GetDirectoryName(filePath);
                return new FileProto(name, stream, archivePath);
            }
            catch
            {
                if (stream != null) //Dispose stream on exceptions.
                {
                    stream.Dispose();
                }
                throw;
            }
        }

        protected static void getIdAndExt(string name, out string nameUpper, out string id, out string ext)
        {
            //Returns name, id and ext in upper case.
            nameUpper = name.ToUpperInvariant();
            int dotInd = nameUpper.LastIndexOf('.');
            if (dotInd < 0) //No file extension?
            {
                id = nameUpper;
                ext = string.Empty;
            }
            else
            {
                id = nameUpper.Substring(0, dotInd);
                ext = nameUpper.Substring(dotInd + 1);
            }
        }

        public string Name
        {
            get { return mName; }
        }

        public string NameUpper //Name in upper case.
        {
            get { return mNameUpper; }
        }

        public string Id //Name without extension in upper case.
        {
            get { return mId; }
        }

        public string Ext //Extension in upper case.
        {
            get { return mExt; }
        }

        public long Start //Stream start position.
        {
            get { return mStart; }
        }

        public long Length //Stream/file length.
        {
            get { return mLength; }
        }

        public string ArchivePath //Directory path including any parent archive. Shouldn't end with a path separator.
        {
            get { return mArchivePath; }
        }

        public override string ToString()
        {
            return Name;
        }

        public Stream getStream(long offset) //Offset from start position.
        {
            mStream.Seek(mStart + offset, SeekOrigin.Begin);
            return mStream;
        }

        public byte[] readAllBytes()
        {
            return getStream(0).readArray(Length);
        }

        public static bool isContentEqual(FileProto fileProto1, FileProto fileProto2)
        {
            if (fileProto1.Length != fileProto2.Length)
            {
                return false;
            }
            Stream stream1 = fileProto1.getStream(0);
            Stream stream2 = fileProto2.getStream(0);
            for (long i = 0, length = fileProto1.Length; i < length; i++)
            {
                if (stream1.ReadByte() != stream2.ReadByte())
                {
                    return false;
                }
            }
            return true;
        }

        public void debugSaveContent(string folderPath)
        {
            debugSaveContent(folderPath, Name);
        }

        public void debugSaveContent(string folderPath, string fileName)
        {
            Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(Path.Combine(folderPath, fileName), readAllBytes());
        }
    }

    static class FileProtoExt
    {
        public static T parseAs<T>(this FileProto fileProto) //Throws any exception.
            where T : FileBase, new()
        {
            if (fileProto == null)
            {
                return null;
            }
            T t = new T();
            t.parseInit(fileProto);
            return t;
        }

        public static T tryParseAs<T>(this FileProto fileProto) //Returns null on exception.
            where T : FileBase, new()
        {
            try
            {
                return parseAs<T>(fileProto);
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<T> tryParseAs<T>(this IEnumerable<FileProto> fileProtos) //Returns what could be parsed.
            where T : FileBase, new()
        {
            return tryParseAs<T>(fileProtos, null);
        }

        public static IEnumerable<T> tryParseAs<T>(this IEnumerable<FileProto> fileProtos, string gfxFileExt) //Returns what could be parsed.
            where T : FileBase, new()
        {
            if (gfxFileExt != null)
            {
                gfxFileExt = gfxFileExt.ToUpperInvariant();
            }
            T t = new T(); //Used to check extension with.
            foreach (FileProto fp in fileProtos)
            {
                if (t.isSuitableExt(fp.Ext, gfxFileExt)) //Only try files with a suitable extension.
                {
                    T result = tryParseAs<T>(fp);
                    if (result != null)
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
