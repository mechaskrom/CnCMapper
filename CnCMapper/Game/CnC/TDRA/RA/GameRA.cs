using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //Command & Conquer: Red Alert (RA).
    static class GameRA
    {
        //Cache some common MIX-files.
        public static readonly FileMixSearch FileMixMain = new FileMixSearch("MAIN.MIX");
        public static readonly FileMixSearch FileMixMainGeneral = new FileMixSearch("GENERAL.MIX", FileMixMain);
        public static readonly FileMixSearch FileMixInterior = new FileMixSearch("INTERIOR.MIX", FileMixMain);
        public static readonly FileMixSearch FileMixSnow = new FileMixSearch("SNOW.MIX", FileMixMain);
        public static readonly FileMixSearch FileMixTemperat = new FileMixSearch("TEMPERAT.MIX", FileMixMain);
        //public static readonly FileMixSearch FileMixTemperat = new FileMixSearch("TEMPERAT.MIX", TestsRA.PrintPath); //Testing with modified theater.
        public static readonly FileMixSearch FileMixMainConquer = new FileMixSearch("CONQUER.MIX", FileMixMain);
        //public static readonly FileMixSearch FileMixMainConquer = new FileMixSearch("CONQUER.MIX", TestsRA.PrintPath); //Testing with modified sprites.
        public static readonly FileMixSearch FileMixRedAlert = new FileMixSearch("REDALERT.MIX");
        public static readonly FileMixSearch FileMixRedAlertHiRes = new FileMixSearch("HIRES.MIX", FileMixRedAlert);
        //public static readonly FileMixSearch FileMixRedAlertHiRes = new FileMixSearch("HIRES.MIX", TestsRA.PrintPath); //Testing with modified sprites.
        public static readonly FileMixSearch FileMixRedAlertLoRes = new FileMixSearch("LORES.MIX", FileMixRedAlert);
        public static readonly FileMixSearch FileMixRedAlertLocal = new FileMixSearch("LOCAL.MIX", FileMixRedAlert);
        //Added with Counterstrike & Aftermath expansions?
        public static readonly FileMixSearch FileMixExpand = new FileMixSearch("EXPAND.MIX");
        public static readonly FileMixSearch FileMixExpand2 = new FileMixSearch("EXPAND2.MIX");
        public static readonly FileMixSearch FileMixHiRes1 = new FileMixSearch("HIRES1.MIX");
        public static readonly FileMixSearch FileMixLoRes1 = new FileMixSearch("LORES1.MIX");

        public static ConfigRA Config = null;

        public static void addNeededFilesInfo(StringBuilder sb)
        {
            sb.AppendLine("A Command & Conquer Red Alert (RA) folder:");
            sb.AppendLine("-needs 'CONQUER.MIX' (folder or inside 'MAIN.MIX')");
            sb.AppendLine("-needs 'HIRES.MIX' or 'LORES.MIX' (folder or inside 'REDALERT.MIX')");
            sb.AppendLine("-needs 'LOCAL.MIX' (folder or inside 'REDALERT.MIX')");
            sb.AppendLine("-optionally 'INTERIOR.MIX' (folder or inside 'MAIN.MIX')");
            sb.AppendLine("-optionally 'SNOW.MIX' (folder or inside 'MAIN.MIX')");
            sb.AppendLine("-optionally 'TEMPERAT.MIX' (folder or inside 'MAIN.MIX')");
            sb.AppendLine("-optionally 'EXPAND.MIX'");
            sb.AppendLine("-optionally 'EXPAND2.MIX'");
            sb.AppendLine("-optionally 'HIRES1.MIX' or 'LORES1.MIX'");
            sb.AppendLine("-maps (.INI) searched for in 'GENERAL.MIX' (folder or inside 'MAIN.MIX') and the folder");
            sb.AppendLine("-multiplayer maps should start with 'SCM' and end with '.INI|.MPR' e.g. 'SCM01EA.INI'");
        }

        public static bool init(FileIni fileConfig)
        {
            System.Diagnostics.Debug.Assert(Program.ProgramPath != null && Program.GamePath != null,
                "Program and game paths must be set before init!");

            FileMixArchiveWw.FileDatabase = new FileDatabaseRA(); //Set MIX-file database first.

            if (fileConfig != null) //Detect game in config file?
            {
                if (ConfigRA.isFileConfig(fileConfig))
                {
                    Config = ConfigRA.create(fileConfig);
                    return true;
                }
            }
            else //Try to detect game in folder.
            {
                //"CONQUER.MIX" and "POWR.SHP" are two often needed files for Red Alert maps.
                FileMixArchiveWw fileMix = FileMixMainConquer.find();
                if (fileMix != null && fileMix.hasFile("POWR.SHP"))
                {
                    Config = ConfigRA.create(); //Use default config file.
                    return true;
                }
            }
            return false; //Init as Red Alert failed. 
        }

        public static void run()
        {
#if DEBUG
            //TestsRA.print();
            //TestsRA.run();


            //FolderContainer fc = new FolderContainer(Program.GamePath);
            //TheaterRA theater = TheaterRA.getTheater("TEMPERATE");
            //fc.debugSaveFilesOfTypeInMix<FileMixArchiveWw>((mix) => mix.debugSaveMixFileEntries());
            //fc.debugSaveFilesOfTypeInMix<FileIni>((ini) => ini.debugSaveContent());
            //fc.debugSaveFilesOfTypeInMix<FileUnknown>((unk) => unk.debugSaveContent());
            //fc.debugSaveFilesOfTypeInMix<FileFntFontWw>((fnt) => fnt.debugSaveFontSheet(theater.FilePalette));
            //fc.debugSaveFilesOfTypeInMix<FileIcnTileSetRA>((icn) => icn.debugSaveIcnSheet(theater.Id, theater.FilePalette, 2));
            //fc.debugSaveFilesOfTypeInMix<FileShpSpriteSetTDRA>((shp) => shp.debugSaveShpSheet(theater.Id, theater.FilePalette, 2));

            //TheaterRA.debugSaveAll(true);


            MapRA.debugSaveRenderAll();
            //MapRA.renderAll();
#else
            MapRA.renderAll();
#endif
        }
    }
}
