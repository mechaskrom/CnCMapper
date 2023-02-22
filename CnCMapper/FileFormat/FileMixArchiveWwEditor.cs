using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    partial class FileMixArchiveWw
    {
        //Simple MIX-file editor. Mostly created for replacing mission INI-files in GENERAL.MIX with tests.
        public class Editor : IFileContainer
        {
            private readonly FileInfo mSourceInfo;
            private readonly string mGfxFileExt;
            private readonly Dictionary<UInt32, DataEntry> mDataEntries;

            private class DataEntry //Keep track of some data used when saving.
            {
                private readonly UInt32 mId;
                private byte[] mData;
                private UInt32 mDataOffset; //Set and used only when saving.

                public DataEntry(UInt32 id, byte[] data)
                {
                    mId = id;
                    mData = data;
                    mDataOffset = 0;
                }

                public UInt32 Id
                {
                    get { return mId; }
                }

                public byte[] Data
                {
                    get { return mData; }
                    set { mData = value; }
                }

                public UInt32 DataOffset //Set and used only when saving.
                {
                    get { return mDataOffset; }
                    set { mDataOffset = value; }
                }

                public UInt32 DataLength
                {
                    get { return (UInt32)mData.Length; }
                }

                public bool isDuplicateOf(DataEntry dataEntry)
                {
                    //Actually a bit faster in my tests than using a cached hash of data content. Probably
                    //few situations where a hash is faster. Like compact saving an editor more than once?
                    if (mData.Length != dataEntry.mData.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < mData.Length; i++)
                    {
                        if (mData[i] != dataEntry.mData[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

            private Editor(FileInfo sourceInfo, string gfxFileExt)
            {
                mSourceInfo = sourceInfo;
                mGfxFileExt = gfxFileExt;
                mDataEntries = new Dictionary<UInt32, DataEntry>();
            }

            public string Name
            {
                get { return mSourceInfo.Name; }
            }

            public string FullName
            {
                get { return mSourceInfo.FullName.TrimEnd(Path.DirectorySeparatorChar); }
            }

            public string GfxFileExt
            {
                get { return mGfxFileExt; }
            }

            public static Editor open(string path)
            {
                FileInfo sourceInfo = new FileInfo(path);
                FileMixArchiveWw source = new FileMixArchiveWw(sourceInfo.FullName);
                Editor editor = new Editor(sourceInfo, source.GfxFileExt);
                using (Stream stream = source.getStream(0))
                {
                    foreach (FileEntry fe in source.mFileEntries)
                    {
                        stream.Seek(source.mDataPosition + fe.offset, SeekOrigin.Begin);
                        byte[] feData = stream.readArray(fe.length);
                        editor.mDataEntries.Add(fe.id, new DataEntry(fe.id, feData));
                    }
                }
                return editor;
            }

            public void save()
            {
                save(mSourceInfo.FullName);
            }

            public void save(string path)
            {
                save(path, false);
            }

            public void saveCompact(string path)
            {
                //Removes duplicate entries by pointing their offset to the same data. Can reduce
                //file length, but duplicates are usually rare and this will make save slower.
                save(path, true);
            }

            private void save(string path, bool doCompact) //Replaces file if it exists.
            {
                using (FileStream fs = File.Create(path))
                {
                    List<DataEntry> dataEntries = mDataEntries.Values.ToList();

                    //Write file entries.
                    UInt32 dataLengthTotal = writeFileEntries(fs, dataEntries, doCompact);

                    //Write data.
                    long dataStart = HeaderLength + (FileEntryLength * dataEntries.Count);
                    System.Diagnostics.Debug.Assert(fs.Position == dataStart);
                    foreach (DataEntry dataNext in dataEntries)
                    {
                        //Write data if not a duplicate entry i.e. offset is not to already written data.
                        if (!doCompact || dataNext.DataOffset + dataStart >= fs.Position)
                        {
                            fs.writeArray(dataNext.Data);
                        }
                    }
                    System.Diagnostics.Debug.Assert(fs.Position == dataLengthTotal + dataStart);

                    //Write header.
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.writeUInt16((UInt16)dataEntries.Count);
                    fs.writeUInt32(dataLengthTotal);
                }
            }

            private static UInt32 writeFileEntries(Stream stream, List<DataEntry> dataEntries, bool doCompact)
            {
                stream.Seek(HeaderLength, SeekOrigin.Begin);
                UInt32 dataLengthTotal = 0;
                for (int i = 0; i < dataEntries.Count; i++)
                {
                    DataEntry dataNext = dataEntries[i];
                    dataNext.DataOffset = dataLengthTotal;
                    UInt32 addLengthTotal = dataNext.DataLength;
                    if (doCompact) //Look for duplicates?
                    {
                        //Check if next entry is a duplicate of a previously added entry.
                        for (int j = 0; j < i; j++)
                        {
                            DataEntry dataPrev = dataEntries[j];
                            if (dataNext.isDuplicateOf(dataPrev)) //Duplicate?
                            {
                                //Set offset to the previously added entry.
                                dataNext.DataOffset = dataPrev.DataOffset;
                                addLengthTotal = 0; //Don't increase total data length.
                                break;
                            }
                        }
                    }
                    //Write file entry.
                    stream.writeUInt32(dataNext.Id); //Id.
                    stream.writeUInt32(dataNext.DataOffset); //Offset.
                    stream.writeUInt32(dataNext.DataLength); //Length.
                    dataLengthTotal += addLengthTotal;
                }
                return dataLengthTotal;
            }

            private DataEntry findDataEntry(string name) //Returns null if file is not found. Case is ignored.
            {
                UInt32 id = FileDatabase.toFileId(name);
                DataEntry dataEntry;
                if (mDataEntries.TryGetValue(id, out dataEntry))
                {
                    return dataEntry;
                }
                return null;
            }

            private DataEntry getDataEntry(string name) //Throws if file is not found. Case is ignored.
            {
                DataEntry dataEntry = findDataEntry(name);
                if (dataEntry == null)
                {
                    throw new ArgumentException(string.Format("Couldn't find file '{0}' in edited MIX-file '{1}'!",
                        name, mSourceInfo.FullName));
                }
                return dataEntry;
            }

            public bool hasFile(string name) //Case is ignored.
            {
                return findDataEntry(name) != null;
            }

            public FileProto findFile(string name) //Returns null if file is not found. Case is ignored.
            {
                DataEntry dataEntry = findDataEntry(name);
                if (dataEntry != null)
                {
                    return createFile(name, dataEntry);
                }
                return null;
            }

            public FileProto getFile(string name) //Throws if file is not found. Case is ignored.
            {
                return createFile(name, getDataEntry(name));
            }

            public IEnumerable<FileProto> getFiles()
            {
                foreach (DataEntry de in mDataEntries.Values)
                {
                    yield return createFile(FileDatabase.toFileName(de.Id), de);
                }
            }

            public void add(string nameToAdd, string pathToSourceFile)
            {
                add(nameToAdd, File.ReadAllBytes(pathToSourceFile));
            }

            public void add(string nameToAdd, byte[] file)
            {
                UInt32 id = FileDatabase.toFileId(nameToAdd);
                mDataEntries.Add(id, new DataEntry(id, file));
            }

            public void remove(string nameToRemove)
            {
                UInt32 id = getDataEntry(nameToRemove).Id;
                mDataEntries.Remove(id);
            }

            public void replace(string nameToReplace, string pathToSourceFile)
            {
                replace(nameToReplace, File.ReadAllBytes(pathToSourceFile));
            }

            public void replace(string nameToReplace, byte[] file)
            {
                getDataEntry(nameToReplace).Data = file;
            }

            private FileProto createFile(string name, DataEntry dataEntry)
            {
                MemoryStream stream = new MemoryStream(dataEntry.Data);
                return new FileProto(name, stream, mSourceInfo.FullName);
            }
        }
    }
}
