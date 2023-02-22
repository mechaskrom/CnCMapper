using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class FileDatabaseRA : FileDatabaseTDRA
    {
        private const DatSection DatSectionRA = DatSection.RedAlert;

        protected override Dictionary<uint, string> createFileDatabaseInner()
        {
            return createFileDatabase(DatSectionRA);
        }

        public static void debugSaveEntries()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<UInt32, string> fileDatabase = createFileDatabase(DatSectionRA);
            foreach (KeyValuePair<UInt32, string> pair in fileDatabase)
            {
                sb.AppendLine("{0}={1}", pair.Key.ToString("X8"), pair.Value);
            }
            File.WriteAllText(Program.ProgramPath + "file database entries RA.txt", sb.ToString());
        }
    }
}
