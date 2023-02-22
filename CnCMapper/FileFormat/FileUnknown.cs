using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //Special class for files with an unknown file ID i.e. not found in a file database.
    class FileUnknown : FileBase
    {
        public const string Extension = "UNK";

        public FileUnknown()
        {
        }

        public FileUnknown(string filePath)
            : base(filePath)
        {
        }

        public FileUnknown(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == Extension;
        }

        protected override void parseInit(Stream stream)
        {
        }

        public void debugSaveContent()
        {
            Program.message(addFileInfo("Unknown file content save."));
            debugSaveContent(Program.DebugOutPath + "unk\\");
        }
    }
}
