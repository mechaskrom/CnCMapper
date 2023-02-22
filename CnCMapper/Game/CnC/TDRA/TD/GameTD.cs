using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    //Command & Conquer: Tiberian Dawn (TD).
    static class GameTD
    {
        //Cache some common MIX-files.
        public static readonly FileMixSearch FileMixGeneral = new FileMixSearch("GENERAL.MIX");
        public static readonly FileMixSearch FileMixDesert = new FileMixSearch("DESERT.MIX");
        //public static readonly FileMixSearch FileMixDesert = new FileMixSearch("DESERT.MIX", TestsTD.PrintPath); //Testing with modified theater.
        public static readonly FileMixSearch FileMixJungle = new FileMixSearch("JUNGLE.MIX");
        public static readonly FileMixSearch FileMixTemperat = new FileMixSearch("TEMPERAT.MIX");
        //public static readonly FileMixSearch FileMixTemperat = new FileMixSearch("TEMPERAT.MIX", TestsTD.PrintPath); //Testing with modified theater.
        public static readonly FileMixSearch FileMixWinter = new FileMixSearch("WINTER.MIX");
        public static readonly FileMixSearch FileMixConquer = new FileMixSearch("CONQUER.MIX");
        //public static readonly FileMixSearch FileMixConquer = new FileMixSearch("CONQUER.MIX", TestsTD.PrintPath); //Testing with modified sprites.
        //Added with Covert Operations expansion?
        public static readonly FileMixSearch FileMixSc000 = new FileMixSearch("SC-000.MIX");
        public static readonly FileMixSearch FileMixSc001 = new FileMixSearch("SC-001.MIX");

        public static ConfigTD Config = null;

        public static void addNeededFilesInfo(StringBuilder sb)
        {
            sb.AppendLine("A Command & Conquer aka Tiberian Dawn (TD) folder:");
            sb.AppendLine("-needs 'CONQUER.MIX'");
            sb.AppendLine("-optionally 'DESERT.MIX'");
            sb.AppendLine("-optionally 'TEMPERAT.MIX'");
            sb.AppendLine("-optionally 'WINTER.MIX'");
            sb.AppendLine("-maps (.INI+.BIN) searched for in 'GENERAL.MIX', 'SC-000.MIX', 'SC-001.MIX' and the folder");
            sb.AppendLine("-multiplayer maps should start with 'SCM' and end with '.INI|.MPR' e.g. 'SCM01EA.INI'");
        }

        public static bool init(FileIni fileConfig)
        {
            System.Diagnostics.Debug.Assert(Program.ProgramPath != null && Program.GamePath != null,
                "Program and game paths must be set before init!");

            FileMixArchiveWw.FileDatabase = new FileDatabaseTD(); //Set MIX-file database first.

            if (fileConfig != null) //Detect game in config file?
            {
                if (ConfigTD.isFileConfig(fileConfig))
                {
                    Config = ConfigTD.create(fileConfig);
                    return true;
                }
            }
            else //Try to detect game in folder.
            {
                //"CONQUER.MIX" and "NUKE.SHP" are two often needed files for Tiberian Dawn maps.
                FileMixArchiveWw fileMix = FileMixConquer.find();
                if (fileMix != null && fileMix.hasFile("NUKE.SHP"))
                {
                    Config = ConfigTD.create(); //Use default config file.
                    return true;
                }
            }
            return false; //Init as Tiberian Dawn failed. 
        }

        public static void run()
        {
#if DEBUG
            //TestsTD.print();
            //TestsTD.run();


            //FolderContainer fc = new FolderContainer(Program.GamePath);
            //TheaterTD theater = TheaterTD.getTheater("TEMPERATE");
            //fc.debugSaveFilesOfTypeInMix<FileMixArchiveWw>((mix) => mix.debugSaveMixFileEntries());
            //fc.debugSaveFilesOfTypeInMix<FileIni>((ini) => ini.debugSaveContent());
            //fc.debugSaveFilesOfTypeInMix<FileUnknown>((unk) => unk.debugSaveContent());
            //fc.debugSaveFilesOfTypeInMix<FileFntFontWw>((fnt) => fnt.debugSaveFontSheet(theater.FilePalette));
            //fc.debugSaveFilesOfTypeInMix<FileIcnTileSetTD>((icn) => icn.debugSaveIcnSheet(theater.Id, theater.FilePalette, 2));
            //fc.debugSaveFilesOfTypeInMix<FileShpSpriteSetTDRA>((shp) => shp.debugSaveShpSheet(theater.Id, theater.FilePalette, 2));

            //TheaterTD.debugSaveAll(true);


            MapTD.debugSaveRenderAll();
            //MapTD.renderAll();
#else
            MapTD.renderAll();
#endif
        }
    }
}
