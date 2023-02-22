using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game
{
    //Search for a file in a folder first and then inside an optional archive file.
    class FileSearch<TFile, TArchive>
        where TFile : FileBase, new()
        where TArchive : FileBase, IFileContainer, new()
    {
        private readonly string mName; //Name of file to find.
        private readonly string mFolderPath; //Folder to look in first. Use game folder if null.
        private readonly ArchiveSearch<TArchive> mArchiveSearch; //Optional archive to look in if not found in folder.
        private TFile mResult;
        private bool mIsSearched; //File already searched for?

        //Can't use game folder in constructors because it may not have been set yet. Use null instead.

        public FileSearch(string name)
            : this(name, null, null)
        {
        }

        public FileSearch(string name, string folderPath)
            : this(name, folderPath, null)
        {
        }

        public FileSearch(string name, ArchiveSearch<TArchive> archiveSearch)
            : this(name, null, archiveSearch)
        {
        }

        public FileSearch(string name, string folderPath, ArchiveSearch<TArchive> archiveSearch)
        {
            mName = name;
            mFolderPath = folderPath;
            mArchiveSearch = archiveSearch;
        }

        public TFile find() //Null if not found.
        {
            if (!mIsSearched)
            {
                string path = Path.Combine(getFolderPath(), mName);
                if (File.Exists(path)) //Look in folder first.
                {
                    mResult = FileProto.create(path).parseAs<TFile>();
                }
                else if (mArchiveSearch != null) //Look in specified archive-file.
                {
                    TArchive archive = mArchiveSearch.find();
                    if (archive != null)
                    {
                        mResult = archive.findFile(mName).parseAs<TFile>();
                    }
                }
                mIsSearched = true;
            }
            return mResult;
        }

        public TFile get() //Throws if not found.
        {
            find();
            if (mResult == null)
            {
                throw new FileNotFoundException(string.Format("Couldn't find file '{0}' inside '{1}'!",
                    mName, getSearchPath(mArchiveSearch, getFolderPath())));
            }
            return mResult;
        }

        private string getFolderPath()
        {
            return mFolderPath != null ? mFolderPath : Program.GamePath;
        }

        private static string getSearchPath(ArchiveSearch<TArchive> archiveSearch, string folderPath)
        {
            //Return inner path of archive-file if it exists, else the folder path.
            string searchPath = folderPath;
            if (archiveSearch != null) //Search has an archive-file?
            {
                TArchive archive = archiveSearch.find();
                if (archive != null) //Does it exist?
                {
                    searchPath = archive.FullPath; //Get its inner path.
                }
            }
            return searchPath;
        }
    }

    class ArchiveSearch<TArchive> : FileSearch<TArchive, TArchive>
        where TArchive : FileBase, IFileContainer, new()
    {
        public ArchiveSearch(string name)
            : base(name)
        {
        }

        public ArchiveSearch(string name, string folderPath)
            : base(name, folderPath)
        {
        }

        public ArchiveSearch(string name, ArchiveSearch<TArchive> archiveSearch)
            : base(name, archiveSearch)
        {
        }

        public ArchiveSearch(string name, string folderPath, ArchiveSearch<TArchive> archiveSearch)
            : base(name, folderPath, archiveSearch)
        {
        }
    }
}
