using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //General file for unsupported file formats.
    class FileOther : FileBase
    {
        public FileOther()
        {
        }

        public FileOther(string filePath)
            : base(filePath)
        {
        }

        public FileOther(FileProto fileProto)
            : base(fileProto)
        {
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return true;
        }

        protected override void parseInit(Stream stream)
        {
        }

        public void debugSaveContent()
        {
            debugSaveContent(Program.DebugOutPath + "other\\");
        }
    }
}
