using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //File archive. Used to group files together.
    //https://moddingwiki.shikadi.net/wiki/PAK_Format_(Westwood)
    partial class FilePakArchiveWw : FileBase, IFileContainer
    {
        //Layout: file_entries,data.
        //file entry:
        //-offset UInt32: File's starting offset in the PAK archive.
        //-name char[x]: File name (8.3 style), null terminated, variable width.

        //There exists a few different file entries layout versions.
        private enum Version
        {
            V1,
            V2,
            V3,
        }

        private struct FileEntry
        {
            public UInt32 offset; //Within file i.e. file entry header is included.
            public string name;
            public UInt32 length; //Not stored in PAK archive. Must be calculated.
        }
        private FileEntry[] mFileEntries;

        private string mFullPathFileEntry;
        private Version mVersion;

        public FilePakArchiveWw()
        {
        }

        public FilePakArchiveWw(string filePath)
            : base(filePath)
        {
        }

        public FilePakArchiveWw(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "PAK";
        }

        protected override void parseInit(Stream stream)
        {
            //File entries.
            checkFileLengthMin(4); //File contains at least one 32-bit offset value?
            List<FileEntry> entries = new List<FileEntry>();
            long startPosition = stream.Position;
            UInt32 firstOffset = stream.readUInt32();
            long endPosition = startPosition + firstOffset;
            for (UInt32 offset = firstOffset; ; offset = stream.readUInt32())
            {
                if (offset == 0)
                {
                    //Found an end-of-file-entries flag, used in V2 and V3.
                    if (entries.Count > 0 && entries.Last().name == "") //Previous entry had no name? Used in V3.
                    {
                        mVersion = Version.V3;
                        //Last entry's offset always equal to file length in V3?
                        System.Diagnostics.Debug.Assert(entries.Last().offset == Length,
                            "PAK V3 last entry's offset not equal to file length! Invalid file?");
                    }
                    else
                    {
                        mVersion = Version.V2;
                    }
                    break;
                }
                else if (stream.Position >= endPosition)
                {
                    //Stream position reached first offset without any 0 offset found. Probably V1.
                    mVersion = Version.V1;
                    //Last offset usually, but not always, equal to file length in V1?
                    System.Diagnostics.Debug.Assert(offset == Length,
                        "PAK V1 last offset read not equal to file length! Invalid/incorrect file?");
                    break;
                }

                //This seems to be a normal file entry.
                FileEntry fe = new FileEntry();
                fe.offset = offset;
                fe.name = stream.readString();
                entries.Add(fe);
            }

            //Add a V3 style file entry ending (empty name and offset set to file length).
            //Makes it easier to calculate length of files in the code after.
            if (mVersion == Version.V1 || mVersion == Version.V2)
            {
                FileEntry fe = new FileEntry();
                fe.name = "";
                fe.offset = (UInt32)Length;
                entries.Add(fe);
            }

            if (mVersion != Version.V2) //Only V2 PAK-files (used in Dune 2) are supported for now.
            {
                throwParseError(string.Format("Unsupported version '{0}'!", mVersion));
                //TODO: Maybe support/test other PAK-file versions too?
            }

            //Store file entries and calculate their file length. Assumes there is a V3 ending entry at the end.
            FileEntry[] fileEntries = mFileEntries = new FileEntry[entries.Count - 1]; //Ignore V3 ending entry.
            for (int i = 0; i < fileEntries.Length; i++)
            {
                FileEntry fe = entries[i];
                fileEntries[i].offset = fe.offset;
                fileEntries[i].name = fe.name;
                fileEntries[i].length = entries[i + 1].offset - fe.offset; //Calculate file length.
            }
            mFullPathFileEntry = FullName;
        }

        public string GfxFileExt //Not applicable to PAK files.
        {
            get { return null; }
        }

        public bool hasFile(string name)
        {
            foreach (FileEntry fe in mFileEntries)
            {
                if (fe.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public FileProto findFile(string name) //Returns null if file is not found.
        {
            foreach (FileEntry fe in mFileEntries)
            {
                if (fe.name == name)
                {
                    return createFile(fe);
                }
            }
            return null;
        }

        public FileProto getFile(string name) //Throws if file is not found.
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
                yield return createFile(fe);
            }
        }

        private FileProto createFile(FileEntry fe)
        {
            return new FileProto(fe.name, getStream(0), Start + fe.offset, fe.length, mFullPathFileEntry);
        }

        public void debugSavePakFileEntries()
        {
            StringBuilder sb = new StringBuilder();
            foreach (FileEntry fe in mFileEntries)
            {
                sb.AppendLine("Name={0,10}, Offset={1,8}, RealOffset={2,8}, Length={3,8}",
                    fe.name, fe.offset, Start + fe.offset, fe.length);
            }
            string folderPath = Program.DebugOutPath + "pak\\";
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(folderPath + Name + " entries.txt", sb.ToString());
        }
    }
}
