using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    //Stores data about all known normal singleplayer missions in Tiberian Dawn.
    class MissionDataTD : MissionDataTDRA
    {
        private const string GameName = "Command & Conquer: Tiberian Dawn";
        private const string MissGdi = "GDI";
        private const string MissNod = "Nod";
        private const string MissCovertGdi = "Covert Operations - GDI";
        private const string MissCovertNod = "Covert Operations - Nod";
        private const string MissCovertDino = "Jurassic";

        //City = Point of conflict location from the game's selected mission data.
        //Country = Country from the game's selected mission data.

        //Name is primary from in game or INI-file (covert operation missions), but most missions in TD are nameless.
        //If name is missing then get it from https://cnc.fandom.com/wiki/Category:Tiberian_Dawn_missions which
        //should be the same as in the "Command & Conquer: Remastered Collection". Can't get more official than that.

        //Some names are altered slightly to better match the game and to be more coherent.
        //E.g. Dr. Moebius is how TD spells his name, but his official name was later changed to Dr. Mobius?

        private MissionDataTD(string mission, int number, string name, string city, string country)
            : base(mission, number, name, city, country)
        {
        }

        public static string getHeader(FileIni fileIni)
        {
            return getHeader(GameName, fileIni, getData(fileIni.Id), true);
        }

        public static string getFileName(FileIni fileIni)
        {
            return getFileName(fileIni, getData(fileIni.Id), true);
        }

        private static MissionDataTD getData(string mapId)
        {
            switch (mapId)
            {
                case "SCG01EA": return new MissionDataTD(MissGdi, 1, "X16-Y42", "Parnu", "Estonia");
                case "SCG02EA": return new MissionDataTD(MissGdi, 2, "Knock Out That Refinery", "Parnu", "Estonia");
                case "SCG03EA": return new MissionDataTD(MissGdi, 3, "Air Supremacy", "Jelgava", "Latvia");
                case "SCG04EA": return new MissionDataTD(MissGdi, 4, "Stolen Property", "Bobyrusk", "Belarus");
                case "SCG04WA": return new MissionDataTD(MissGdi, 4, "Stolen Property", "Gdansk", "Poland"); //Rerouted to Bobyrusk, Belarus?
                case "SCG04WB": return new MissionDataTD(MissGdi, 4, "Reinforce Byelistok", "Byelistok", "Poland");
                case "SCG05EA": return new MissionDataTD(MissGdi, 5, "Restoring Power", "Ivano-Frankovsk", "Ukraine");
                case "SCG05EB": return new MissionDataTD(MissGdi, 5, "Restoring Power", "Ivano-Frankovsk", "Ukraine"); //Inaccessible?
                case "SCG05WA": return new MissionDataTD(MissGdi, 5, "Restoring Power", "Hanover", "Germany");
                case "SCG05WB": return new MissionDataTD(MissGdi, 5, "Restoring Power", "Dresden", "Germany");
                case "SCG06EA": return new MissionDataTD(MissGdi, 6, "Havoc", "Ostrava", "Czech Republic");
                case "SCG07EA": return new MissionDataTD(MissGdi, 7, "Destroy The Airstrip", "Ostrava", "Czech Republic");
                case "SCG08EA": return new MissionDataTD(MissGdi, 8, "U.N. Sanctions", "Salzburg", "Austria"); //Game says Jelgava, but that's in Latvia. *1
                case "SCG08EB": return new MissionDataTD(MissGdi, 8, "Doctor Moebius", "Bratislava", "Slovakia");
                case "SCG09EA": return new MissionDataTD(MissGdi, 9, "Clearing A Path", "Budapest", "Hungary");
                case "SCG10EA": return new MissionDataTD(MissGdi, 10, "Orcastration", "Trieste", "Slovenia"); //Trieste is in Italy, but near Slovenia.
                case "SCG10EB": return new MissionDataTD(MissGdi, 10, "Orcastration", "Arad", "Romania");
                case "SCG11EA": return new MissionDataTD(MissGdi, 11, "Code Name Delphi", "Corinth", "Greece");
                case "SCG12EA": return new MissionDataTD(MissGdi, 12, "Saving Doctor Moebius", "Shkoder", "Albania");
                case "SCG12EB": return new MissionDataTD(MissGdi, 12, "Saving Doctor Moebius", "Sofia", "Bulgaria");
                case "SCG13EA": return new MissionDataTD(MissGdi, 13, "Ion Cannon Strike", "Nis", "Yugoslavia");
                case "SCG13EB": return new MissionDataTD(MissGdi, 13, "Ion Cannon Strike", "Nis", "Yugoslavia"); //Inaccessible?
                case "SCG14EA": return new MissionDataTD(MissGdi, 14, "Fish In A Barrel", "Belgrade", "Yugoslavia");
                case "SCG15EA": return new MissionDataTD(MissGdi, 15, "Temple Strike", "Sarajevo", "Bosnia/Herzogovina");
                case "SCG15EB": return new MissionDataTD(MissGdi, 15, "Temple Strike", "Sarajevo", "Bosnia/Herzogovina");
                case "SCG15EC": return new MissionDataTD(MissGdi, 15, "Temple Strike", "Sarajevo", "Bosnia/Herzogovina");
                //*1=Probably should be Salzburg from looking at order of names in 'CONQUER.ENG' and location on the selection screen.

                case "SCB01EA": return new MissionDataTD(MissNod, 1, "Silencing Nikoomba", "Tmassah", "Libya");
                case "SCB02EA": return new MissionDataTD(MissNod, 2, "Liberation Of Egypt", "Al-Alamyn", "Egypt");
                case "SCB02EB": return new MissionDataTD(MissNod, 2, "Liberation Of Egypt", "Al-Kharijah", "Egypt");
                case "SCB03EA": return new MissionDataTD(MissNod, 3, "Friends Of The Brotherhood", "Al-Ubayyid", "Sudan");
                case "SCB03EB": return new MissionDataTD(MissNod, 3, "Friends Of The Brotherhood", "Kafia-Kingi", "Sudan");
                case "SCB04EA": return new MissionDataTD(MissNod, 4, "Convoy Interception", "Oum Hadjer", "Chad");
                case "SCB04EB": return new MissionDataTD(MissNod, 4, "False Flag Operation", "Mao", "Chad");
                case "SCB05EA": return new MissionDataTD(MissNod, 5, "Grounded", "Tidjikdja", "Mauritania");
                case "SCB06EA": return new MissionDataTD(MissNod, 6, "Extract The Detonator", "Abidjan", "Ivory Coast");
                case "SCB06EB": return new MissionDataTD(MissNod, 6, "Extract The Detonator", "Porto-Novo", "Benin");
                case "SCB06EC": return new MissionDataTD(MissNod, 6, "Extract The Detonator", "Abuja", "Nigeria");
                case "SCB07EA": return new MissionDataTD(MissNod, 7, "Sick And Dying", "Koula-Moutou", "Gabon");
                case "SCB07EB": return new MissionDataTD(MissNod, 7, "Sick And Dying", "Bertoua", "Cameroon");
                case "SCB07EC": return new MissionDataTD(MissNod, 7, "Orca Heist", "Bangassou", "Central African Republic");
                case "SCB08EA": return new MissionDataTD(MissNod, 8, "New Construction Options", "Lodja", "Zaire");
                case "SCB08EB": return new MissionDataTD(MissNod, 8, "New Construction Options", "Kinshasa", "Zaire");
                case "SCB09EA": return new MissionDataTD(MissNod, 9, "No Mercy", "Luxor", "Egypt");
                case "SCB10EA": return new MissionDataTD(MissNod, 10, "Doctor Wong", "Caiundo", "Angola");
                case "SCB10EB": return new MissionDataTD(MissNod, 10, "Belly Of The Beast", "Mzuzu", "Tanzania");
                case "SCB11EA": return new MissionDataTD(MissNod, 11, "Ezekiel's Wheel", "Keetmanshoop", "Namibia");
                case "SCB11EB": return new MissionDataTD(MissNod, 11, "Ezekiel's Wheel", "Xai-Xai", "Mozambique");
                case "SCB12EA": return new MissionDataTD(MissNod, 12, "Steal The Codes", "Ghanzi", "Botswana");
                case "SCB13EA": return new MissionDataTD(MissNod, 13, "Cradle Of My Temple", "Cape Town", "South Africa");
                case "SCB13EB": return new MissionDataTD(MissNod, 13, "Cradle Of My Temple", "Cape Town", "South Africa");
                case "SCB13EC": return new MissionDataTD(MissNod, 13, "Cradle Of My Temple", "Cape Town", "South Africa");

                case "SCG22EA": return new MissionDataTD(MissCovertGdi, 1, "Blackout", null, null);
                case "SCG23EA": return new MissionDataTD(MissCovertGdi, 2, "Hell's Fury", null, null);
                case "SCG36EA": return new MissionDataTD(MissCovertGdi, 3, "Infiltrated!", null, null);
                case "SCG38EA": return new MissionDataTD(MissCovertGdi, 4, "Elemental Imperative", null, null);
                case "SCG40EA": return new MissionDataTD(MissCovertGdi, 5, "Ground Zero", null, null);
                case "SCG41EA": return new MissionDataTD(MissCovertGdi, 6, "Twist Of Fate", null, null);
                case "SCG50EA": return new MissionDataTD(MissCovertGdi, 7, "Blindsided", null, null);

                case "SCB20EA": return new MissionDataTD(MissCovertNod, 1, "Bad Neighborhood", null, null);
                case "SCB21EA": return new MissionDataTD(MissCovertNod, 2, "Deceit", null, null);
                case "SCB30EA": return new MissionDataTD(MissCovertNod, 3, "Eviction Notice", null, null);
                case "SCB31EA": return new MissionDataTD(MissCovertNod, 4, "The Tiberium Strain", null, null);
                case "SCB32EA": return new MissionDataTD(MissCovertNod, 5, "Cloak And Dagger", null, null);
                case "SCB33EA": return new MissionDataTD(MissCovertNod, 6, "Hostile Takeover", null, null);
                case "SCB35EA": return new MissionDataTD(MissCovertNod, 7, "Under Siege: C&C", null, null);
                case "SCB37EA": return new MissionDataTD(MissCovertNod, 8, "Nod Death Squad", null, null);

                case "SCJ01EA": return new MissionDataTD(MissCovertDino, 1, "Strange Behavior", null, null);
                case "SCJ02EA": return new MissionDataTD(MissCovertDino, 2, "Strange Behavior", null, null);
                case "SCJ03EA": return new MissionDataTD(MissCovertDino, 3, "Strange Behavior", null, null);
                case "SCJ04EA": return new MissionDataTD(MissCovertDino, 4, "Strange Behavior", null, null);
                case "SCJ05EA": return new MissionDataTD(MissCovertDino, 5, "Strange Behavior", null, null);

                default: return null;
            }
        }

        //Tiberian Dawn Mission Tree.
        //Mission select maps can be found in "GENERAL.MIX" ("*.WSA"-files).
        //W=west, E=east, N=north, S=south mission target arrow selected.
        //GDI campaign ("SCG##??.INI"-files):----------------------------------------------------------------------------------
        //01EA -> 02EA -> 03EA -> W:04WA -> N:05WA -> W:06EA -> 07EA -> W:08EA -> W:09EA -> W:10EA -> 11EA -> W:12EA -> W:13EA -> 14EA -> W:15EA
        //                        S:04WB    S:05WB    E:06EA            E:08EB    E:09EA    E:10EB            E:12EB    E:13EA            S:15EB
        //                                                                                                                                E:15EC
        //                        E:04EA -> W:05EA
        //                                  E:05EA
        //-Mission level 4 to 5 is the only point in the campaign that splits into two branches.
        //  The west and south targets (SCG04WA and SCG04WB) will continue in Germany (SCG05WA and SCG05WB),
        //  while the east target (SCG04EA) will continue in Ukraine (SCG05EA).
        //  Both branches will join at mission level 6 though.
        //-Both W&E path goes to 06EA. Only SCG06EA exists.
        //-Both W&E path goes to 09EA. Only SCG09EA exists.
        //-Both W&E path goes to 05EA. Both SCG05EA (pretty much same as SCG05WA) and SCG05EB exists.
        //-Both W&E path goes to 13EA. Both SCG13EA and SCG13EB exists (same map though).
        //-SCG05EB and SCG13EB (same as SCG13EA though) are the only missions that can't(?) be selected even though they exist in "GENERAL.MIX".

        //Nod campaign ("SCB##??.INI"-files):----------------------------------------------------------------------------------
        //01EA -> N:02EA -> E:03EA -> N:04EA -> 05EA -> W:06EA -> S:07EA -> E:08EA -> W:09EA -> W:10EA -> W:11EA -> W:12EA -> W:13EA
        //        S:02EB    W:03EB    S:04EB            S:06EB    N:07EB    W:08EB    S:09EA    E:10EB    E:11EB    E:12EA    N:13EB
        //                                              E:06EC    E:07EC                                                      E:13EC
        //-Both W&S path goes to 09EA. Only SCB09EA exists.
        //-Both W&E path goes to 12EA. Only SCB12EA exists.
        //
    }
}
