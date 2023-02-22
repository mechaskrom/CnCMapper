using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    class FolderContainer : IFileContainer
    {
        private DirectoryInfo mDirectoryInfo;
        private FileInfo[] mFileInfos;

        public FolderContainer(string path)
        {
            mDirectoryInfo = new DirectoryInfo(path);
            mFileInfos = null;
        }

        public string Name
        {
            get { return mDirectoryInfo.Name; }
        }

        public string FullName
        {
            get { return mDirectoryInfo.FullName.TrimEnd(Path.DirectorySeparatorChar); }
        }

        public string GfxFileExt
        {
            get { return null; }
        }

        private FileInfo[] getFileInfos()
        {
            if (mFileInfos == null)
            {
                mFileInfos = mDirectoryInfo.GetFiles();
            }
            return mFileInfos;
        }

        public bool hasFile(string name)
        {
            return File.Exists(FullName + Path.DirectorySeparatorChar + name);
        }

        public FileProto findFile(string name) //Returns null if file is not found. Case is ignored.
        {
            foreach (FileInfo fileInfo in getFileInfos())
            {
                if (string.Equals(fileInfo.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return FileProto.create(fileInfo.FullName);
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
            throw new ArgumentException(string.Format("Couldn't find file '{0}' in folder '{1}'!", name, FullName));
        }

        public IEnumerable<FileProto> getFiles()
        {
            foreach (FileInfo fileInfo in getFileInfos())
            {
                yield return FileProto.create(fileInfo.FullName);
            }
        }
    }
}
