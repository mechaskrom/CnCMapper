using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    //Dune II: The Building of a Dynasty / Battle for Arrakis (D2).
    class GameD2
    {
        //Cache some common PAK-files.
        public static readonly FilePakSearch FilePakDune = new FilePakSearch("DUNE.PAK");
        //public static readonly FilePakSearch FilePakDune = new FilePakSearch("DUNE.PAK", TestsD2.PrintPath); //Testing with modified theater.
        public static readonly FilePakSearch FilePakScenario = new FilePakSearch("SCENARIO.PAK");

        public static ConfigD2 Config = null;

        public static void addNeededFilesInfo(StringBuilder sb)
        {
            sb.AppendLine("A Dune II: The Building of a Dynasty / Battle for Arrakis (D2) folder:");
            sb.AppendLine("-needs 'DUNE.PAK'");
            sb.AppendLine("-maps (.INI) searched for in 'SCENARIO.PAK' and the folder");
        }

        public static bool init(FileIni fileConfig)
        {
            System.Diagnostics.Debug.Assert(Program.ProgramPath != null && Program.GamePath != null,
                "Program and game paths must be set before init!");

            if (fileConfig != null) //Detect game in config file?
            {
                if (ConfigD2.isFileConfig(fileConfig))
                {
                    Config = ConfigD2.create(fileConfig);
                    return true;
                }
            }
            else //Try to detect game in folder.
            {
                //"DUNE.PAK" and "UNITS.SHP" are two often needed files for Dune 2 maps.
                FilePakArchiveWw filePak = FilePakDune.find();
                if (filePak != null && filePak.hasFile("UNITS.SHP"))
                {
                    Config = ConfigD2.create(); //Use default config file.
                    return true;
                }
            }
            return false; //Init as Dune 2 failed. 
        }

        public static void run()
        {
#if DEBUG
            //TestsD2.print();
            //TestsD2.run();


            //FolderContainer fc = new FolderContainer(Program.GamePath);
            //TheaterD2 theater = TheaterD2.Desert;
            //fc.debugSaveFilesOfTypeInPak<FilePakArchiveWw>((pak) => pak.debugSavePakFileEntries());
            //fc.debugSaveFilesOfTypeInPak<FileIni>((ini) => ini.debugSaveContent());
            //fc.debugSaveFilesOfTypeInPak<FileFntFontWw>((fnt) => fnt.debugSaveFontSheet(theater.FilePalette));
            //fc.debugSaveFilesOfTypeInPak<FileCpsImageWw>((cps) => cps.debugSaveImage(cps.Palette != null ? cps.Palette : theater.FilePalette));
            //fc.debugSaveFilesOfTypeInPak<FileIcnTilesD2>((icn) => icn.debugSaveIcnSheet(theater.Name, theater.FilePalette, 2, true));
            //fc.debugSaveFilesOfTypeInPak<FileShpSpriteSetD2>((shp) => shp.debugSaveShpSheet(theater.Name, theater.FilePalette, 2, true));

            //TheaterD2.debugSaveAll(true);


            MapD2.debugSaveRenderAll();
            //MapD2.renderAll();
#else
            MapD2.renderAll();
#endif
        }
    }
}
