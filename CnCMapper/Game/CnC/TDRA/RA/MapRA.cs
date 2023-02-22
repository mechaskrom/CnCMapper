using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    partial class MapRA : MapTDRA
    {
        public const int WidthInTiles = 128;
        public const int HeightInTiles = 128;
        public const int Width = TileWidth * WidthInTiles;
        public const int Height = TileHeight * HeightInTiles;
        public static readonly Size Size = new Size(Width, Height);
        public static readonly Size SizeInTiles = new Size(WidthInTiles, HeightInTiles);
        public static readonly Rectangle Area = new Rectangle(0, 0, Width, Height);
        public static readonly Rectangle AreaInTiles = new Rectangle(0, 0, WidthInTiles, HeightInTiles);

        private static MapRA mMapCarryOver = null; //Last map set to carry over units and structures from.

        private RulesGeneralRA mRulesGeneral = null;
        private readonly Dictionary<string, RulesObjectRA> mRulesObject = new Dictionary<string, RulesObjectRA>();
        private readonly Dictionary<string, RulesBuildingRA> mRulesBuilding = new Dictionary<string, RulesBuildingRA>();
        private MapRA mMapInherit = null; //Map to inherit from if this map is set to do so.

        private MapRA(FileIni fileIni, int namesakeCount, TheaterRA theater, Rectangle bordersInTiles)
            : base(fileIni, namesakeCount, bordersInTiles, theater, HouseRA.Neutral)
        {
        }

        public static MapRA create(FileIni fileIni)
        {
            return create(fileIni, 1);
        }

        private static MapRA create(FileIni fileIni, int namesakeCount)
        {
            IniSection mapSection = fileIni.getSection("Map");
            TheaterRA theater = TheaterRA.getTheater(mapSection.getKey("Theater").Value);
            Rectangle bordersInTiles = getBordersInTiles(mapSection);
            return new MapRA(fileIni, namesakeCount, theater, bordersInTiles);
        }

        public static TilePos toTilePos(string tileNumber)
        {
            return toTilePos(tileNumber, WidthInTiles);
        }

        public static TilePos toTilePos(int tileNumber)
        {
            return toTilePos(tileNumber, WidthInTiles);
        }

        public static int toTileNum(TilePos tilePos)
        {
            return toTileNum(tilePos, WidthInTiles);
        }

        protected override HouseTDRA createHouse(string id)
        {
            return HouseRA.create(id);
        }

        public RulesGeneralRA getRulesGeneral()
        {
            if (mRulesGeneral == null)
            {
                mRulesGeneral = new RulesGeneralRA(mFileIni);
            }
            return mRulesGeneral;
        }

        public RulesObjectRA getRulesObject(string objectId)
        {
            RulesObjectRA rulesObject;
            if (!mRulesObject.TryGetValue(objectId, out rulesObject))
            {
                rulesObject = new RulesObjectRA(objectId, mFileIni);
                mRulesObject.Add(objectId, rulesObject);
            }
            return rulesObject;
        }

        public RulesBuildingRA getRulesBuilding(string buildingId)
        {
            RulesBuildingRA rulesBuilding;
            if (!mRulesBuilding.TryGetValue(buildingId, out rulesBuilding))
            {
                rulesBuilding = new RulesBuildingRA(buildingId, mFileIni);
                mRulesBuilding.Add(buildingId, rulesBuilding);
            }
            return rulesBuilding;
        }

        public FileShpSpriteSetTDRA getFileShpFromRule(string objectId)
        {
            return getFileShpFromRule(getRulesObject(objectId));
        }

        public FileShpSpriteSetTDRA getFileShpFromRule(RulesObjectRA rulesObject)
        {
            return mTheater.getSpriteSet(rulesObject.getFileShpOrObjectId());

            //Only structure and infantry sprites currently uses the image rule. Normally it's used
            //for fake buildings and civilians and what's currently only implemented/supported.
            //Seems to work for other sprites too (I tried ships and units). Draw offsets and
            //other stuff can get weird though. Seems like a map specific image rule doesn't work,
            //it must be in the global "rules.ini" file. Or maybe a "rules.ini" in the game
            //directory just overrides everything?
            //TODO: Add better and more support for the image rule? Normally not needed though?
        }

        public bool isInvisibleEnemyByRule(RulesObjectRA rulesObject, HouseRA house)
        {
            //Invisible enemy sprite? Doesn't matter if enemy is an ally. Checked in game.
            return !GameRA.Config.ShowInvisibleEnemies && (house != Player && rulesObject.getIsInvisible());

            //Only structure sprites currently uses the invisible rule. Normally only used by mines.
            //TODO: Use the invisible rule for more types (also applies to infantry, ship, unit)? Normally not needed though?
        }

        public static void renderAll()
        {
            renderAll(getMaps());
        }

        public static void renderAll(List<MapRA> maps)
        {
            maps.SortStable((x, y) => string.CompareOrdinal(x.mFileIni.Id, y.mFileIni.Id)); //Sort needed to make inherit map system work.
            RenderParams renderParams;
            renderParams.folderPath = Program.RenderOutPath;
            renderParams.folderPathRadar = Program.RenderOutPath + "radar\\";
            renderParams.image = new IndexedImage(Width, Height);
            renderParams.imagesRadar = RadarRA.preCreateImages();
            renderAll(maps, renderParams, renderMap);
        }

        private static void renderMap(MapRA map, RenderParams renderParams)
        {
            map.render(MissionDataRA.getFileName(map.mFileIni), renderParams);
        }

        private static List<MapRA> getMaps()
        {
            List<MapRA> maps = new List<MapRA>();
            List<IFileContainer> mapContainers = getMapContainers();
            foreach (IFileContainer container in mapContainers)
            {
                foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
                {
                    if (fileIni.isMapTDRA() && isMapTypeIncluded(fileIni, GameRA.Config.IncludeMapTypes))
                    {
                        //Check if map's name and content is unique before adding it.
                        List<MapRA> namesakeMaps = maps.FindAll((MapRA m) => m.mFileIni.Id == fileIni.Id);
                        if (namesakeMaps.Count == 0) //Unique name?
                        {
                            maps.Add(create(fileIni));
                        }
                        else if (namesakeMaps.Find((MapRA m) => m.mFileIni.isContentEqual(fileIni)) == null) //Same name, different content?
                        {
                            maps.Add(create(fileIni, namesakeMaps.Count + 1));
                        }
                    }
                }
            }
            return maps;
        }

        private static List<IFileContainer> getMapContainers()
        {
            //Return MIX-files known to contain maps.
            List<IFileContainer> mapContainers = new List<IFileContainer>();
            mapContainers.AddNotNull(GameRA.FileMixMainGeneral.find());
            //Include game folder also so free-standing maps to be rendered can be placed there.
            mapContainers.Add(new FolderContainer(Program.GamePath));
            return mapContainers;
        }

        public static void debugSaveRenderAll() //For quick testing of map rendering.
        {
            List<MapRA> maps = new List<MapRA>();
            IFileContainer container = GameRA.FileMixMainGeneral.get();
            //IFileContainer container = new FolderContainer(Program.GamePath);
            foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
            {
                if (!fileIni.isMapTDRA()) continue;

                //if (fileIni.Id.StartsWith("SCG"))
                //if (fileIni.Id.StartsWith("SCU"))
                //if (fileIni.Id.StartsWith("SCG1"))
                //if (fileIni.Id.StartsWith("SCU1"))
                //if (fileIni.Id.StartsWith("SCG1") || fileIni.Id.StartsWith("SCU1"))
                //if (!fileIni.Id.StartsWith("SCM"))
                //if (fileIni.Id.StartsWith("SCM"))
                //if (fileIni.Id.StartsWith("SCA"))
                {
                    maps.Add(create(fileIni));
                }
            }
            debugSaveRenderAll(maps);
        }

        public static void debugSaveRenderAll(List<MapRA> maps)
        {
            maps.SortStable((x, y) => string.CompareOrdinal(x.FileIni.Id, y.FileIni.Id)); //Sort needed to make inherit map system work.
            RenderParams renderParams;
            renderParams.folderPath = Program.DebugOutPath + "maps\\";
            renderParams.folderPathRadar = Program.DebugOutPath + "radar\\";
            renderParams.image = new IndexedImage(Width, Height);
            renderParams.imagesRadar = RadarRA.preCreateImages();
            renderAll(maps, renderParams, debugSaveRenderMap);
        }

        private static void debugSaveRenderMap(MapRA map, RenderParams renderParams)
        {
            map.render(map.FileIni.Id.ToLowerInvariant(), renderParams);
        }

        private void render(string fileName, RenderParams renderParams)
        {
            IniSection basicSection = mFileIni.findSection("Basic");
            if (basicSection != null)
            {
                IniKey playerKey = basicSection.findKey("Player");
                if (playerKey != null)
                {
                    mPlayer = HouseRA.create(playerKey.Value);
                }

                //If "ToCarryOver" is set for a map then all its units and structures at the end will
                //appear in the next map that has "ToInherit" set. Obviously hard to do when rendering
                //maps, but let's fake it by inheriting all units and structures at the start instead.
                //The only official maps that do this are:
                //-"SCA01EA" has "ToCarryOver" set and "SCA02EA" has "ToInherit" set.
                //-"SCG02EA" has "ToCarryOver" set and "SCG04EA" has "ToInherit" set.
                IniKey toInheritKey = basicSection.findKey("ToInherit");
                if (toInheritKey != null && toInheritKey.valueAsBool()) //Inherit carry over map?
                {
                    if (mMapCarryOver != null && MissionDataRA.isLaterCampaignMission(mFileIni, mMapCarryOver.mFileIni))
                    {
                        mMapInherit = mMapCarryOver;
                    }
                    mMapCarryOver = null;
                }
                IniKey toCarryOverKey = basicSection.findKey("ToCarryOver");
                if (toCarryOverKey != null && toCarryOverKey.valueAsBool()) //Should this map carry over?
                {
                    mMapCarryOver = this;
                }
            }

            GroundLayerRA groundLayer;
            List<SpriteTDRA> sprites;
            load(out groundLayer, out sprites);

            fileName = fileName + mNamesakeCount;
            saveMapImage(groundLayer, sprites, renderParams.folderPath, fileName, renderParams.image);
            RadarRA.saveRadarImage(this, groundLayer, sprites, renderParams.folderPathRadar, fileName, renderParams.imagesRadar);
        }

        private void saveMapImage(GroundLayerRA groundLayer, List<SpriteTDRA> sprites, string folderPath, string fileName, IndexedImage image)
        {
            Rectangle borders = toInPixels(mBordersInTiles);
            Rectangle trimRect = getTrimRect(borders, GameRA.Config.OutsideMapBorders, Area); //Used later when saving image.
            image.Clip = trimRect; //Store visible area info. Can be used to optimize some drawing.

            groundLayer.draw(mTheater, image); //Draw tiled background layer.
            foreach (SpriteTDRA sprite in sprites) //Draw sprites over background layer.
            {
                sprite.draw(mTheater, image);
            }

            if (GameRA.Config.ShadeOutsideMapBorders) //Shade area outside borders?
            {
                image.drawOutsideShade(borders, mTheater.getOutsideRemap());
            }

            mTheater.MapInfoDrawer.drawExtra(mFileIni, sprites, SizeInTiles, image,
                GameRA.Config.DrawWaypoints, GameRA.Config.DrawCellTriggers, GameRA.Config.HighlightCrates,
                GameRA.Config.DrawBaseNumbers, GameRA.Config.DrawSpriteActions, GameRA.Config.DrawSpriteTriggers);

            System.Diagnostics.Debug.Assert(image.Width == Width && image.Height == Height, "Image should be the size of a full map!");
            saveImage(trimRect, mTheater.GamePalette, mTheater.MapInfoDrawer, GameRA.Config.DrawMapHeader,
                Path.Combine(folderPath, fileName + ".png"), image);
        }

        private void load(out GroundLayerRA groundLayer, out List<SpriteTDRA> sprites)
        {
            //In the source code there is deactivated support for a [AIRCRAFT] section with the same format
            //as [UNITS] (but no action and trigger fields?). This section isn't present in any INI-file either.
            //Remnant from Tiberian Dawn?

            List<SpriteSmudgeRA> smudges = new List<SpriteSmudgeRA>();
            List<SpriteOverlayRA> overlays = new List<SpriteOverlayRA>();
            List<SpriteFlagRA> flags = new List<SpriteFlagRA>();
            List<SpriteTerrainRA> terrains = new List<SpriteTerrainRA>();
            List<SpriteStructureRA> structures = new List<SpriteStructureRA>();
            List<SpriteUnitRA> units = new List<SpriteUnitRA>();
            List<SpriteInfantryRA> infantries = new List<SpriteInfantryRA>();
            List<SpriteShipRA> ships = new List<SpriteShipRA>();

            if (mMapInherit != null) //Inherit sprites from a previous map?
            {
                //Use inherited map's INI-file to add sprites to this map.
                //Seems like only structures, units, infantries and ships are inherited.
                SpriteStructureRA.addNormal(this, mMapInherit.FileIni, structures); //Normal structures only, not base.
                SpriteUnitRA.add(this, mMapInherit.FileIni, units);
                SpriteInfantryRA.add(this, mMapInherit.FileIni, infantries);
                SpriteShipRA.add(this, mMapInherit.FileIni, ships);
            }

            //Add sprites from current map.
            SpriteSmudgeRA.add(this, smudges);
            SpriteOverlayRA.add(this, overlays);
            SpriteFlagRA.add(this, flags);
            SpriteTerrainRA.add(this, terrains);
            SpriteStructureRA.add(this, structures);
            SpriteUnitRA.add(this, units);
            SpriteInfantryRA.add(this, infantries);
            SpriteShipRA.add(this, ships);

            //Create ground layer.
            groundLayer = GroundLayerRA.create(mFileIni);

            //Compile final list of all sprites and sort them in draw order.
            sprites = new List<SpriteTDRA>();
            SpriteSmudgeRA.endAdd(smudges, structures, this, sprites); //Also adds bib to structures.
            SpriteOverlayRA.endAdd(overlays, sprites);
            SpriteFlagRA.endAdd(flags, sprites);
            SpriteTerrainRA.endAdd(terrains, sprites);
            SpriteStructureRA.endAdd(structures, this, sprites);
            SpriteUnitRA.endAdd(units, structures, this, sprites); //Also adds aircraft to helipads if configured so.
            SpriteInfantryRA.endAdd(infantries, sprites);
            SpriteShipRA.endAdd(ships, sprites);
            sprites.SortStable(SpriteTDRA.compareDrawOrderRA); //Sort draw order.
        }
    }
}
