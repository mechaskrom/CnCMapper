using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //File archive. Used to group files together.
    //http://xhp.xwis.net/documents/MIX_Format.html
    //https://moddingwiki.shikadi.net/wiki/MIX_Format_(Westwood)
    partial class FileMixArchiveWw : FileBase, IFileContainer
    {
        public interface IFileDatabase
        {
            string toFileName(UInt32 fileId);
            UInt32 toFileId(string fileName);
        }
        public static IFileDatabase FileDatabase = null; //Must be set before any name<->id conversion is performed.

        //If encrypted then only <header,file_entries> part of MIX-file is encrypted.
        //Layout: [flags],[key],<header,file_entries>,data,[checksum].
        //flags UInt32: Optional flags if checksum/encryption is present.
        //key UInt8[80]: Optional key if encryption flag is set.
        //header:
        //-file count UInt16: Number of files in archive.
        //-data length UInt32: Length of data section.
        //file entry:
        //-id UInt32: Numerical id of file name.
        //-offset UInt32: Offset of file within data section.
        //-length UInt32: Length of file.
        //checksum UInt8[20]: Optional checksum last in file if checksum flag is set.

        private const int FlagsLength = 4;
        private const int HeaderLength = 6; //Normal unencrypted header length.
        private const int KeyLength = 80;
        private const int ChecksumLength = 20;
        private const int FileEntryLength = 12;
        private const int FlagsPresentMask = 0xFFFF;
        private const int FlagsHasChecksumMask = 0x010000;
        private const int FlagsIsEncryptedMask = 0x020000;

        private UInt32 mFlags;
        private byte[] mKey;
        private UInt16 mFileCount;
        private UInt32 mDataLength;

        private struct FileEntry
        {
            public UInt32 id;
            public UInt32 offset;
            public UInt32 length;
        }
        private FileEntry[] mFileEntries;
        private byte[] mChecksum;

        private long mDataPosition; //File data position in stream.
        private string mFullPathFileEntry;
        private string mGfxFileExt; //Theater graphic files use first three letters of MIX-file's name as extension.

        public FileMixArchiveWw()
        {
        }

        public FileMixArchiveWw(string filePath)
            : base(filePath)
        {
        }

        public FileMixArchiveWw(FileProto fileProto)
            : base(fileProto)
        {
        }

        private static MemoryStream mDummyStream = null;
        public static FileMixArchiveWw createDummy(string fileName, Origin origin)
        {
            if (mDummyStream == null)
            {
                //Write header.
                mDummyStream.Seek(0, SeekOrigin.Begin);
                mDummyStream.writeUInt16(0);
                mDummyStream.writeUInt32(0);
            }
            FileMixArchiveWw fileMix = new FileMixArchiveWw(new FileProto(fileName, mDummyStream, "InternalDummy"));
            fileMix.mOrigin = origin;
            return fileMix;
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "MIX";
        }

        protected override void parseInit(Stream stream)
        {
            //Flags.
            checkFileLengthMin(FlagsLength);
            mFlags = stream.readUInt32();
            if (IsEncrypted) //Header and file entries are encrypted?
            {
                //Key.
                checkFileLengthMin(FlagsLength + KeyLength);
                mKey = stream.readArray(KeyLength);

                long encryptedDataStart = stream.Position;

                //Key is two RSA encrypted blocks of 40 bytes each that after decryption
                //is concatenated to form a 56 byte blowfish key.
                byte[] blowfishKey = Crypt.BlowfishKey.decrypt(mKey);

                //Data after key until file data is encrypted with blowfish in electronic codebook (ECB)
                //mode i.e. a serie of 8 byte blocks that each need to be decrypted with blowfish.
                //The decrypted blocks concatenated form the header and file entries. The end is padded
                //with zeros if this data is not a multiple of 8 bytes.
                Crypt.Blowfish blowfish = new Crypt.Blowfish(blowfishKey);

                //Read file count from first decrypted block so we can calculate total
                //number of blocks needed to hold all encrypted header plus file entries.
                UInt16 fileCount = blowfish.decryptBlocks(stream, 1).readUInt16();

                //8 bytes per block, round up to whole blocks.
                int blockCount = ((HeaderLength + (fileCount * FileEntryLength)) + 7) / 8;
                stream.Seek(encryptedDataStart, SeekOrigin.Begin);
                MemoryStream ms = blowfish.decryptBlocks(stream, blockCount);
                parseHeader(ms, ms.Length);
            }
            else
            {
                if (!IsFlagsPresent)
                {
                    stream.Seek(Start, SeekOrigin.Begin);
                }
                parseHeader(stream, Length);
            }

            mDataPosition = stream.Position;
            long actualDataLength = Length - (mDataPosition - Start);

            //Checksum.
            if (HasChecksum)
            {
                checkFileLengthMin(ChecksumLength, actualDataLength); //Data section has room for a checksum?
                actualDataLength -= ChecksumLength;
                stream.Seek(Start + Length - ChecksumLength, SeekOrigin.Begin);
                mChecksum = stream.readArray(ChecksumLength);
            }

            mFullPathFileEntry = FullName;
            mGfxFileExt = Id.Substring(0, Math.Min(3, Id.Length));

            //Do warnings last after parse didn't throw any errors.
            if (mDataLength != actualDataLength) //Check data length value in header.
            {
                //Can't rely on value in header? Just warn if incorrect instead of throwing.
                warn(string.Format("Data length in header '{0}' doesn't match '{1}'!", mDataLength, actualDataLength));
            }
        }

        private void parseHeader(Stream stream, long fileLength)
        {
            parseHeader(this, stream, fileLength);
        }

        private static void parseHeader(FileMixArchiveWw fileMix, Stream stream, long fileLength)
        {
            //Header.
            fileMix.checkFileLengthMin(HeaderLength, fileLength);
            UInt16 fileCount = fileMix.mFileCount = stream.readUInt16();
            UInt32 dataLength = fileMix.mDataLength = stream.readUInt32();

            //File entries.
            fileMix.checkFileLengthMin(HeaderLength + (fileCount * FileEntryLength), fileLength);
            FileEntry[] fileEntries = fileMix.mFileEntries = new FileEntry[fileMix.mFileCount];
            for (int i = 0; i < fileEntries.Length; i++)
            {
                fileEntries[i].id = stream.readUInt32();
                fileEntries[i].offset = stream.readUInt32();
                fileEntries[i].length = stream.readUInt32();
            }
        }

        public bool IsFlagsPresent
        {
            get { return (mFlags & FlagsPresentMask) == 0; }
        }

        public bool IsEncrypted
        {
            get { return IsFlagsPresent && (mFlags & FlagsIsEncryptedMask) != 0; }
        }

        public bool HasChecksum
        {
            get { return IsFlagsPresent && (mFlags & FlagsHasChecksumMask) != 0; }
        }

        public string GfxFileExt
        {
            get { return mGfxFileExt; }
        }

        public bool hasFile(string name) //Case is ignored.
        {
            UInt32 fileId = FileDatabase.toFileId(name);
            foreach (FileEntry fe in mFileEntries)
            {
                if (fe.id == fileId)
                {
                    return true;
                }
            }
            return false;
        }

        public FileProto findFile(string name) //Returns null if file is not found. Case is ignored.
        {
            UInt32 fileId = FileDatabase.toFileId(name);
            foreach (FileEntry fe in mFileEntries)
            {
                if (fe.id == fileId)
                {
                    return createFile(name, fe);
                }
            }
            return null;
        }

        public FileProto getFile(string name) //Throws if file is not found. Case is ignored.
        {
            FileProto file = findFile(name);
            if (file != null)
            {
                return file;
            }
            throw newArgError(string.Format("Couldn't find file '{0}'!", name));
        }

        public IEnumerable<FileProto> getFiles()
        {
            foreach (FileEntry fe in mFileEntries)
            {
                yield return createFile(FileDatabase.toFileName(fe.id), fe);
            }
        }

        private FileProto createFile(string name, FileEntry fe)
        {
            return new FileProto(name, getStream(0), mDataPosition + fe.offset, fe.length, mFullPathFileEntry);
        }

        public void debugSaveMixFileEntries()
        {
            StringBuilder sb = new StringBuilder();
            foreach (FileEntry fe in mFileEntries)
            {
                sb.AppendLine("ID={0,8:X}, Offset={1,8}, RealOffset={2,8}, Length={3,8}, File={4,12}",
                    fe.id, fe.offset, mDataPosition + fe.offset, fe.length, FileDatabase.toFileName(fe.id));
            }
            string folderPath = Program.DebugOutPath + "mix\\";
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(folderPath + Name + " entries.txt", sb.ToString());
        }
    }
}
