using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class FileDatabaseTD : FileDatabaseTDRA
    {
        private const DatSection DatSectionTD = DatSection.TiberianDawn;

        protected override Dictionary<uint, string> createFileDatabaseInner()
        {
            return createFileDatabase(DatSectionTD);
        }

        public static void debugSaveEntries()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<UInt32, string> fileDatabase = createFileDatabase(DatSectionTD);
            foreach (KeyValuePair<UInt32, string> pair in fileDatabase)
            {
                sb.AppendLine("{0}={1}", pair.Key.ToString("X8"), pair.Value);
            }
            File.WriteAllText(Program.ProgramPath + "file database entries TD.txt", sb.ToString());
        }
    }
}
