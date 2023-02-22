using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class SpriteOverlayRA : SpriteTDRA
    {
        private enum OverlayType
        {
            None = 0, //Init value. Shouldn't be used otherwise.
            WallBarb, //Barbwire fence (Tiberian Dawn). Walls must be listed in this order.
            WallBrik, //Concrete wall.
            WallCycl, //Chain link fence.
            WallFenc, //Barbwire fence (Soviet).
            WallSbag, //Sandbag wall.
            WallWood, //Wooden fence.
            Gems,
            Gold,
            Farmland, //V12-V18 neutral farmland structures.
            FlagBase, //Flag placement sign (capture the flag).
            SteelCrate,
            WoodenCrate,
            WaterCrate,  //Water wooden crate.
            Undefined,
        }
        private OverlayType mType = OverlayType.None;

        private SpriteOverlayRA(TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(fileShp.Id, tilePos, fileShp)
        {
            mPriPlane = PriPlaneOverlay;
        }

        private static bool isWall(OverlayType type)
        {
            return type >= OverlayType.WallBarb && type <= OverlayType.WallWood;
        }

        public static void endAdd(List<SpriteOverlayRA> overlays, List<SpriteTDRA> sprites)
        {
            //Overlays in list must be in the order they were added from the INI-file.

            //In Tiberian Dawn overlays don't overlap, only first overlay at a tile number is added.
            //With the new overlay pack format in Red Alert overlays normally can't overlap though.
            TileTracker<SpriteOverlayRA> overlayTracker = new TileTracker<SpriteOverlayRA>(MapRA.SizeInTiles);
            foreach (SpriteOverlayRA overlay in overlays)
            {
                overlayTracker.addItem(overlay); //Add overlay if tile is empty.
            }

            //Adjust and add final overlays.
            sprites.AddDerivedRange(getFinalOverlays(overlayTracker));
        }

        public static void add(MapRA map, List<SpriteOverlayRA> overlays)
        {
            //https://www.modenc.renegadeprojects.com/NewINIFormat
            //"NewINIFormat" key in [Basic] section specifies format used (sections read), but it seems
            //like Red Alert can only handle "NewINIFormat=3". It crashes with other values.
            //"NewINIFormat=3" means the game will among other things read overlay from [OverlayPack]
            //instead of old [OVERLAY].

            //Decoded overlay pack data is 16KB (128*128*1) with overlay id data (UInt8), 0xFF=no overlay in tile.
            IniSection iniSection = map.FileIni.findSection("OverlayPack");
            if (iniSection != null)
            {
                byte[] overlayIdInTiles = Crypt.Pack.decode(iniSection.getPackDataAsBase64());
                for (int i = 0; i < overlayIdInTiles.Length; i++)
                {
                    string id = toOverlayId(overlayIdInTiles[i]);
                    if (id != null)
                    {
                        TilePos tilePos = MapRA.toTilePos(i);
                        FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(id);
                        SpriteOverlayRA spr = new SpriteOverlayRA(tilePos, fileShp);
                        addInner(spr, overlays);
                    }
                }
            }
        }

        private static void addInner(SpriteOverlayRA spr, List<SpriteOverlayRA> overlays)
        {
            switch (spr.Id)
            {
                case "BARB": //Barbwire fence (Tiberian Dawn).
                    addDefault(spr, OverlayType.WallBarb, overlays); break;
                case "BRIK": //Concrete wall.
                    addDefault(spr, OverlayType.WallBrik, overlays); break;
                case "CYCL": //Chain link fence.
                    addDefault(spr, OverlayType.WallCycl, overlays); break;
                case "FENC": //Barbwire fence (Soviet).
                    addDefault(spr, OverlayType.WallFenc, overlays); break;
                case "SBAG": //Sandbag wall.
                    addDefault(spr, OverlayType.WallSbag, overlays); break;
                case "WOOD": //Wooden fence.
                    addDefault(spr, OverlayType.WallWood, overlays); break;
                case "GEM01": //Gems.
                case "GEM02":
                case "GEM03":
                case "GEM04":
                    addDefault(spr, OverlayType.Gems, overlays); break;
                case "GOLD01": //Gold.
                case "GOLD02":
                case "GOLD03":
                case "GOLD04":
                    addDefault(spr, OverlayType.Gold, overlays); break;
                case "V12": //Farmland. Not same as structure V12-V18. These don't have an energy bar and can't be destroyed.
                case "V13":
                case "V14":
                case "V15":
                case "V16":
                case "V17":
                case "V18":
                    addDefault(spr, OverlayType.Farmland, overlays); break;
                case "FPLS": //Flag placement sign.
                    addDefault(spr, OverlayType.FlagBase, overlays); break;
                case "SCRATE": //Steel crate.
                    addLandCrate(spr, OverlayType.SteelCrate, overlays); break;
                case "WCRATE": //Wooden crate.
                    addLandCrate(spr, OverlayType.WoodenCrate, overlays); break;
                case "WWCRATE": //Water wooden crate.
                    addDefault(spr, OverlayType.WaterCrate, overlays); break;
                default: //Undefined overlay id.
                    addUndefined(spr, overlays); break;
            }
        }

        private static void addLandCrate(SpriteOverlayRA spr, OverlayType type, List<SpriteOverlayRA> overlays)
        {
            spr.mType = type;
            spr.mDrawOffset = getDrawOffsetCenter(spr); //Probably true for all overlays?
            //Crates are often hidden behind terrain or under structures.
            spr.mPriPlane = GameRA.Config.ExposeConcealed ? PriPlaneHighCrate : spr.mPriPlane;
            overlays.Add(spr);
        }

        private static void addDefault(SpriteOverlayRA spr, OverlayType type, List<SpriteOverlayRA> overlays)
        {
            spr.mType = type;
            //spr.mDrawOffset = getDrawOffsetCenter(spr); //Probably true for all overlays(?),
            //but feels a bit excessive as most are 24*24 (size of a tile) and therefore have no draw offset.
            overlays.Add(spr);
        }

        private static void addUndefined(SpriteOverlayRA spr, List<SpriteOverlayRA> overlays)
        {
            Program.warn(string.Format("Undefined overlay sprite id '{0}'!", spr.Id));
            if (GameRA.Config.AddUndefinedSprites)
            {
                addDefault(spr, OverlayType.Undefined, overlays);
            }
        }

        private static string toOverlayId(byte idValue) //Convert overlay id value to string form.
        {
            //https://web.archive.org/web/20070821071141/http://freecnc.org/dev/map-format/#AppendixB.2:RedAlert
            //Gavin Pugh, THE RED ALERT FILE FORMATS GUIDE, https://cnc.fandom.com/wiki/Red_Alert_File_Formats_Guide

            //All overlays are in CONQUER.MIX, except GOLD01-GOLD04, GEM01-GEM04 and V12-V18
            //which are only in TEMPERAT.MIX and SNOW.MIX.
            switch (idValue)
            {
                case 0x00: return "SBAG"; //Sandbag wall.
                case 0x01: return "CYCL"; //Chain link fence.
                case 0x02: return "BRIK"; //Concrete wall.
                case 0x03: return "BARB"; //Barbwire fence (Tiberian Dawn).
                case 0x04: return "WOOD"; //Wooden fence.
                case 0x05: return "GOLD01"; //Gold.
                case 0x06: return "GOLD02"; //Gold.
                case 0x07: return "GOLD03"; //Gold.
                case 0x08: return "GOLD04"; //Gold.
                case 0x09: return "GEM01"; //Gems.
                case 0x0A: return "GEM02"; //Gems.
                case 0x0B: return "GEM03"; //Gems.
                case 0x0C: return "GEM04"; //Gems.
                case 0x0D: return "V12"; //Farmland.
                case 0x0E: return "V13"; //Farmland.
                case 0x0F: return "V14"; //Farmland.
                case 0x10: return "V15"; //Farmland.
                case 0x11: return "V16"; //Farmland.
                case 0x12: return "V17"; //Farmland.
                case 0x13: return "V18"; //Farmland.
                case 0x14: return "FPLS"; //Flag placement sign.
                case 0x15: return "WCRATE"; //Wooden crate (Credits).
                case 0x16: return "SCRATE"; //Steel crate (Heal).
                case 0x17: return "FENC"; //Barbwire fence (Soviet).
                case 0x18: return "WWCRATE"; //Water crate.
                case 0xFF: return null; //No overlay here.
                default: //Unknown overlay.
                    Program.warn(string.Format("Unknown overlay sprite id value '0x{0}'!", idValue));
                    return null;
            }
        }

        public static byte toOverlayIdValue(string id) //Convert overlay id string to value form.
        {
            switch (id)
            {
                case "SBAG": return 0x00; //Sandbag wall.
                case "CYCL": return 0x01; //Chain link fence.
                case "BRIK": return 0x02; //Concrete wall.
                case "BARB": return 0x03; //Barbwire fence (Tiberian Dawn).
                case "WOOD": return 0x04; //Wooden fence.
                case "GOLD01": return 0x05; //Gold.
                case "GOLD02": return 0x06; //Gold.
                case "GOLD03": return 0x07; //Gold.
                case "GOLD04": return 0x08; //Gold.
                case "GEM01": return 0x09; //Gems.
                case "GEM02": return 0x0A; //Gems.
                case "GEM03": return 0x0B; //Gems.
                case "GEM04": return 0x0C; //Gems.
                case "V12": return 0x0D; //Farmland.
                case "V13": return 0x0E; //Farmland.
                case "V14": return 0x0F; //Farmland.
                case "V15": return 0x10; //Farmland.
                case "V16": return 0x11; //Farmland.
                case "V17": return 0x12; //Farmland.
                case "V18": return 0x13; //Farmland.
                case "FPLS": return 0x14; //Flag placement sign
                case "WCRATE": return 0x15; //Wooden crate (Credits).
                case "SCRATE": return 0x16; //Steel crate (Heal).
                case "FENC": return 0x17; //Barbwire fence (Soviet).
                case "WWCRATE": return 0x18; //Water crate.
                case null: return 0xFF; //No overlay here.
                default: //Unknown overlay.
                    Program.warn(string.Format("Unknown overlay sprite id '{0}'!", id));
                    return 0xFF;
            }
        }

        private static IEnumerable<SpriteOverlayRA> getFinalOverlays(TileTracker<SpriteOverlayRA> overlayTracker)
        {
            //Walls and ore (gems or gold) patches are affected by adjacent overlays.
            IEnumerable<SpriteOverlayRA> overlayInTiles = overlayTracker.getTileItems();
            foreach (SpriteOverlayRA overlay in overlayInTiles)
            {
                OverlayType type = overlay.mType;
                if (type == OverlayType.Gold)
                {
                    adjustGoldSprite(overlay, overlayTracker);
                }
                else if (type == OverlayType.Gems)
                {
                    adjustGemsSprite(overlay, overlayTracker);
                }
                else if (isWall(type))
                {
                    adjustWallSprite(overlay, overlayTracker);
                }
            }
            return overlayInTiles;
        }

        private static void adjustWallSprite(SpriteOverlayRA wallSprite, TileTracker<SpriteOverlayRA> overlayTracker)
        {
            OverlayType wallType = wallSprite.mType;
            TilePos tilePos = wallSprite.TilePos;
            int index = wallSprite.mFrameIndex;
            //Check for adjacent walls of same type and adjust frame index.
            if (hasTileWallType(overlayTracker.getAdjacentN(tilePos), wallType)) index += 1;
            if (hasTileWallType(overlayTracker.getAdjacentE(tilePos), wallType)) index += 2;
            if (hasTileWallType(overlayTracker.getAdjacentS(tilePos), wallType)) index += 4;
            if (hasTileWallType(overlayTracker.getAdjacentW(tilePos), wallType)) index += 8;
            wallSprite.mFrameIndex = index;
        }

        private static bool hasTileWallType(SpriteOverlayRA overlayInTile, OverlayType wallType)
        {
            return overlayInTile != null && overlayInTile.mType == wallType;
        }

        private static void adjustGemsSprite(SpriteOverlayRA gemsSprite, TileTracker<SpriteOverlayRA> overlayTracker)
        {
            //Which ore (gems or gold) SHP-file (GEM01-GEM04 and GOLD01-GOLD04) to use seems to be random. They are
            //different every time you restart a mission. The id values in the overlay section in
            //mission INI-files seems to be ignored and only tile position is used i.e. which
            //tiles have ore. Ore SHP-file frame index is determined by number of adjacent
            //ore tiles though. Checked in game.
            //This applies to ore at start. New ore added during game, because of mines,
            //always(?) start at index 1 regardless of number of adjacent tiles.
            gemsSprite.mFrameIndex = getFrameIndexGems(getAdjOreCount(gemsSprite, overlayTracker));
        }

        private static void adjustGoldSprite(SpriteOverlayRA goldSprite, TileTracker<SpriteOverlayRA> overlayTracker)
        {
            goldSprite.mFrameIndex = getFrameIndexGold(getAdjOreCount(goldSprite, overlayTracker));
        }

        private static int getAdjOreCount(SpriteOverlayRA oreSprite, TileTracker<SpriteOverlayRA> overlayTracker)
        {
            TilePos tilePos = oreSprite.TilePos;
            int adjOreCount = 0; //Count adjacent ore tiles in a 3x3 area.
            if (hasTileOre(overlayTracker.getAdjacentN(tilePos))) adjOreCount++;
            if (hasTileOre(overlayTracker.getAdjacentNE(tilePos))) adjOreCount++;
            if (hasTileOre(overlayTracker.getAdjacentE(tilePos))) adjOreCount++;
            if (hasTileOre(overlayTracker.getAdjacentSE(tilePos))) adjOreCount++;
            if (hasTileOre(overlayTracker.getAdjacentS(tilePos))) adjOreCount++;
            if (hasTileOre(overlayTracker.getAdjacentSW(tilePos))) adjOreCount++;
            if (hasTileOre(overlayTracker.getAdjacentW(tilePos))) adjOreCount++;
            if (hasTileOre(overlayTracker.getAdjacentNW(tilePos))) adjOreCount++;
            return adjOreCount;
        }

        private static bool hasTileOre(SpriteOverlayRA overlayInTile)
        {
            return overlayInTile != null && (overlayInTile.mType == OverlayType.Gems || overlayInTile.mType == OverlayType.Gold);
        }

        private static int getFrameIndexGems(int adjOreCount)
        {
            //Gems frame index is determined by the number of adjacent ore (gems or gold) tiles.
            //I.e. number of ore tiles in a 3x3 area excluding the center. Checked in game.
            return adjOreCount / 3;
        }

        private static int getFrameIndexGold(int adjOreCount)
        {
            //Gold frame index is determined by the number of adjacent ore (gems or gold) tiles.
            //I.e. number of ore tiles in a 3x3 area excluding the center. Checked in game.
            switch (adjOreCount) //Ore tiles in a 3x3 area minus one (center).
            {
                case 0: return 0;
                case 1: return 1;
                case 2: return 3;
                case 3: return 4;
                case 4: return 6;
                case 5: return 7;
                case 6: return 8;
                case 7: return 10;
                case 8: return 11;
                default: throw new ArgumentException("Only 0-8 are valid adjacent ore counts!");
            }
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            //"Render_Overlay()" in "RADAR.CPP" seems to use IsRadarVisible, but only gems and gold are drawn
            //and they are set to true so it shouldn't affect anything in the end. Overlays with missing
            //graphics are not drawn. Checked in source and game.
            if ((mType == OverlayType.Gems || mType == OverlayType.Gold) && !mFileShp.getFrame(mFrameIndex).IsEmpty)
            {
                if (scale == 1)
                {
                    image[TilePos.Location] = 13; //"Render_Overlay()" in "RADAR.CPP".
                }
                else
                {
                    //Always(?) only 1 tile drawn for all overlays regardless of their size (which usually is 1 tile though).
                    RadarRA.drawMiniTiles(scale, this, map.Theater.getYellowRemap(), image, true);
                }
            }
        }

        public byte getLandTypeRadarIndex()
        {
            //Return the land type radar color (palette index) of this overlay.
            switch (mType)
            {
                case OverlayType.None: return 141; //Clear. Land type.
                case OverlayType.WallBarb: return 21; //Wall.
                case OverlayType.WallBrik: return 21; //Wall.
                case OverlayType.WallCycl: return 21; //Wall.
                case OverlayType.WallFenc: return 21; //Wall.
                case OverlayType.WallSbag: return 21; //Wall.
                case OverlayType.WallWood: return 21; //Wall.
                case OverlayType.Gems: return 158; //Tiberium.
                case OverlayType.Gold: return 158; //Tiberium.
                case OverlayType.Farmland: return 21; //Rock.
                case OverlayType.FlagBase: return 141; //Clear.
                case OverlayType.SteelCrate: return 141; //Clear.
                case OverlayType.WoodenCrate: return 141; //Clear.
                case OverlayType.WaterCrate: return 172; //Water.
                case OverlayType.Undefined: return 141; //Clear.
                default: throw new ArgumentException(); //Should never happen.
            }
        }
    }
}
