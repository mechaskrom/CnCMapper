using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    class SpriteStructureD2 : SpriteD2
    {
        //https://forum.dune2k.com/topic/19099-format80-decompression/page/3/#comment-336619
        //ArrakisResearchCo Posted December 20, 2008
        //"
        //The "Windtrap" structure uses palette blending to create the animation of the three "towers".
        //Palette entry 223 is used for this palette blending. All other structures use tile swapping to
        //create the structure animations - including all the flags.
        //"

        //https://forum.dune2k.com/topic/19099-format80-decompression/page/3/#comment-336439
        //ArrakisResearchCo Posted December 15, 2008
        //"
        //Well - the turret tile you mentioned - tile 367 - is missing a few pixels at the turret base
        //in the v1.0 ICON.ICN file. It should have a wider bright base at the top of the tile and a wider
        //shadow base at the bottom of the tile on both sides of the turret.
        //"

        //https://forum.dune2k.com/topic/19099-format80-decompression/?do=findComment&comment=336531
        //https://forum.dune2k.com/topic/20343-differences-between-dune-ii-v10-and-v107/?do=findComment&comment=341690
        //Some differences (in tiles and palettes among other things) between Dune 2 versions.

        //The "IX" structure uses palette indices in the remap range on things that shouldn't
        //be remapped and will look weird with other color schemes than neutral (Red/Harkonnen).


        private TileSetD2 mTileSet = null;
        private Section[] mSections = null; //The tiles that make up this structure. [0]=top left tile.
        private readonly bool mIsGen; //Structure was added with the GEN format? Otherwise the ID format.

        private SpriteStructureD2(string id, TilePos tilePos, HouseD2 house, bool isGen)
            : base(id, tilePos, PriPlaneBuilding, house, ActionNull)
        {
            mIsGen = isGen;
        }

        private class Section : ITilePos //Tiled section of a structure.
        {
            private readonly SpriteStructureD2 mOwner;
            private readonly TilePos mTilePos;
            private byte mTileSetIndex;
            private bool mIsHidden; //Tile was overlapped by another structure or was not added.

            public Section(SpriteStructureD2 owner, TilePos tilePos, int tileSetIndex)
            {
                mOwner = owner;
                mTilePos = tilePos;
                mTileSetIndex = (byte)tileSetIndex;
                mIsHidden = false;
            }

            public SpriteStructureD2 Owner { get { return mOwner; } }
            public TilePos TilePos { get { return mTilePos; } }
            public byte TileSetIndex { get { return mTileSetIndex; } set { mTileSetIndex = value; } }
            public bool IsHidden { get { return mIsHidden; } set { mIsHidden = value; } }
            public bool IsGen { get { return mOwner.IsGen; } }
            public bool IsSlab { get { return mOwner.IsSlab; } }
            public bool IsWall { get { return mOwner.IsWall; } }
            public bool IsSlabOrWall { get { return IsSlab || IsWall; } }
        }

        public static TileSetD2 getTileSet(string id)
        {
            switch (id)
            {
                case "Concrete": return TileSetD2.ConcreteSlab; //Concrete slab.
                case "Concrete4": return TileSetD2.Concrete4Slab; //Concrete slab (2*2).
                case "Wall": return TileSetD2.Wall; //Wall.

                case "Turret": return TileSetD2.Turret; //Turret.
                case "R-Turret": return TileSetD2.RocketTurret; //Rocket Turret.

                case "Const Yard": return TileSetD2.ConstructionYard; //Construction Yard.
                case "Windtrap": return TileSetD2.Windtrap; //Windtrap.
                case "Barracks": return TileSetD2.Barracks; //Barracks.
                case "Light Fctry": return TileSetD2.LightFactory; //Light factory.
                case "Heavy Fctry": return TileSetD2.HeavyFactory; //Heavy factory.
                case "Hi-Tech": return TileSetD2.HighTechFactory; //High tech factory.
                case "Refinery": return TileSetD2.Refinery; //Refinery.
                case "Spice Silo": return TileSetD2.SpiceSilos; //Spice Silos.
                case "Outpost": return TileSetD2.Outpost; //Outpost (radar).
                case "Repair": return TileSetD2.RepairFacility; //Repair Facility.
                case "IX": return TileSetD2.IX; //IX (research centre).
                case "WOR": return TileSetD2.WOR; //WOR (trooper training facility).
                case "Palace": return TileSetD2.Palace; //Palace.
                case "Starport": return TileSetD2.Starport; //Starport.

                default:
                    throw new ArgumentException(string.Format("'{0}' is not a valid structure id!", id));
            }
        }

        public bool IsGen
        {
            get { return mIsGen; }
        }

        public bool IsSlab
        {
            get { return mTileSet.Id == TileSetD2.IdConcreteSlab; }
        }

        public bool IsWall
        {
            get { return mTileSet.Id == TileSetD2.IdWall; }
        }

        public bool IsSlabOrWall //Is concrete?
        {
            get { return IsSlab || IsWall; }
        }

        private bool getIsHidden() //Is structure (all its sections) hidden?
        {
            foreach (Section section in mSections)
            {
                if (!section.IsHidden)
                {
                    return false;
                }
            }
            return true;
        }

        private void setIsHidden(bool isHidden)
        {
            foreach (Section section in mSections)
            {
                section.IsHidden = isHidden;
            }
        }

        public static bool[] getTilesWithStructure(List<SpriteStructureD2> structures)
        {
            bool[] tilesWithStructure = new bool[MapD2.WidthInTiles * MapD2.HeightInTiles];
            foreach (SpriteStructureD2 structure in structures)
            {
                foreach (Section section in structure.mSections)
                {
                    //Do not consider hidden sections.
                    tilesWithStructure[MapD2.toTileNum(section.TilePos)] = !section.IsHidden;
                }
            }
            return tilesWithStructure;
        }

        public static void endAdd(MapD2 map, List<SpriteStructureD2> structures)
        {
            //Structures in list must be in the order they were added from the INI-file.

            //Terminology used here:
            //-Concrete = Walls and concrete slabs (1x1 and 2x2). Usually added with the GEN-format.
            //-Building = Everything except walls and concrete slabs. Usually added with the ID-format.
            //-Structure = Both concrete and building.

            //Overlapping structures are a bit weird in the game. Testing seems to indicate that
            //walls and concrete slabs are treated differently and have lower priority than other
            //structure types. A wall will update its and adjacent wall's frame index when added,
            //but not when replaced. If a building is added it removes any walls and concrete slabs
            //beneath it.

            //It seems like walls added at an existing building affect adjacent walls. Maybe the
            //game actually adds them to the map, but they are shortly after replaced by the
            //building that is there when updating the map?

            //Looking at OpenDUNE's source code it seems like slabs and walls directly change the
            //ground layer tiles when placed. Buildings however are stored in an array and change
            //the ground layer tiles when placed and on map updates?

            //Testing seems to indicate that overlapping structures:
            //-Added in order in INI-file i.e. lower line number is added first.
            //-Structures are always added if in the GEN-format.
            //-Structures are not added in the ID-format if the top left tile is occupied by another structure.
            //  Except if the existing structure is a concrete slab or wall.
            //-Concrete slabs and walls seem to not hide existing building even if added?
            //  Maybe they are added, but shortly after replaced by existing building?
            //-Overlapping building sections will flicker (looks a bit like Z-fighting).

            //Surely not 100% accurate, but maps usually don't have overlapping structures
            //so it's not really worth investigating more into.

            //Copy list of structures and create a new one with only structures that were added.
            TileTracker<Section> structureTracker = new TileTracker<Section>(MapD2.SizeInTiles);
            foreach (SpriteStructureD2 structure in structures)
            {
                Section structureInTile = structureTracker[structure.TilePos];
                //Check if structure can be added i.e. top left tile is not occupied by a building.
                //Structures added with the GEN format seems to ignore this check.
                if (structure.IsGen || structureInTile == null || structureInTile.IsSlabOrWall)
                {
                    foreach (Section newSection in structure.mSections)
                    {
                        Section oldSection = structureTracker[newSection.TilePos];
                        structureTracker.replaceItem(newSection);
                        if (newSection.IsWall) //A wall was added? Then adjust it and adjacent walls.
                        {
                            adjustAdjacentWalls(newSection, structureTracker);
                        }

                        if (oldSection != null) //There was already a section in this tile?
                        {
                            //Walls and slabs are added, but don't hide existing building sections?
                            oldSection.IsHidden = oldSection.IsSlabOrWall || !newSection.IsSlabOrWall;

                            //If old section wasn't hidden then it means it had higher priority
                            //and new section should be hidden instead. This can happen if a slab
                            //or wall was added at an existing building.
                            newSection.IsHidden = !oldSection.IsHidden;
                        }
                    }
                }
                else //Hide structure if it couldn't be added.
                {
                    structure.setIsHidden(true);
                }
            }

            //Structures with all their sections hidden should probably not be added to final list.
            //But since sections with the hidden flag set isn't drawn it doesn't really matter.
            structures.RemoveAll((SpriteStructureD2 s) => s.getIsHidden());
        }

        public static void add(MapD2 map, List<SpriteStructureD2> structures)
        {
            //Structure entries can have two formats: one for concrete and one for buildings.
            //The game might behave strange if the wrong format is used for a type?
            //I tried all structures with both formats in the game and it seemed to work fine though?
            //Except sometimes some buildings disappear/aren't added?

            //Concrete.
            //Format: GEN<tileNumber>=house,id
            //Example: GEN1592=Atreides,Concrete
            //Id should only be "Wall", "Concrete" or "Concrete4".
            //"Concrete4" is never used in any of the game's maps, but seems to work.

            //Buildings.
            //Format: ID<number>=house,id,health,tileNumber
            //Example: ID022=Sardaukar,Const Yard,256,122
            //Id should not be "Wall", "Concrete" or "Concrete4".
            //Health field does nothing? Buildings always have full health? It's not a direction
            //field for turrets like in Tiberian Dawn and Red Alert either. Checked in game.

            //Structures with the same key id seems to behave the same as units i.e. the first entry
            //is repeated. Use the key finder class to mimic this behavior.

            //IniSection iniSection = map.FileIni.findSection("STRUCTURES");
            IniKeyFinderD2 keyFinder = IniKeyFinderD2.create(map.FileIni.findSection("STRUCTURES"));
            //if (iniSection != null)
            if (keyFinder != null)
            {
                //foreach (IniKey key in iniSection.Keys)
                foreach (IniKey key in keyFinder.findKeys())
                {
                    string[] values = key.Value.Split(',');
                    if (key.Id.StartsWith("GEN")) //Concrete wall or slab?
                    {
                        TilePos tilePos = MapD2.toTilePos(key.Id.Substring(3));
                        HouseD2 house = HouseD2.create(values[0]);
                        string id = values[1];
                        addInner(id, tilePos, house, true, structures);
                    }
                    else if (key.Id.StartsWith("ID")) //Building?
                    {
                        HouseD2 house = HouseD2.create(values[0]);
                        string id = values[1];
                        //int health = toHealth(values[2]); //Health field not used.
                        TilePos tilePos = MapD2.toTilePos(values[3]);
                        addInner(id, tilePos, house, false, structures);
                    }
                }
            }
        }

        private static void addInner(string id, TilePos tilePos, HouseD2 house, bool isGen, List<SpriteStructureD2> structures)
        {
            SpriteStructureD2 spr = new SpriteStructureD2(id, tilePos, house, isGen);
            addInner(spr, structures);
        }

        private static void addInner(SpriteStructureD2 spr, List<SpriteStructureD2> structures)
        {
            switch (spr.Id)
            {
                case "Concrete": //Concrete slab.
                    addConcrete(spr, TileSetD2.ConcreteSlab, structures); break;
                case "Concrete4": //Concrete slab (2*2).
                    addConcrete4(spr, structures); break;
                case "Wall": //Wall.
                    addConcrete(spr, TileSetD2.Wall, structures); break;

                case "Turret": //Turret.
                    addDefault(spr, TileSetD2.Turret, structures); break;
                case "R-Turret": //Rocket Turret.
                    addDefault(spr, TileSetD2.RocketTurret, structures); break;

                case "Const Yard": //Construction Yard.
                    addDefault(spr, TileSetD2.ConstructionYard, structures); break;
                case "Windtrap": //Windtrap.
                    addDefault(spr, TileSetD2.Windtrap, structures); break;
                case "Barracks": //Barracks.
                    addDefault(spr, TileSetD2.Barracks, structures); break;
                case "Light Fctry": //Light factory.
                    addDefault(spr, TileSetD2.LightFactory, structures); break;
                case "Heavy Fctry": //Heavy factory.
                    addDefault(spr, TileSetD2.HeavyFactory, structures); break;
                case "Hi-Tech": //High tech factory.
                    addDefault(spr, TileSetD2.HighTechFactory, structures); break;
                case "Refinery": //Refinery.
                    addDefault(spr, TileSetD2.Refinery, structures); break;
                case "Spice Silo": //Spice Silos.
                    addDefault(spr, TileSetD2.SpiceSilos, structures); break;
                case "Outpost": //Outpost (radar).
                    addDefault(spr, TileSetD2.Outpost, structures); break;
                case "Repair": //Repair Facility.
                    addDefault(spr, TileSetD2.RepairFacility, structures); break;
                case "IX": //IX (research centre).
                    addDefault(spr, TileSetD2.IX, structures); break;
                case "WOR": //WOR (trooper training facility).
                    addDefault(spr, TileSetD2.WOR, structures); break;
                case "Palace": //Palace.
                    addDefault(spr, TileSetD2.Palace, structures); break;
                case "Starport": //Starport.
                    addDefault(spr, TileSetD2.Starport, structures); break;

                default:
                    Program.warn(string.Format("Undefined structure sprite id '{0}'!", spr.Id)); break;
            }
        }

        private static void addConcrete(SpriteStructureD2 spr, TileSetD2 tileSet, List<SpriteStructureD2> structures)
        {
            spr.mPriPlane = PriPlaneConcrete;
            addDefault(spr, tileSet, structures);
        }

        private static void addConcrete4(SpriteStructureD2 spr, List<SpriteStructureD2> structures)
        {
            spr.mPriPlane = PriPlaneConcrete;
            //Concrete4 is just one concrete slab repeated four times in a 2*2 tile size.
            int tilesPerFrame = 1;
            int tileSetIndexAdd = 0; //Repeat tile.
            addDefault(spr, TileSetD2.Concrete4Slab, tilesPerFrame, tileSetIndexAdd, structures);
        }

        private static void addDefault(SpriteStructureD2 spr, TileSetD2 tileSet, List<SpriteStructureD2> structures)
        {
            int tilesPerFrame = tileSet.TemplateSize.Width * tileSet.TemplateSize.Height;
            int tileSetIndexAdd = 1; //Iterate tiles.
            addDefault(spr, tileSet, tilesPerFrame, tileSetIndexAdd, structures);
        }

        private static void addDefault(SpriteStructureD2 spr, TileSetD2 tileSet,
            int tilesPerFrame, int tileSetIndexAdd, List<SpriteStructureD2> structures)
        {
            Size sizeInTiles = tileSet.TemplateSize;
            spr.mTileSet = tileSet;
            spr.mSections = new Section[sizeInTiles.Width * sizeInTiles.Height];

            //Frames in structure sets are usually:
            //0=under construction
            //1=destroyed
            //2-X=normal with different animations
            int tileSetIndex = 2 * tilesPerFrame;
            for (int y = 0, i = 0; y < sizeInTiles.Height; y++)
            {
                for (int x = 0; x < sizeInTiles.Width; x++, tileSetIndex += tileSetIndexAdd, i++)
                {
                    spr.mSections[i] = new Section(spr, spr.TilePos.getOffset(x, y), tileSetIndex);
                }
            }
            structures.Add(spr);
        }

        private static void adjustAdjacentWalls(Section section, TileTracker<Section> sectionTracker)
        {
            //A wall's frame index is affected by adjacent walls when added.
            TilePos tilePos = section.TilePos;
            adjustWallSprite(section, sectionTracker);

            //Also adjust adjacent walls around the newly added wall.
            adjustWallSprite(sectionTracker.getAdjacentN(tilePos), sectionTracker);
            adjustWallSprite(sectionTracker.getAdjacentE(tilePos), sectionTracker);
            adjustWallSprite(sectionTracker.getAdjacentS(tilePos), sectionTracker);
            adjustWallSprite(sectionTracker.getAdjacentW(tilePos), sectionTracker);
        }

        private static void adjustWallSprite(Section section, TileTracker<Section> sectionTracker)
        {
            if (hasTileWallType(section))
            {
                TilePos tilePos = section.TilePos;
                int adjWalls = 0;
                //Check for adjacent walls and adjust frame index.
                //Different owner/house doesn't matter. Checked in game.
                if (hasTileWallType(sectionTracker.getAdjacentN(tilePos))) adjWalls += 1;
                if (hasTileWallType(sectionTracker.getAdjacentE(tilePos))) adjWalls += 2;
                if (hasTileWallType(sectionTracker.getAdjacentS(tilePos))) adjWalls += 4;
                if (hasTileWallType(sectionTracker.getAdjacentW(tilePos))) adjWalls += 8;
                section.TileSetIndex = WallTileSetIndices[adjWalls];
            }
        }

        private static bool hasTileWallType(Section sectionInTile)
        {
            return sectionInTile != null && sectionInTile.IsWall;
        }

        //Wall tile set index depending on adjacent walls table.
        private static readonly byte[] WallTileSetIndices = new byte[16]
        {
            1,4,2,3,4,4,5,6,2,7,2,8,9,10,11,12
        };

        public override Rectangle getDrawBox(TheaterD2 theater)
        {
            //Calculate bounding box of all sections.
            Rectangle drawBox = Rectangle.Empty;
            for (int i = 0; i < mSections.Length; i++)
            {
                Section section = mSections[i];
                Rectangle bb = theater.getTileFrame(mTileSet.Id, section.TileSetIndex).getBoundingBox();
                bb.Offset(getDrawPos(section.TilePos)); //Add draw offset.
                if (i == 0)
                {
                    drawBox = bb;
                }
                else
                {
                    drawBox = Rectangle.Union(drawBox, bb);
                }
            }
            return drawBox;
        }

        public override void draw(TheaterD2 theater, IndexedImage image)
        {
            //A transparent pixel (index 0) is a bit weird in structures. It seems like the
            //game doesn't draw it i.e. current pixel is not overwritten, but the current pixel
            //will start doing the Hall-Of-Mirrors (HOM) effect like in Tiberian Dawn and Red Alert.
            //A bit difficult to mimic so let's just draw structures for now i.e. don't write
            //transparent pixels to image.

            //All structures are remapped to owner/house color in the game, even walls and concrete slabs.

            foreach (Section section in mSections)
            {
                if (!section.IsHidden) //Don't draw hidden sections.
                {
                    Frame frame = theater.getTileFrame(mTileSet.Id, section.TileSetIndex);
                    image.draw(frame, getDrawPos(section.TilePos), section.Owner.House.RemapStructure);
                }
            }
        }

        public override void drawRadar(int scale, MapD2 map, IndexedImage image)
        {
            foreach (Section section in mSections)
            {
                if (!section.IsHidden) //Don't draw hidden sections.
                {
                    TilePos tilePos = section.TilePos;
                    Rectangle dstRect = new Rectangle(tilePos.X * scale, tilePos.Y * scale, scale, scale);
                    byte radarIndex = section.Owner.House.RadarIndex;
                    if (section.IsSlab)
                    {
                        //Concrete slabs use index 133 regardless of owner. Checked in game.
                        radarIndex = 133;
                    }
                    image.drawRectFilled(dstRect, radarIndex);
                }
            }
        }
    }
}
