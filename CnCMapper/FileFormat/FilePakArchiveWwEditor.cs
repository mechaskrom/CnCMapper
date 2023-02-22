using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    partial class FilePakArchiveWw
    {
        //Simple PAK-file editor. Mostly created for replacing mission INI-files in SCENARIO.PAK with tests.
        public class Editor : IFileContainer
        {
            private readonly FileInfo mSourceInfo;
            private readonly Dictionary<string, DataEntry> mDataEntries;

            private class DataEntry
            {
                private readonly string mName;
                private byte[] mData;

                public DataEntry(string name, byte[] data)
                {
                    mName = name;
                    mData = data;
                }

                public string Name
                {
                    get { return mName; }
                }

                public byte[] Data
                {
                    get { return mData; }
                    set { mData = value; }
                }
            }

            private Editor(FileInfo sourceInfo, Dictionary<string, DataEntry> dataEntries)
            {
                mSourceInfo = sourceInfo;
                mDataEntries = dataEntries;
            }

            public string Name
            {
                get { return mSourceInfo.Name; }
            }

            public string FullName
            {
                get { return mSourceInfo.FullName.TrimEnd(Path.DirectorySeparatorChar); }
            }

            public string GfxFileExt //Not applicable to PAK files.
            {
                get { return null; }
            }

            public static Editor open(string path)
            {
                FileInfo sourceInfo = new FileInfo(path);
                FilePakArchiveWw source = new FilePakArchiveWw(sourceInfo.FullName);
                Dictionary<string, DataEntry> dataEntries = new Dictionary<string, DataEntry>();
                foreach (FileProto file in source.getFiles())
                {
                    dataEntries.Add(file.Name, new DataEntry(file.Name, file.readAllBytes()));
                }
                return new Editor(sourceInfo, dataEntries);
            }

            public void save()
            {
                save(mSourceInfo.FullName);
            }

            public void save(string path) //Replaces file if it exists.
            {
                using (FileStream fs = File.Create(path))
                {
                    DataEntry[] dataEntries = mDataEntries.Values.ToArray();

                    //Write header file entries.
                    int offset = calculateHeaderLength(dataEntries);
                    foreach (DataEntry de in dataEntries)
                    {
                        fs.writeUInt32((UInt32)offset);
                        fs.writeStringZ(de.Name);
                        offset += de.Data.Length;
                    }

                    //Write V2 end-of-file-entries flag.
                    fs.writeUInt32(0);

                    //Write data.
                    foreach (DataEntry de in dataEntries)
                    {
                        fs.writeArray(de.Data);
                    }
                }
            }

            private static int calculateHeaderLength(DataEntry[] dataEntries)
            {
                //Calculate length of PAK-file header i.e. length of all file entries in bytes.
                //Each file entry has a 32bit offset and a zero terminated string.
                int length = 0;
                foreach (DataEntry de in dataEntries)
                {
                    length += 4; //UInt32 offset.
                    length += de.Name.Length + 1; //Name + null terminator.
                }

                //V2 end-of-file-entries flag.
                length += 4;

                return length;
            }

            private DataEntry findDataEntry(string name) //Returns null if file is not found.
            {
                DataEntry dataEntry;
                if (mDataEntries.TryGetValue(name, out dataEntry))
                {
                    return dataEntry;
                }
                return null;
            }

            private DataEntry getDataEntry(string name) //Throws if file is not found.
            {
                DataEntry dataEntry = findDataEntry(name);
                if (dataEntry == null)
                {
                    throw new ArgumentException(string.Format("Couldn't find file '{0}' in edited PAK-file '{1}'!",
                        name, mSourceInfo.FullName));
                }
                return dataEntry;
            }

            public bool hasFile(string name)
            {
                return findDataEntry(name) != null;
            }

            public FileProto findFile(string name) //Returns null if file is not found.
            {
                DataEntry dataEntry = findDataEntry(name);
                if (dataEntry != null)
                {
                    return createFile(dataEntry);
                }
                return null;
            }

            public FileProto getFile(string name) //Throws if file is not found.
            {
                return createFile(getDataEntry(name));
            }

            public IEnumerable<FileProto> getFiles()
            {
                foreach (DataEntry de in mDataEntries.Values)
                {
                    yield return createFile(de);
                }
            }

            public void add(string nameToAdd, string pathToSourceFile)
            {
                add(nameToAdd, File.ReadAllBytes(pathToSourceFile));
            }

            public void add(string nameToAdd, byte[] file)
            {
                mDataEntries.Add(nameToAdd, new DataEntry(nameToAdd, file));
            }

            public void remove(string nameToRemove)
            {
                mDataEntries.Remove(nameToRemove);
            }

            public void replace(string nameToReplace, string pathToSourceFile)
            {
                replace(nameToReplace, File.ReadAllBytes(pathToSourceFile));
            }

            public void replace(string nameToReplace, byte[] file)
            {
                getDataEntry(nameToReplace).Data = file;
            }

            private FileProto createFile(DataEntry dataEntry)
            {
                MemoryStream stream = new MemoryStream(dataEntry.Data);
                return new FileProto(dataEntry.Name, stream, mSourceInfo.FullName);
            }
        }
    }
}
