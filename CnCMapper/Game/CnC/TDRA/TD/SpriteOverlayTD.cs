using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class SpriteOverlayTD : SpriteTDRA
    {
        private enum OverlayType
        {
            None = 0, //Init value. Shouldn't be used otherwise.
            WallBarb, //Barbwire fence. Walls must be listed in this order.
            WallBrik, //Concrete wall.
            WallCycl, //Chain link fence.
            WallSbag, //Sandbag wall.
            WallWood, //Wooden fence.
            Tiberium,
            Farmland, //V12-V18 neutral farmland structures.
            FlagBase, //Flag placement sign (capture the flag).
            SteelCrate,
            WoodenCrate,
            Concrete,
            ConcreteFixup, //Fixup tiles adjacent to concrete. Special type so it's not mixed up with "real" concrete.
            Road,
            Squish, //Squish mark (can only be added by crushed infantry?).
            Undefined,
        }
        private OverlayType mType = OverlayType.None;

        private SpriteOverlayTD(TilePos tilePos, FileShpSpriteSetTDRA fileShp)
            : base(fileShp.Id, tilePos, fileShp)
        {
            mPriPlane = PriPlaneOverlay;
        }

        private static bool isWall(OverlayType type)
        {
            return type >= OverlayType.WallBarb && type <= OverlayType.WallWood;
        }

        public static void endAdd(List<SpriteOverlayTD> overlays, List<SpriteStructureTD> structures, MapTD map, List<SpriteTDRA> sprites)
        {
            //Overlays and structures in lists must be in the order they were added from the INI-file.

            //Walls (BARB, BRIK, CYCL, SBAG, WOOD) are also defined in structures. Tests indicate that
            //these are (after overlays are added) converted to neutral overlay walls and replaces all
            //existing overlays (including ROAD and SQUISH) except walls. Checked in game.
            //If wall structures overlap, only the last (highest line number in INI-file) is added.
            //House (color remap), health and direction fields in structure keys don't affect walls.

            //Farmlands (V12-V18) are also defined in structures. These are different from overlays
            //and added as structures and do not affect overlays. Main (only?) difference is that
            //structure farmlands have an energy bar and can be damaged/destroyed. Checked in game.

            //I wonder why walls and farmlands even are overlays at all? Seems like it would be
            //better to always treat them like structures in the game instead?

            //Road (ROAD) is a bit weird and it seems like its frame index is increased if any
            //overlay tries to replace it. Kinda like smudge craters do? Checked in game.

            //Squish mark (SQUISH) seems to be added (overlay can't overlap it), but not drawn? Checked in game.

            //Overlays don't overlap, only first overlay at a tile number is added. Checked in game.
            TileTracker<SpriteOverlayTD> overlayTracker = new TileTracker<SpriteOverlayTD>(MapTD.SizeInTiles);
            foreach (SpriteOverlayTD overlay in overlays)
            {
                SpriteOverlayTD overlayInTile = overlayTracker[overlay.TilePos];
                if (overlayInTile == null) //Add overlay if empty tile.
                {
                    overlayTracker.replaceItem(overlay);
                }
                else if (overlayInTile.mType == OverlayType.Road) //Road's frame index is increased if overlapped?
                {
                    overlayInTile.increaseFrameIndex();
                }
            }

            //Convert any structure walls to overlays.
            convertStructureWalls(structures, overlayTracker);

            //Adjust and add final overlays.
            sprites.AddDerivedRange(getFinalOverlays(map, overlayTracker));
        }

        public static void add(MapTD map, List<SpriteOverlayTD> overlays)
        {
            //Format: tileNumber=id
            //Example: 3040=TI10
            IniSection iniSection = map.FileIni.findSection("OVERLAY");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    TilePos tilePos = MapTD.toTilePos(key.Id);
                    FileShpSpriteSetTDRA fileShp = map.Theater.getSpriteSet(key.Value);
                    SpriteOverlayTD spr = new SpriteOverlayTD(tilePos, fileShp);
                    addInner(spr, overlays);
                }
            }
        }

        private static void addInner(SpriteOverlayTD spr, List<SpriteOverlayTD> overlays)
        {
            switch (spr.Id)
            {
                case "BARB": //Barbwire fence.
                    addDefault(spr, OverlayType.WallBarb, overlays); break;
                case "BRIK": //Concrete wall.
                    addDefault(spr, OverlayType.WallBrik, overlays); break;
                case "CYCL": //Chain link fence.
                    addDefault(spr, OverlayType.WallCycl, overlays); break;
                case "SBAG": //Sandbag wall.
                    addDefault(spr, OverlayType.WallSbag, overlays); break;
                case "WOOD": //Wooden fence.
                    addDefault(spr, OverlayType.WallWood, overlays); break;
                case "TI1": //Tiberium.
                case "TI2":
                case "TI3":
                case "TI4":
                case "TI5":
                case "TI6":
                case "TI7":
                case "TI8":
                case "TI9":
                case "TI10":
                case "TI11":
                case "TI12":
                    addDefault(spr, OverlayType.Tiberium, overlays); break;
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
                    addCrate(spr, OverlayType.SteelCrate, overlays); break;
                case "WCRATE": //Wooden crate.
                    addCrate(spr, OverlayType.WoodenCrate, overlays); break;

                case "CONC": //Concrete. CONC and ROAD are defined and added in the game, but never used?
                    addDefault(spr, OverlayType.Concrete, overlays); break;
                case "ROAD": //Road.
                    addDefault(spr, OverlayType.Road, overlays); break;
                case "SQUISH": //Squish mark (overran infantry). Defined and added(?), but not drawn in game.
                    addSquish(spr, overlays); break;

                default: //Undefined overlay id.
                    addUndefined(spr, overlays); break;
            }
        }

        private static void addCrate(SpriteOverlayTD spr, OverlayType type, List<SpriteOverlayTD> overlays)
        {
            spr.mType = type;
            spr.mDrawOffset = getDrawOffsetCenter(spr); //Probably true for all overlays?
            //Crates are often hidden behind terrain or under structures.
            spr.mPriPlane = GameTD.Config.ExposeConcealed ? PriPlaneHighCrate : spr.mPriPlane;
            overlays.Add(spr);
        }

        private static void addSquish(SpriteOverlayTD spr, List<SpriteOverlayTD> overlays)
        {
            spr.mDrawMode = DrawMode.Invisible; //Squish marks are added(?), but not drawn. Checked in game.
            addDefault(spr, OverlayType.Squish, overlays);
        }

        private static void addDefault(SpriteOverlayTD spr, OverlayType type, List<SpriteOverlayTD> overlays)
        {
            spr.mType = type;
            //spr.mDrawOffset = getDrawOffsetCenter(spr); //Probably true for all overlays(?),
            //but feels a bit excessive as most are 24*24 (size of a tile) and therefore have no draw offset.
            overlays.Add(spr);
        }

        private static void addUndefined(SpriteOverlayTD spr, List<SpriteOverlayTD> overlays)
        {
            Program.warn(string.Format("Undefined overlay sprite id '{0}'!", spr.Id));
            if (GameTD.Config.AddUndefinedSprites)
            {
                addDefault(spr, OverlayType.Undefined, overlays);
            }
        }

        private static void convertStructureWalls(List<SpriteStructureTD> structures, TileTracker<SpriteOverlayTD> overlayTracker)
        {
            //Converts any normal wall structures in the sprite list to wall overlays.
            //Not sure how to deal with base walls (can they even exist?) so just ignore them for now.
            //Walls in structure section is rare though (never used in normal maps?) so really no need
            //to optimize this method. Pretty much only implemented for running my tests.

            List<SpriteOverlayTD> convertedWalls = new List<SpriteOverlayTD>();
            foreach (SpriteStructureTD structure in structures)
            {
                if (structure.IsWall && !structure.IsBase) //Is a normal wall structure?
                {
                    FileShpSpriteSetTDRA fileShp = structure.FileShp; //Same(?) SHP-file used by both structure and overlay walls.
                    SpriteOverlayTD spr = new SpriteOverlayTD(structure.TilePos, fileShp);
                    addInner(spr, convertedWalls);

                    System.Diagnostics.Debug.Assert(isWall(spr.mType), "Converted structure isn't a wall!");
                }
            }

            //If wall structures overlap, only the last (highest line number in INI-file) is added.
            //Loop over converted walls in reverse and only add first one to mimic this.
            foreach (SpriteOverlayTD convertedWall in convertedWalls.Reversed())
            {
                SpriteOverlayTD overlayInTile = overlayTracker[convertedWall.TilePos];
                //Structure walls replaces all overlays except walls.
                if (!(overlayInTile != null && isWall(overlayInTile.mType))) //Not an overlay wall?
                {
                    overlayTracker.replaceItem(convertedWall);
                }
            }
        }

        private static IEnumerable<SpriteOverlayTD> getFinalOverlays(MapTD map, TileTracker<SpriteOverlayTD> overlayTracker)
        {
            //Walls, tiberium and concrete are affected by adjacent overlays.
            IEnumerable<SpriteOverlayTD> overlayInTiles = overlayTracker.getTileItems();
            foreach (SpriteOverlayTD overlay in overlayInTiles)
            {
                OverlayType type = overlay.mType;
                if (type == OverlayType.Tiberium)
                {
                    adjustTiberiumSprite(overlay, overlayTracker);
                }
                else if (isWall(type))
                {
                    adjustWallSprite(overlay, overlayTracker);
                }
                else if (type == OverlayType.Concrete)
                {
                    adjustConcreteSprite(overlay, map, overlayTracker);
                }
            }
            return overlayInTiles;
        }

        private static void adjustWallSprite(SpriteOverlayTD wallSprite, TileTracker<SpriteOverlayTD> overlayTracker)
        {
            OverlayType wallType = wallSprite.mType;
            TilePos tilePos = wallSprite.TilePos;
            int frameIndex = 0;
            //Check for adjacent walls of same type and adjust frame index.
            if (hasTileWallType(overlayTracker.getAdjacentN(tilePos), wallType)) frameIndex += 1;
            if (hasTileWallType(overlayTracker.getAdjacentE(tilePos), wallType)) frameIndex += 2;
            if (hasTileWallType(overlayTracker.getAdjacentS(tilePos), wallType)) frameIndex += 4;
            if (hasTileWallType(overlayTracker.getAdjacentW(tilePos), wallType)) frameIndex += 8;
            wallSprite.mFrameIndex = frameIndex;
        }

        private static bool hasTileWallType(SpriteOverlayTD overlayInTile, OverlayType wallType)
        {
            return overlayInTile != null && overlayInTile.mType == wallType;
        }

        private static void adjustTiberiumSprite(SpriteOverlayTD tibSprite, TileTracker<SpriteOverlayTD> overlayTracker)
        {
            //Which tiberium SHP-file (ti1-ti12) to use seems to be random. They are different
            //every time you restart a mission. The id-values in the "[OVERLAY]" section in
            //mission INI-files seems to be ignored and only tile position is used i.e. which
            //tiles have tiberium. Tiberium SHP-file frame index is determined by number of
            //adjacent tiberium tiles though. Checked in game.
            //This applies to tiberium at start. New tiberium added during game, because of
            //growth, always(?) start at index 1 regardless of number of adjacent tiles.
            TilePos tilePos = tibSprite.TilePos;
            int adjTibCount = 0; //Count adjacent tiberium tiles.
            if (hasTileTiberium(overlayTracker.getAdjacentN(tilePos))) adjTibCount++;
            if (hasTileTiberium(overlayTracker.getAdjacentNE(tilePos))) adjTibCount++;
            if (hasTileTiberium(overlayTracker.getAdjacentE(tilePos))) adjTibCount++;
            if (hasTileTiberium(overlayTracker.getAdjacentSE(tilePos))) adjTibCount++;
            if (hasTileTiberium(overlayTracker.getAdjacentS(tilePos))) adjTibCount++;
            if (hasTileTiberium(overlayTracker.getAdjacentSW(tilePos))) adjTibCount++;
            if (hasTileTiberium(overlayTracker.getAdjacentW(tilePos))) adjTibCount++;
            if (hasTileTiberium(overlayTracker.getAdjacentNW(tilePos))) adjTibCount++;
            tibSprite.mFrameIndex = getFrameIndexTiberium(adjTibCount);
        }

        private static bool hasTileTiberium(SpriteOverlayTD overlayInTile)
        {
            return overlayInTile != null && overlayInTile.mType == OverlayType.Tiberium;
        }

        private static int getFrameIndexTiberium(int adjTibCount)
        {
            //Tiberium frame index is determined by the number of adjacent tiberium tiles.
            //I.e. number of tiberium tiles in a 3*3 area excluding the center. Checked in game.
            switch (adjTibCount)
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
                default: throw new ArgumentException("Only 0-8 are valid adjacent tiberium counts!");
            }
        }

        private static void adjustConcreteSprite(SpriteOverlayTD concSprite, MapTD map, TileTracker<SpriteOverlayTD> overlayTracker)
        {
            //Concrete (CONC) is weird. I converted "Concrete_Calc()" in "CELL.CPP" to a C# version,
            //but it still didn't 100% match tests I've done in the game. Concrete seems really random
            //in the game with no obvious pattern. "Concrete_Calc()" seems to be able to add new
            //concrete to a map to fill in missing gaps (frame index 4, 5, 6, 7, 12 and 13 in "CONC.SHP"),
            //but I've never seen these filler frames in the game. I guess that feature doesn't work for
            //some reason? Perhaps it's this feature that is causing the random concrete? There probably
            //is a good reason why concrete isn't used in any normal mission map or why you can't build
            //it. Maybe concrete wasn't finished in time and cut from the released game?

            //I made a test which surrounded a center concrete tile with up to eight (256 combinations),
            //both on even and odd tile number, and used the in game result to create two look-up-tables (2*256 entries).
            //That should've covered all cases, but it still didn't 100% match the game. The center tile was
            //correct, but not the surrounding tiles. Seems like not just adjacent tiles and tile number
            //affects concrete. There are other factors too?

            //Concrete is affected by an even or odd tile number which makes me suspect that it probably
            //should be added in pairs (left and right) i.e concrete tiles are really 48*24 and not 24*24.
            //At least concrete looks a lot better in pairs than single. Question is if all pairs must start
            //on an even or odd tile number though. Probably even, but odd actually looks pretty good too
            //and it doesn't need fixup pieces.

            //This method adjusts concrete tiles like I assume they should be. It doesn't match the game.
            //Pretty close to the source though and same if you add one extra switch-case to each frame index.
            //Seems like they should be there to make concrete look better anyway.
            //2: case OF_SE | OF_E | OF_NE:
            //8: case OF_SE | OF_N:
            //10: case OF_S | OF_NE:
            //3: case EF_NW | EF_W | EF_SW:
            //9: case EF_SW | EF_N:
            //11: case EF_NW | EF_S:

            //TODO: Figure out concrete better? Rarely (never?) used so not worth bothering with?

            TilePos tilePos = concSprite.TilePos;
            int frameIndex = 0;
            bool hasN = hasTileConcrete(overlayTracker.getAdjacentN(tilePos));
            bool hasE = hasTileConcrete(overlayTracker.getAdjacentE(tilePos));
            bool hasS = hasTileConcrete(overlayTracker.getAdjacentS(tilePos));
            bool hasW = hasTileConcrete(overlayTracker.getAdjacentW(tilePos));
            if ((tilePos.X & 0x01) != 0) //Odd: Affected by adjacent N, NE, E, SE, S.
            {
                bool hasNE = hasTileConcrete(overlayTracker.getAdjacentNE(tilePos));
                bool hasSE = hasTileConcrete(overlayTracker.getAdjacentSE(tilePos));

                if ((hasNE && hasSE) || (hasN && hasE && hasS) || (hasN && hasE && hasSE) || (hasNE && hasE && hasS)) frameIndex = 2; //C_RIGHT_UPDOWN
                else if (hasSE || (hasE && hasS)) frameIndex = 8; //C_RIGHT_DOWN
                else if (hasNE || (hasN && hasE)) frameIndex = 10; //C_RIGHT_UP
                else frameIndex = 1; //C_RIGHT
            }
            else //Even: Affected by adjacent N, NW, W, SW, S.
            {
                bool hasNW = hasTileConcrete(overlayTracker.getAdjacentNW(tilePos));
                bool hasSW = hasTileConcrete(overlayTracker.getAdjacentSW(tilePos));

                if ((hasNW && hasSW) || (hasN && hasW && hasS) || (hasN && hasW && hasSW) || (hasNW && hasW && hasS)) frameIndex = 3; //C_LEFT_UPDOWN
                else if (hasSW || (hasW && hasS)) frameIndex = 9; //C_LEFT_DOWN
                else if (hasNW || (hasN && hasW)) frameIndex = 11; //C_LEFT_UP
                else frameIndex = 0; //C_LEFT
            }
            concSprite.mFrameIndex = frameIndex;

            //Check if adjacent tiles with no concrete need fixup pieces.
            if (!hasN) fixupConcrete(tilePos.getOffset(0, -1), map, overlayTracker); //N.
            if (!hasE) fixupConcrete(tilePos.getOffset(1, 0), map, overlayTracker); //E.
            if (!hasS) fixupConcrete(tilePos.getOffset(0, 1), map, overlayTracker); //S.
            if (!hasW) fixupConcrete(tilePos.getOffset(-1, 0), map, overlayTracker); //W.
        }

        private static void fixupConcrete(TilePos tilePos, MapTD map, TileTracker<SpriteOverlayTD> overlayTracker)
        {
            //If a tile with no concrete has adjacent concrete then a fixup piece may be needed.
            if (overlayTracker.isInside(tilePos))
            {
                //If a tile with no concrete has adjacent concrete then a fixup piece may be needed.
                //Should've been checked before this method is called.
                System.Diagnostics.Debug.Assert(!hasTileConcrete(overlayTracker[tilePos])); //No concrete?

                int frameIndex = -1; //Assume no fixup piece needed.
                bool hasN = hasTileConcrete(overlayTracker.getAdjacentN(tilePos));
                bool hasS = hasTileConcrete(overlayTracker.getAdjacentS(tilePos));
                if ((tilePos.X & 0x01) != 0) //Odd: Affected by adjacent N, E, S.
                {
                    bool hasE = hasTileConcrete(overlayTracker.getAdjacentE(tilePos));

                    if (hasN && hasE && hasS) frameIndex = 12; //C_UPDOWN_RIGHT
                    else if (hasE && hasS) frameIndex = 6; //C_DOWN_RIGHT
                    else if (hasN && hasE) frameIndex = 4; //C_UP_RIGHT
                }
                else //Even: Affected by adjacent N, S, W.
                {
                    bool hasW = hasTileConcrete(overlayTracker.getAdjacentW(tilePos));

                    if (hasN && hasW && hasS) frameIndex = 13; //C_UPDOWN_LEFT
                    else if (hasW && hasS) frameIndex = 7; //C_DOWN_LEFT
                    else if (hasN && hasW) frameIndex = 5; //C_UP_LEFT
                }

                if (frameIndex >= 0) //Fixup piece is needed?
                {
                    SpriteOverlayTD spr = new SpriteOverlayTD(tilePos, map.Theater.getSpriteSet("CONC"));
                    spr.mType = OverlayType.ConcreteFixup;
                    spr.mFrameIndex = frameIndex;
                    overlayTracker.addItem(spr); //Only add piece if tile has no overlay (*1).

                    //*1= Does a concrete fixup piece replace (all?) other overlays?
                    //If it does then all pieces must be added before getFinalOverlays() is called,
                    //because a piece could affect walls and tiberium by removing them.

                    //Difficult to test because fixup pieces aren't added in the game. Check source?
                    //I did some quick testing in case fixup pieces are actually added, but not drawn,
                    //and existing overlays where not removed at fixup locations in the game.

                    //For now only add piece if tile has no overlay because it's simpler and
                    //other overlay types usually don't replace existing overlays.
                }
            }
        }

        private static bool hasTileConcrete(SpriteOverlayTD overlayInTile)
        {
            //Only check for "real" concrete, not fixup pieces.
            return overlayInTile != null && overlayInTile.mType == OverlayType.Concrete;
        }

        public override void drawRadar(int scale, MapTDRA map, IndexedImage image)
        {
            byte[] colorRemap;
            if (mType == OverlayType.Tiberium)
            {
                colorRemap = map.Theater.getGreenRemap();
            }
            else if (mType == OverlayType.Squish || mType == OverlayType.Farmland ||
                mType == OverlayType.SteelCrate || mType == OverlayType.WoodenCrate) //IsRadarVisible==false?
            {
                //"Render_Overlay()" in "RADAR.CPP" does check if OverlayTypeClass->IsRadarVisible.
                //SQUISH, V12-V18, SCRATE and WCRATE are not radar visible (IsRadarVisible=false in "ODATA.CPP").
                //Checked in source and game.
                return;
            }
            else
            {
                colorRemap = map.Theater.getBrightenRemap();
            }
            //Always(?) only 1 tile drawn for all overlays regardless of their size (which usually is 1 tile though).
            RadarTD.drawMiniTiles(scale, this, colorRemap, image, true);
        }

        public byte getLandTypeRadarIndex()
        {
            //Return the land type radar color (palette index) of this overlay.
            switch (mType)
            {
                case OverlayType.None: return 66; //Clear. Land type.
                case OverlayType.WallBarb: return 13; //Wall.
                case OverlayType.WallBrik: return 13; //Wall.
                case OverlayType.WallCycl: return 13; //Wall.
                case OverlayType.WallSbag: return 13; //Wall.
                case OverlayType.WallWood: return 13; //Wall.
                case OverlayType.Tiberium: return 143; //Tiberium.
                case OverlayType.Farmland: return 13; //Rock.
                case OverlayType.FlagBase: return 66; //Clear.
                case OverlayType.SteelCrate: return 66; //Clear.
                case OverlayType.WoodenCrate: return 66; //Clear.
                case OverlayType.Concrete: return 68; //Road.
                case OverlayType.ConcreteFixup: return 68; //Road.
                case OverlayType.Road: return 68; //Road.
                case OverlayType.Squish: return 66; //Clear.
                case OverlayType.Undefined: return 66; //Clear.
                default: throw new ArgumentException(); //Should never happen.
            }
        }
    }
}
