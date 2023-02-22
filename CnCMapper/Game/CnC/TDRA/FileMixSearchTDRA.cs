using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    //Not sure how Tiberian Dawn and Red Alert searches for files, but they seem to look in game folder first
    //and then in MIX-files. So this helper class will search a folder first and then a specified MIX-file.
    class FileSearch<TFile> : FileSearch<TFile, FileMixArchiveWw>
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

        public FileSearch(string name, FileMixSearch fileMixSearch)
            : base(name, fileMixSearch)
        {
        }

        public FileSearch(string name, string folderPath, FileMixSearch fileMixSearch)
            : base(name, folderPath, fileMixSearch)
        {
        }
    }

    class FileMixSearch : ArchiveSearch<FileMixArchiveWw>
    {
        public FileMixSearch(string name)
            : base(name)
        {
        }

        public FileMixSearch(string name, string folderPath)
            : base(name, folderPath)
        {
        }

        public FileMixSearch(string name, FileMixSearch fileMixSearch)
            : base(name, fileMixSearch)
        {
        }

        public FileMixSearch(string name, string folderPath, FileMixSearch fileMixSearch)
            : base(name, folderPath, fileMixSearch)
        {
        }
    }
}
