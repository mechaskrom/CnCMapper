using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    class FileSearch<TFile> : FileSearch<TFile, FilePakArchiveWw>
        where TFile : FileBase, new()
    {
        public FileSearch(string name)
            : base(name)
        {
        }

        public FileSearch(string name, string folderPath)
            : base(name, folderPath)
        {
        }

        public FileSearch(string name, FilePakSearch filePakSearch)
            : base(name, filePakSearch)
        {
        }

        public FileSearch(string name, string folderPath, FilePakSearch filePakSearch)
            : base(name, folderPath, filePakSearch)
        {
        }
    }

    class FilePakSearch : ArchiveSearch<FilePakArchiveWw>
    {
        public FilePakSearch(string name)
            : base(name)
        {
        }

        public FilePakSearch(string name, string folderPath)
            : base(name, folderPath)
        {
        }

        public FilePakSearch(string name, FilePakSearch filePakSearch)
            : base(name, filePakSearch)
        {
        }

        public FilePakSearch(string name, string folderPath, FilePakSearch filePakSearch)
            : base(name, folderPath, filePakSearch)
        {
        }
    }
}
