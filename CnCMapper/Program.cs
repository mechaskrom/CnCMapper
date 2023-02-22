using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using CnCMapper.FileFormat;

namespace CnCMapper
{
    //This program outputs images of maps/missions from Command & Conquer (CnC) games. Currently it supports:
    //-Dune II: The Building of a Dynasty (D2) [1992]. Also known as Dune II: Battle for Arrakis.
    //-Command & Conquer: Tiberian Dawn (TD) [1995].
    //-Command & Conquer: Red Alert (RA) [1996].

    //TD and RA (worked on February 2020 to August 2021):--------------------------------------------------------------
    //All game info, tests and verification (checked in game) in this program is based on Tiberian Dawn (V.1.04a)
    //and Red Alert (V2.00ECSAM) in the "Command & Conquer The First Decade" collection unless noted otherwise.

    //All source code info and verification (checked in source) in this program is based on Tiberian Dawn and
    //Red Alert in the "CnC Remastered Collection" (master snapshot downloaded 20200603) unless noted otherwise.

    //I was about halfway done with this project when the CnC Remastered Collection source code was released.
    //A serendipitous event that made many things easier for me. Thank you very much Electronic Arts for this
    //nice surprise. This also mean that many of the older parts of this project may read a bit strange now.

    //Tiberian Dawn in "The First Decade" is the same as the windows 95 version (aka C&C Gold) I believe.
    //I did some testing with the DOS version (V.1.07) (aka C&C Original), but it's very unstable in DOSBox
    //for me which makes testing in it difficult. :(
    //The DOS version seems very similar, but graphics are scaled differently in the radar renderer at least.
    //So consider support for the DOS version to be sketchy in this program.

    //D2 (worked on November 2022 to February 2023):-------------------------------------------------------------------
    //Dune II was added a while after TD and RA which probably is noticeable in the code. TD and RA also have
    //more in common.

    //All game info, tests and verification (checked in game) in this program is based on Dune II: The Building
    //of a Dynasty (V1.00) DOS version unless noted otherwise.

    class Program
    {
        private enum GameId
        {
            Unknown,
            Dune2,
            TiberianDawn,
            RedAlert,
        }

        private static string mDebugBasePath = null; //Base folder for debug tests/saves.
        private static string mProgramPath = null;
        private static string mGamePath = null;
        private static string mRenderOutPath = null;
        private static string mDebugOutPath = null;

        public static string DebugBasePath
        {
            get { return getDebugBasePath(); }
        }

        public static string ProgramPath
        {
            get { return mProgramPath; }
        }

        public static string GamePath
        {
            get { return mGamePath; }
        }

        public static string RenderOutPath
        {
            get { return mRenderOutPath; }
        }

        public static string DebugOutPath
        {
            get { return mDebugOutPath; }
        }

        private static string getDebugBasePath()
        {
            if (mDebugBasePath == null)
            {
                //Return base path used when doing debug tests and saves i.e. running program within visual studio.
                //Use the solution folder and assume it is three levels up from the exe-file in the debug folder.
                //Projects\CnCMapper\CnCMapper\bin\Debug
                DirectoryInfo di = Directory.GetParent(getExePath()); //Get exe folder.
                mDebugBasePath = di.Parent.Parent.Parent.FullName.GetFullPathWithEndingSlashes(); //Get solution folder.
            }
            return mDebugBasePath;
        }

        private static string getExePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory.GetFullPathWithEndingSlashes();
        }

        private static void Main(string[] args)
        {
            GameId game;
            try
            {
                game = init(args);
            }
#if DEBUG
            finally
            {
            }
#else
            catch (Exception ex)
            {
                message("Failed to initialize program because: " + ex.Message);
                game = GameId.Unknown;
            }
#endif

            if (game != GameId.Unknown)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (game == GameId.Dune2)
                {
                    Game.CnC.D2.GameD2.run();
                }
                else if (game == GameId.TiberianDawn)
                {
                    Game.CnC.TDRA.TD.GameTD.run();
                }
                else if (game == GameId.RedAlert)
                {
                    Game.CnC.TDRA.RA.GameRA.run();
                }
                sw.Stop();
                message("Rendering time = " + sw.Elapsed);
            }

            Bench.saveResults(ProgramPath + "log.txt");
        }

        private static GameId init(string[] args)
        {
            GameId mDebugGame; //Force which game to init. Unknown==Detect game from command line arguments instead.
#if DEBUG
            //mDebugGame = GameId.Unknown;
            mDebugGame = GameId.Dune2;
            //mDebugGame = GameId.TiberianDawn;
            //mDebugGame = GameId.RedAlert;

            mProgramPath = DebugBasePath;
#else
            mDebugGame = GameId.Unknown;

            mProgramPath = getExePath();
#endif

            //Print default config files if they are missing.
            Game.CnC.D2.ConfigD2.printDefault(ProgramPath);
            Game.CnC.TDRA.TD.ConfigTD.printDefault(ProgramPath);
            Game.CnC.TDRA.RA.ConfigRA.printDefault(ProgramPath);

            string configPath = null;
            //Must set program and game paths first before continuing.
            if (mDebugGame == GameId.Dune2)
            {
                mGamePath = DebugBasePath + @"files\dos\dune-ii-the-building-of-a-dynasty\"; //US 1.00
                //mGamePath = DebugBasePath + @"files\dos\dune-ii-the-battle-for-arrakis\"; //EU 1.07

                configPath = ProgramPath + "DuneII.ini";
            }
            else if (mDebugGame == GameId.TiberianDawn)
            {
                mGamePath = DebugBasePath + @"files\Command & Conquer(tm)\";

                configPath = ProgramPath + "TiberianDawn.ini";
                //configPath = ProgramPath + "TiberianDawnLow.ini"; //Low amount of non-default settings.
                //configPath = ProgramPath + "TiberianDawnHigh.ini"; //High amount.
            }
            else if (mDebugGame == GameId.RedAlert)
            {
                mGamePath = DebugBasePath + @"files\Command & Conquer Red Alert(tm)\";

                configPath = ProgramPath + "RedAlert.ini";
                //configPath = ProgramPath + "RedAlertLow.ini";
                //configPath = ProgramPath + "RedAlertHigh.ini";
            }
            else
            {
                //Check number of command line arguments.
                if (args.Length < 1 || args.Length > 2)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Number of command line arguments '{0}' should be 1 or 2!", args.Length);
                    sb.AppendLine("-First argument: Path to game folder.");
                    sb.AppendLine("-Second argument (optional): Path to config file. Try to detect game and use its default config file if omitted.");
                    throw new ArgumentException(sb.ToString());
                }
                mGamePath = args[0].GetFullPathWithEndingSlashes();
                if (args.Length == 2) //Path to a config file is provided?
                {
                    configPath = Path.GetFullPath(args[1]);
                }
            }
            System.Diagnostics.Debug.Assert(ProgramPath != null && GamePath != null, "Program and game paths must be set!");

            //Check game folder path.
            if (!Directory.Exists(mGamePath))
            {
                throw new ArgumentException("Specified game folder path '" + mGamePath + "' doesn't exist!");
            }

            //Check config file path.
            FileIni fileConfig = null;
            if (configPath != null) //Path to a config file was provided?
            {
                if (!File.Exists(configPath))
                {
                    throw new ArgumentException("Specified config file path '" + configPath + "' doesn't exist!");
                }
                fileConfig = new FileIni(configPath);
            }

            return initDetectGame(fileConfig);
        }

        private static GameId initDetectGame(FileIni fileConfig)
        {
            //Try to detect game in folder or config file (game init succeeded).
            //This will also set the MIX-file database if the game needs it.
            GameId game;
            if (Game.CnC.D2.GameD2.init(fileConfig)) //Dune II: The Building of a Dynasty / Battle for Arrakis (D2)?
            {
                message("Game in config file or folder detected as Dune II: The Building of a Dynasty / Battle for Arrakis.");
                game = GameId.Dune2;
                mRenderOutPath = ProgramPath + "maps\\Dune 2\\";
                mDebugOutPath = DebugBasePath + "out\\D2\\";
                //mDebugOutPath = DebugBasePath + "out\\D2_EU\\";
            }
            else if (Game.CnC.TDRA.TD.GameTD.init(fileConfig)) //Command & Conquer: Tiberian Dawn (TD)?
            {
                message("Game in config file or folder detected as Command & Conquer: Tiberian Dawn.");
                game = GameId.TiberianDawn;
                mRenderOutPath = ProgramPath + "maps\\Tiberian Dawn\\";
                mDebugOutPath = DebugBasePath + "out\\TD\\";
            }
            else if (Game.CnC.TDRA.RA.GameRA.init(fileConfig)) //Command & Conquer: Red Alert (RA)?
            {
                message("Game in config file or folder detected as Command & Conquer: Red Alert.");
                game = GameId.RedAlert;
                mRenderOutPath = ProgramPath + "maps\\Red Alert\\";
                mDebugOutPath = DebugBasePath + "out\\RA\\";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                if (fileConfig == null)
                {
                    sb.AppendLine("Couldn't detect game in folder '{0}'.", mGamePath);
                }
                else
                {
                    sb.AppendLine("Couldn't detect game in folder '{0}' or config file '{1}'.", mGamePath, fileConfig.FullName);
                }
                sb.AppendLine();
                Game.CnC.D2.GameD2.addNeededFilesInfo(sb);
                sb.AppendLine();
                Game.CnC.TDRA.TD.GameTD.addNeededFilesInfo(sb);
                sb.AppendLine();
                Game.CnC.TDRA.RA.GameRA.addNeededFilesInfo(sb);
                throw new ArgumentException(sb.ToString());
            }
            return game;
        }

        public static void message(string message)
        {
            Console.WriteLine(message);
            Bench.add(message);
        }

        public static void warn(string message)
        {
            message = "Warning! " + message;
            Console.WriteLine(message);
            Bench.add(message);
        }
    }
}
