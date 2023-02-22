using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class MapTD : MapTDRA
    {
        public const int WidthInTiles = 64;
        public const int HeightInTiles = 64;
        public const int Width = TileWidth * WidthInTiles;
        public const int Height = TileHeight * HeightInTiles;
        public static readonly Size Size = new Size(Width, Height);
        public static readonly Size SizeInTiles = new Size(WidthInTiles, HeightInTiles);
        public static readonly Rectangle Area = new Rectangle(0, 0, Width, Height);
        public static readonly Rectangle AreaInTiles = new Rectangle(0, 0, WidthInTiles, HeightInTiles);

        private readonly FileBinTileSetTableTD mFileBin; //Null (not found?) to use [TEMPLATE] section instead.

        private MapTD(FileIni fileIni, FileBinTileSetTableTD fileBin, int namesakeCount, TheaterTD theater, Rectangle bordersInTiles)
            : base(fileIni, namesakeCount, bordersInTiles, theater, HouseTD.Neutral)
        {
            mFileBin = fileBin; //Null (not found?) to use [TEMPLATE] section instead.
        }

        public static MapTD create(FileIni fileIni, IFileContainer mapContainer)
        {
            return create(fileIni, findFileBin(fileIni, mapContainer));
        }

        private static MapTD create(FileIni fileIni, FileBinTileSetTableTD fileBin)
        {
            return create(fileIni, fileBin, 1);
        }

        private static MapTD create(FileIni fileIni, FileBinTileSetTableTD fileBin, int namesakeCount)
        {
            IniSection mapSection = fileIni.getSection("MAP");
            TheaterTD theater = TheaterTD.getTheater(mapSection.getKey("Theater").Value);
            Rectangle bordersInTiles = getBordersInTiles(mapSection);
            return new MapTD(fileIni, fileBin, namesakeCount, theater, bordersInTiles);
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
            return HouseTD.create(id);
        }

        public static IniSection findBasicSection(FileIni fileIni)
        {
            //Most maps use [Basic], but a few use [BASIC] in Tiberian Dawn.
            return fileIni.findSection((s) => s.Id == "Basic" || s.Id == "BASIC");
        }

        public static void renderAll()
        {
            renderAll(getMaps());
        }

        public static void renderAll(List<MapTD> maps)
        {
            maps.SortStable((x, y) => string.CompareOrdinal(x.mFileIni.Id, y.mFileIni.Id)); //Sort not needed, but console output looks nicer.
            RenderParams renderParams;
            renderParams.folderPath = Program.RenderOutPath;
            renderParams.folderPathRadar = Program.RenderOutPath + "radar\\";
            renderParams.image = new IndexedImage(Width, Height);
            renderParams.imagesRadar = RadarTD.preCreateImages();
            renderAll(maps, renderParams, renderMap);
        }

        private static void renderMap(MapTD map, RenderParams renderParams)
        {
            map.render(MissionDataTD.getFileName(map.mFileIni), renderParams);
        }

        private static List<MapTD> getMaps()
        {
            List<MapTD> maps = new List<MapTD>();
            List<IFileContainer> mapContainers = getMapContainers();
            foreach (IFileContainer container in mapContainers)
            {
                foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
                {
                    if (fileIni.isMapTDRA() && isMapTypeIncluded(fileIni, GameTD.Config.IncludeMapTypes))
                    {
                        FileBinTileSetTableTD fileBin = findFileBin(fileIni, container);
                        //Check if map's name and content is unique before adding it.
                        List<MapTD> namesakeMaps = maps.FindAll((MapTD m) => m.mFileIni.Id == fileIni.Id);
                        if (namesakeMaps.Count == 0) //Unique name?
                        {
                            maps.Add(create(fileIni, fileBin));
                        }
                        else if (namesakeMaps.Find((MapTD m) => m.mFileIni.isContentEqual(fileIni) &&
                            m.mFileBin.isContentEqual(fileBin)) == null) //Same name, different content?
                        {
                            maps.Add(create(fileIni, fileBin, namesakeMaps.Count + 1));
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
            mapContainers.AddNotNull(GameTD.FileMixGeneral.find());
            mapContainers.AddNotNull(GameTD.FileMixSc000.find());
            mapContainers.AddNotNull(GameTD.FileMixSc001.find());
            //Include game folder also so free-standing maps to be rendered can be placed there.
            mapContainers.Add(new FolderContainer(Program.GamePath));
            return mapContainers;
        }

        private static FileBinTileSetTableTD findFileBin(FileIni fileIni, IFileContainer container)
        {
            //Return BIN-file that corresponds to INI-file in file container. Returns null if not found.
            string fileBinName = fileIni.Id + ".BIN";
            FileProto file = container.findFile(fileBinName);
            if (file != null) //BIN-file found?
            {
                return new FileBinTileSetTableTD(file);
            }
            return null; //BIN-file not found.
        }

        public static void debugSaveRenderAll() //For quick testing of map rendering.
        {
            List<MapTD> maps = new List<MapTD>();
            IFileContainer container = GameTD.FileMixGeneral.get();
            //IFileContainer container = new FolderContainer(Program.GamePath);
            foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
            {
                if (!fileIni.isMapTDRA()) continue;

                //if (fileIni.Id.StartsWith("SCB"))
                //if (fileIni.Id.StartsWith("SCG"))
                //if (fileIni.Id.StartsWith("SCB1"))
                //if (fileIni.Id.StartsWith("SCG1"))
                //if (fileIni.Id.StartsWith("SCG1") || fileIni.Id.StartsWith("SCB1"))
                //if (!fileIni.Id.StartsWith("SCM"))
                //if (fileIni.Id.StartsWith("SCM"))
                //if (fileIni.Id.StartsWith("SCJ"))
                {
                    maps.Add(create(fileIni, container));
                }
            }
            debugSaveRenderAll(maps);
        }

        public static void debugSaveRenderAll(List<MapTD> maps)
        {
            maps.SortStable((x, y) => string.CompareOrdinal(x.mFileIni.Id, y.mFileIni.Id)); //Sort not needed, but console output looks nicer.
            RenderParams renderParams;
            renderParams.folderPath = Program.DebugOutPath + "maps\\";
            renderParams.folderPathRadar = Program.DebugOutPath + "radar\\";
            renderParams.image = new IndexedImage(Width, Height);
            renderParams.imagesRadar = RadarTD.preCreateImages();
            renderAll(maps, renderParams, debugSaveRenderMap);
        }

        private static void debugSaveRenderMap(MapTD map, RenderParams renderParams)
        {
            map.render(map.FileIni.Id.ToLowerInvariant(), renderParams);
        }

        private void render(string fileName, RenderParams renderParams)
        {
            IniSection basicSection = findBasicSection(mFileIni);
            if (basicSection != null)
            {
                IniKey playerKey = basicSection.findKey("Player");
                if (playerKey != null)
                {
                    mPlayer = HouseTD.create(playerKey.Value);
                }
            }

            GroundLayerTD groundLayer;
            List<SpriteTDRA> sprites;
            load(out groundLayer, out sprites);

            fileName = fileName + mNamesakeCount;
            saveMapImage(groundLayer, sprites, renderParams.folderPath, fileName, renderParams.image);
            RadarTD.saveRadarImage(this, groundLayer, sprites, renderParams.folderPathRadar, fileName, renderParams.imagesRadar);
        }

        private void saveMapImage(GroundLayerTD groundLayer, List<SpriteTDRA> sprites, string folderPath, string fileName, IndexedImage image)
        {
            Rectangle borders = toInPixels(mBordersInTiles);
            Rectangle trimRect = getTrimRect(borders, GameTD.Config.OutsideMapBorders, Area); //Used later when saving image.
            image.Clip = trimRect; //Store visible area info. Can be used to optimize some drawing.

            groundLayer.draw(mTheater, image); //Draw tiled background layer.
            foreach (SpriteTDRA sprite in sprites) //Draw sprites over background layer.
            {
                sprite.draw(mTheater, image);
            }

            if (GameTD.Config.ShadeOutsideMapBorders) //Shade area outside borders?
            {
                image.drawOutsideShade(borders, mTheater.getOutsideRemap());
            }

            mTheater.MapInfoDrawer.drawExtra(mFileIni, sprites, SizeInTiles, image,
                GameTD.Config.DrawWaypoints, GameTD.Config.DrawCellTriggers, GameTD.Config.HighlightCrates,
                GameTD.Config.DrawBaseNumbers, GameTD.Config.DrawSpriteActions, GameTD.Config.DrawSpriteTriggers);

            System.Diagnostics.Debug.Assert(image.Width == Width && image.Height == Height, "Image should be the size of a full map!");
            saveImage(trimRect, mTheater.GamePalette, mTheater.MapInfoDrawer, GameTD.Config.DrawMapHeader,
                Path.Combine(folderPath, fileName + ".png"), image);
        }

        private void load(out GroundLayerTD groundLayer, out List<SpriteTDRA> sprites)
        {
            //In the source code there is deactivated support for a [AIRCRAFT] section with the same format
            //as [UNITS] (but no action and trigger fields?). This section isn't present in any INI-file either.

            List<SpriteSmudgeTD> smudges = new List<SpriteSmudgeTD>();
            List<SpriteOverlayTD> overlays = new List<SpriteOverlayTD>();
            List<SpriteFlagTD> flags = new List<SpriteFlagTD>();
            List<SpriteTerrainTD> terrains = new List<SpriteTerrainTD>();
            List<SpriteStructureTD> structures = new List<SpriteStructureTD>();
            List<SpriteUnitTD> units = new List<SpriteUnitTD>();
            List<SpriteInfantryTD> infantries = new List<SpriteInfantryTD>();

            //Add sprites from current map.
            SpriteSmudgeTD.add(this, smudges);
            SpriteOverlayTD.add(this, overlays);
            SpriteFlagTD.add(this, flags);
            SpriteTerrainTD.add(this, terrains);
            SpriteStructureTD.add(this, structures);
            SpriteUnitTD.add(this, units);
            SpriteInfantryTD.add(this, infantries);

            //Create ground layer.
            groundLayer = GroundLayerTD.create(mFileBin, mFileIni, mTheater);

            //Compile final list of all sprites and sort them in draw order.
            sprites = new List<SpriteTDRA>();
            SpriteSmudgeTD.endAdd(smudges, structures, this, sprites); //Also adds bib to structures.
            SpriteOverlayTD.endAdd(overlays, structures, this, sprites);
            SpriteFlagTD.endAdd(flags, sprites);
            SpriteTerrainTD.endAdd(terrains, sprites);
            SpriteStructureTD.endAdd(structures, this, sprites);
            SpriteUnitTD.endAdd(units, structures, this, sprites); //Also adds aircraft to helipads if configured so.
            SpriteInfantryTD.endAdd(infantries, sprites);
            sprites.SortStable(SpriteTDRA.compareDrawOrderTD); //Sort draw order.
        }
    }
}
