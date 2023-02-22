using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace CnCMapper.FileFormat
{
    abstract class FileBase
    {
        private FileProto mFileProto = null; //Set when file is parsed and never changed afterwards.

        public enum Origin //Info about how/why file was created.
        {
            Normal,
            Missing, //File was missing so this was created instead.
            Other,
        }
        protected Origin mOrigin = Origin.Normal;

        protected FileBase()
        {
        }

        protected FileBase(string filePath)
        {
            FileProto fileProto = null;
            try
            {
                fileProto = FileProto.create(filePath);
                parseInit(fileProto);
            }
            catch
            {
                if (fileProto != null) //Make sure stream is disposed on exceptions.
                {
                    Stream stream = fileProto.getStream(0);
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }
                throw;
            }
        }

        protected FileBase(FileProto fileProto)
        {
            parseInit(fileProto);
        }

        public abstract bool isSuitableExt(string ext, string gfxFileExt); //Extensions should be in upper case.

        protected abstract void parseInit(Stream stream);

        public void parseInit(FileProto fileProto) //Should be called only once per file.
        {
            System.Diagnostics.Debug.Assert(mFileProto == null, "File already parsed!");
            if (mFileProto == null)
            {
                mFileProto = fileProto;
                parseInit(fileProto.getStream(0));
            }
        }

        public string Name
        {
            get { return mFileProto.Name; }
        }

        public string NameUpper //Name in upper case.
        {
            get { return mFileProto.NameUpper; }
        }

        public string Id //Name without extension in upper case.
        {
            get { return mFileProto.Id; }
        }

        public string Ext //Extension in upper case.
        {
            get { return mFileProto.Ext; }
        }

        public long Start
        {
            get { return mFileProto.Start; }
        }

        public long Length
        {
            get { return mFileProto.Length; }
        }

        public string FullPath //Directory path including any parent archive.
        {
            get { return mFileProto.ArchivePath; }
        }

        public string FullName //Full path + file name.
        {
            get { return FullPath + Path.DirectorySeparatorChar + Name; }
        }

        public bool IsOriginMissing
        {
            get { return mOrigin == Origin.Missing; }
        }

        protected Stream getStream(long offset)
        {
            return mFileProto.getStream(offset);
        }

        public byte[] readAllBytes()
        {
            return mFileProto.readAllBytes();
        }

        protected void checkFileLengthMin(long minLength)
        {
            checkFileLengthMin(minLength, Length);
        }

        protected void checkFileLengthMin(long minLength, long fileLength)
        {
            if (minLength > fileLength)
            {
                throwParseError(string.Format("File length '{0}' is too short to parse successfully!", fileLength));
            }
        }

        protected void checkFileLengthExpected(long expectedLength)
        {
            if (expectedLength != Length)
            {
                throwParseError(string.Format("File length '{0}' should be '{1}'!", Length, expectedLength));
            }
        }

        protected void checkHeaderFileLength(long headerLength)
        {
            if (headerLength != Length)
            {
                throwParseError(string.Format("File length in header '{0}' doesn't match '{1}'!", headerLength, Length));
            }
        }

        protected void checkOffset(long offset, long bytesToParse) //Check that offset+bytesToParse is within file length.
        {
            if ((offset + bytesToParse) > Length)
            {
                throwParseError(string.Format("File offset '{0}' plus '{1}' bytes to parse isn't within file length '{2}'!", offset, bytesToParse, Length));
            }
        }

        protected void warn(string message)
        {
            Program.warn(addFileInfo(message));
        }

        public ArgumentException newArgError(string message)
        {
            return new ArgumentException(addFileInfo(message));
        }

        protected InvalidDataException newParseError(string message)
        {
            return new InvalidDataException(addFileInfo(message));
        }

        protected void throwParseError(string message)
        {
            throw newParseError(message);
        }

        protected string addFileInfo(string message) //Add info about this file to the end of a message.
        {
            return message + " In '" + this.GetType().Name + "' at '" + FullName + "'.";
        }

        public static bool isContentEqual(FileBase fileBase1, FileBase fileBase2)
        {
            return (fileBase1 == null && fileBase2 == null) ||
                (fileBase1 != null && fileBase2 != null &&
                FileProto.isContentEqual(fileBase1.mFileProto, fileBase2.mFileProto));
        }

        public override string ToString()
        {
            return Name;
        }

        public void debugSaveContent(string folderPath)
        {
            mFileProto.debugSaveContent(folderPath, Name);
        }

        public void debugSaveContent(string folderPath, string fileName)
        {
            mFileProto.debugSaveContent(folderPath, fileName);
        }
    }

    static class FileBaseExt
    {
        public static bool isContentEqual(this FileBase fileBase1, FileBase fileBase2)
        {
            return FileBase.isContentEqual(fileBase1, fileBase2);
        }
    }
}
