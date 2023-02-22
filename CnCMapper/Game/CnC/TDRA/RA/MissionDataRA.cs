using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    //Stores data about all known normal singleplayer missions in Red Alert.
    class MissionDataRA : MissionDataTDRA
    {
        private const string GameName = "Command & Conquer: Red Alert";
        private const string MissAllies = "Allies";
        private const string MissSoviet = "Soviet";
        private const string MissCountAllies = "Counterstrike - Allies";
        private const string MissCountSoviet = "Counterstrike - Soviet";
        private const string MissCountAnt = "Counterstrike - Ant Secret Missions";
        private const string MissAfterAllies = "Aftermath - Allies";
        private const string MissAfterSoviet = "Aftermath - Soviet";

        //City = null.
        //Country = Estimation of target box location in mission select screen.
        //City isn't present in Red Alert and difficult to determine for all maps so let's omit it.
        //Country isn't present in Red Alert and hard to pinpoint sometimes, but should be close.

        //Name is primary from in game or INI-file (sometimes missing for normal missions).
        //If name is missing then get it from https://cnc.fandom.com/wiki/Category:Red_Alert_1_missions which
        //should be the same as in the "Command & Conquer: Remastered Collection". Can't get more official than that.

        //Some names are altered slightly to better match the game and to be more coherent.
        //E.g. names in CnC Remastered often have a parenthesized location, but the original game never had that.

        private MissionDataRA(string mission, int number, string name, string country)
            : base(mission, number, name, null, country)
        {
        }

        public static bool isLaterCampaignMission(FileIni current, FileIni compare) //True if "current" comes after "compare".
        {
            MissionDataRA mdCurr = getData(current.Id);
            MissionDataRA mdComp = getData(compare.Id);
            return mdCurr != null && mdComp != null && mdCurr.mMission == mdComp.mMission &&
                isCampaignMission(mdCurr.mMission) && string.CompareOrdinal(current.Id, compare.Id) > 0;
        }

        private static bool isCampaignMission(string mission)
        {
            //Missions that are chained together and must be completed in an order i.e. not freely selectable.
            return mission == MissAllies || mission == MissSoviet || mission == MissCountAnt;
        }

        public static string getHeader(FileIni fileIni)
        {
            return getHeader(GameName, fileIni, getData(fileIni.Id), false);
        }

        public static string getFileName(FileIni fileIni)
        {
            return getFileName(fileIni, getData(fileIni.Id), false);
        }

        private static MissionDataRA getData(string mapId)
        {
            switch (mapId)
            {
                case "SCG01EA": return new MissionDataRA(MissAllies, 1, "In The Thick Of It", "Hungary");
                case "SCG02EA": return new MissionDataRA(MissAllies, 2, "Five To One", "Poland");
                case "SCG03EA": return new MissionDataRA(MissAllies, 3, "Dead End", "Poland");
                case "SCG03EB": return new MissionDataRA(MissAllies, 3, "Dead End", "Soviet Union"); //*1.
                case "SCG04EA": return new MissionDataRA(MissAllies, 4, "Ten To One", "Poland");
                case "SCG05EA": return new MissionDataRA(MissAllies, 5, "Tanya's Tale", "Soviet Union");
                case "SCG05EB": return new MissionDataRA(MissAllies, 5, "Tanya's Tale", "Soviet Union"); //*1.
                case "SCG05EC": return new MissionDataRA(MissAllies, 5, "Tanya's Tale", "Soviet Union"); //*1.
                case "SCG06EA": return new MissionDataRA(MissAllies, 6, "Iron Curtain Infiltration", "Greece"); //*1.
                case "SCG06EB": return new MissionDataRA(MissAllies, 6, "Iron Curtain Infiltration", "Greece"); //*1.
                case "SCG07EA": return new MissionDataRA(MissAllies, 7, "Sunken Treasure", "Denmark"); //Bornholm island?
                case "SCG08EA": return new MissionDataRA(MissAllies, 8, "Protect The Chronosphere", "Germany"); //*1,*2.
                case "SCG08EB": return new MissionDataRA(MissAllies, 8, "Protect The Chronosphere", "Austria"); //*1,*2.
                case "SCG09EA": return new MissionDataRA(MissAllies, 9, "Extract Kosygin", "Soviet Union"); //*1.
                case "SCG09EB": return new MissionDataRA(MissAllies, 9, "Extract Kosygin", "Soviet Union"); //*1.
                case "SCG10EA": return new MissionDataRA(MissAllies, 10, "Suspicion", "Soviet Union");
                case "SCG10EB": return new MissionDataRA(MissAllies, 10, "Evidence", "Soviet Union");
                case "SCG11EA": return new MissionDataRA(MissAllies, 11, "Naval Supremacy", "Soviet Union"); //*1.
                case "SCG11EB": return new MissionDataRA(MissAllies, 11, "Naval Supremacy", "Soviet Union"); //*1.
                case "SCG12EA": return new MissionDataRA(MissAllies, 12, "Takedown", "Soviet Union");
                case "SCG13EA": return new MissionDataRA(MissAllies, 13, "Focused Blast", "Soviet Union");
                case "SCG14EA": return new MissionDataRA(MissAllies, 14, "No Remorse", "Soviet Union"); //Moscow?
                //*1=No ingame name.
                //*2=Von Esling says in the briefing that it's outside Liech, Liechtenstein?

                case "SCU01EA": return new MissionDataRA(MissSoviet, 1, "Lesson In Blood", "Poland");
                case "SCU02EA": return new MissionDataRA(MissSoviet, 2, "Guard Duty", "Poland"); //*1.
                case "SCU02EB": return new MissionDataRA(MissSoviet, 2, "Guard Duty", "Czechoslovakia"); //*1.
                case "SCU03EA": return new MissionDataRA(MissSoviet, 3, "Covert Cleanup", "Sweden");
                case "SCU04EA": return new MissionDataRA(MissSoviet, 4, "Behind The Lines", "Germany");
                case "SCU04EB": return new MissionDataRA(MissSoviet, 4, "Behind The Lines", "Germany"); //*1.
                case "SCU05EA": return new MissionDataRA(MissSoviet, 5, "Distant Thunder", "Greece");
                case "SCU06EA": return new MissionDataRA(MissSoviet, 6, "Bridge Over The River Grotzny", "Yugoslavia");
                case "SCU06EB": return new MissionDataRA(MissSoviet, 6, "Bridge Over The River Vizchgoi", "Yugoslavia"); //*2
                case "SCU07EA": return new MissionDataRA(MissSoviet, 7, "Core Of The Matter", "Soviet Union");
                case "SCU08EA": return new MissionDataRA(MissSoviet, 8, "Elba Island", "Italy"); //*1.
                case "SCU08EB": return new MissionDataRA(MissSoviet, 8, "Elba Island", "Italy"); //*1.
                case "SCU09EA": return new MissionDataRA(MissSoviet, 9, "Liability Elimination", "France");
                case "SCU10EA": return new MissionDataRA(MissSoviet, 10, "Overseer", "France");
                case "SCU11EA": return new MissionDataRA(MissSoviet, 11, "Sunk Costs", "France"); //*1.
                case "SCU11EB": return new MissionDataRA(MissSoviet, 11, "Sunk Costs", "France"); //*1.
                case "SCU12EA": return new MissionDataRA(MissSoviet, 12, "Capture The Tech Centers", "Switzerland"); //*1.
                case "SCU13EA": return new MissionDataRA(MissSoviet, 13, "Capture The Chronosphere", "Portugal"); //*1.
                case "SCU13EB": return new MissionDataRA(MissSoviet, 13, "Capture The Chronosphere", "Spain"); //*1.
                case "SCU14EA": return new MissionDataRA(MissSoviet, 14, "Soviet Supremacy", "Great Britain"); //London?
                //*1=No ingame name.
                //*2=VizchGoi in the INI-file. A capitalization error? Also doesn't adhere to how I capitalize other map names.

                case "SCG20EA": return new MissionDataRA(MissCountAllies, 1, "Sarin Gas 1: Crackdown", null);
                case "SCG21EA": return new MissionDataRA(MissCountAllies, 2, "Sarin Gas 2: Down Under", null);
                case "SCG22EA": return new MissionDataRA(MissCountAllies, 3, "Sarin Gas 3: Controlled Burn", null);
                case "SCG23EA": return new MissionDataRA(MissCountAllies, 4, "Fall Of Greece 1: Personal War", null);
                case "SCG24EA": return new MissionDataRA(MissCountAllies, 5, "Fall Of Greece 2: Evacuation", null);
                case "SCG26EA": return new MissionDataRA(MissCountAllies, 6, "Siberian Conflict 1: Fresh Tracks", null);
                case "SCG27EA": return new MissionDataRA(MissCountAllies, 7, "Siberian Conflict 2: Trapped", null);
                case "SCG28EA": return new MissionDataRA(MissCountAllies, 8, "Siberian Conflict 3: Wasteland", null);

                case "SCU31EA": return new MissionDataRA(MissCountSoviet, 1, "Proving Grounds", null);
                case "SCU32EA": return new MissionDataRA(MissCountSoviet, 2, "Besieged", null);
                case "SCU33EA": return new MissionDataRA(MissCountSoviet, 3, "Mousetrap", null);
                case "SCU34EA": return new MissionDataRA(MissCountSoviet, 4, "Legacy Of Tesla", null);
                case "SCU35EA": return new MissionDataRA(MissCountSoviet, 5, "Soviet Soldier Volkov & Chitzkoi", null);
                case "SCU36EA": return new MissionDataRA(MissCountSoviet, 6, "Top O' The World", null);
                case "SCU37EA": return new MissionDataRA(MissCountSoviet, 7, "Paradox Equation", null);
                case "SCU38EA": return new MissionDataRA(MissCountSoviet, 8, "Nuclear Escalation", null);

                case "SCA01EA": return new MissionDataRA(MissCountAnt, 1, "It Came From RA! 1: Discovery", null);
                case "SCA02EA": return new MissionDataRA(MissCountAnt, 2, "It Came From RA! 2: Rescue!", null);
                case "SCA03EA": return new MissionDataRA(MissCountAnt, 3, "It Came From RA! 3: Hunt!", null);
                case "SCA04EA": return new MissionDataRA(MissCountAnt, 4, "It Came From RA! 4: Exterminate!", null);

                case "SCG40EA": return new MissionDataRA(MissAfterAllies, 1, "Caught In The Act", null);
                case "SCG41EA": return new MissionDataRA(MissAfterAllies, 2, "In The Nick Of Time", null);
                case "SCG42EA": return new MissionDataRA(MissAfterAllies, 3, "Production Disruption", null);
                case "SCG43EA": return new MissionDataRA(MissAfterAllies, 4, "Harbor Reclamation", null);
                case "SCG44EA": return new MissionDataRA(MissAfterAllies, 5, "Time Flies", null);
                case "SCG45EA": return new MissionDataRA(MissAfterAllies, 6, "Monster Tank Madness", null);
                case "SCG46EA": return new MissionDataRA(MissAfterAllies, 7, "PAWN", null);
                case "SCG47EA": return new MissionDataRA(MissAfterAllies, 8, "Negotiations", null);
                case "SCG48EA": return new MissionDataRA(MissAfterAllies, 9, "Absolute M.A.D.ness", null);

                case "SCU40EA": return new MissionDataRA(MissAfterSoviet, 1, "Shock Therapy", null);
                case "SCU41EA": return new MissionDataRA(MissAfterSoviet, 2, "Test Drive", null);
                case "SCU42EA": return new MissionDataRA(MissAfterSoviet, 3, "Let's Make A Steal", null);
                case "SCU43EA": return new MissionDataRA(MissAfterSoviet, 4, "Testing Grounds", null);
                case "SCU44EA": return new MissionDataRA(MissAfterSoviet, 5, "Situation Critical", null);
                case "SCU45EA": return new MissionDataRA(MissAfterSoviet, 6, "Don't Drink The Water", null);
                case "SCU46EA": return new MissionDataRA(MissAfterSoviet, 7, "Brothers In Arms", null);
                case "SCU47EA": return new MissionDataRA(MissAfterSoviet, 8, "Deus Ex Machina", null);
                case "SCU48EA": return new MissionDataRA(MissAfterSoviet, 9, "Grunyev Revolution", null);

                default: return null;
            }
        }

        //Red Alert Mission Tree.
        //Mission select maps can be found in "GENERAL.MIX" in "MAIN.MIX" ("MS**.WSA"-files).
        //W=west, E=east, N=north, S=south mission target box selected.
        //Allied campaign ("SCG##E?.INI"-files):-------------------------------------------------------------------------------
        //01A -> 02A -> N:03A -> 04A -> W:05A -> N:06A -> 07A -> W:08A -> N:09A -> 10A -> 10B -> N:11A -> 12A -> 13A -> 14A
        //              S:03B           E:05B    S:06B           E:08B    S:09B                  S:11B
        //                              S:05C
        //-SCG10EB follows automatically after SCG10EA and there is no mission select screen between them.
        //
        //Soviet campaign ("SCU##E?.INI"-files):-------------------------------------------------------------------------------
        //01A -> N:02A -> 03A -> N:04A -> 05A -> W:06A -> 07A -> W:08A -> 09A -> 10A -> N:11A -> 12A -> N:13A -> 14A
        //       S:02B           S:04B           E:06B           E:08B                  S:11B           S:13B
        //
    }
}
