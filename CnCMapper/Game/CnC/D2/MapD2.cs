using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    class MapD2 : MapCnC
    {
        public const int TileWidth = 16; //Pixels.
        public const int TileHeight = 16;

        public const int WidthInTiles = 64;
        public const int HeightInTiles = 64;
        public const int Width = TileWidth * WidthInTiles;
        public const int Height = TileHeight * HeightInTiles;
        public static readonly Size Size = new Size(Width, Height);
        public static readonly Size SizeInTiles = new Size(WidthInTiles, HeightInTiles);
        public static readonly Rectangle Area = new Rectangle(0, 0, Width, Height);
        public static readonly Rectangle AreaInTiles = new Rectangle(0, 0, WidthInTiles, HeightInTiles);

        private readonly TheaterD2 mTheater;

        private MapD2(FileIni fileIni, int namesakeCount, Rectangle bordersInTiles)
            : base(fileIni, namesakeCount, bordersInTiles)
        {
            mTheater = TheaterD2.Desert;
        }

        public static MapD2 create(FileIni fileIni)
        {
            return create(fileIni, 1);
        }

        private static MapD2 create(FileIni fileIni, int namesakeCount)
        {
            IniSection basicSection = fileIni.getSection("BASIC");
            Rectangle bordersInTiles = getBordersInTiles(basicSection);
            return new MapD2(fileIni, namesakeCount, bordersInTiles);
        }

        public TheaterD2 Theater
        {
            get { return mTheater; }
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

        private static Rectangle getBordersInTiles(IniSection basicSection)
        {
            //Tile position of NW and SE corners with different map scales. Checked in game.
            //0: NW=1,1     SE=62,62    Size=62*62
            //1: NW=16,16   SE=47,47    Size=32*32
            //2: NW=21,21   SE=41,41    Size=21*21
            int mapScale = basicSection.getKey("MapScale").valueAsInt32();
            switch (mapScale)
            {
                case 0: return new Rectangle(1, 1, 62, 62);
                case 1: return new Rectangle(16, 16, 32, 32);
                case 2: return new Rectangle(21, 21, 21, 21);
                default:
                    Program.warn(string.Format("'{0}' has undefined map scale '{1}'!",
                        basicSection.ParentFile.FullName, mapScale));
                    return new Rectangle(1, 1, 62, 62); //Return 1 tile from edge border.
            }
        }

        private static Rectangle toInPixels(Rectangle rectInTiles)
        {
            return toInPixels(rectInTiles, TileWidth, TileHeight);
        }

        public static void renderAll()
        {
            renderAll(getMaps());
        }

        public static void renderAll(List<MapD2> maps)
        {
            maps.SortStable((x, y) => string.CompareOrdinal(x.mFileIni.Id, y.mFileIni.Id)); //Sort not needed, but console output looks nicer.
            RenderParams renderParams;
            renderParams.folderPath = Program.RenderOutPath;
            renderParams.folderPathRadar = Program.RenderOutPath + "radar\\";
            renderParams.image = new IndexedImage(Width, Height);
            renderParams.imagesRadar = RadarD2.preCreateImages();
            renderAll(maps, renderParams, renderMap);
        }

        private static void renderMap(MapD2 map, RenderParams renderParams)
        {
            map.render(MissionDataD2.getFileName(map.mFileIni), renderParams);
        }

        private static List<MapD2> getMaps()
        {
            List<MapD2> maps = new List<MapD2>();
            List<IFileContainer> mapContainers = getMapContainers();
            foreach (IFileContainer container in mapContainers)
            {
                foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
                {
                    if (fileIni.isMapD2())
                    {
                        //Check if map's name and content is unique before adding it.
                        List<MapD2> namesakeMaps = maps.FindAll((MapD2 m) => m.mFileIni.Id == fileIni.Id);
                        if (namesakeMaps.Count == 0) //Unique name?
                        {
                            maps.Add(create(fileIni));
                        }
                        else if (namesakeMaps.Find((MapD2 m) => m.mFileIni.isContentEqual(fileIni)) == null) //Same name, different content?
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
            //Return PAK-files known to contain maps.
            List<IFileContainer> mapContainers = new List<IFileContainer>();
            mapContainers.AddNotNull(GameD2.FilePakScenario.find());
            //Include game folder also so free-standing maps to be rendered can be placed there.
            mapContainers.Add(new FolderContainer(Program.GamePath));
            return mapContainers;
        }

        public static void debugSaveRenderAll() //For quick testing of map rendering.
        {
            List<MapD2> maps = new List<MapD2>();
            IFileContainer container = GameD2.FilePakScenario.get();
            //IFileContainer container = new FolderContainer(Program.GamePath);
            foreach (FileIni fileIni in container.tryFilesAs<FileIni>())
            {
                if (!fileIni.isMapD2()) continue;

                //if (fileIni.Id.StartsWith("SCENA"))
                {
                    maps.Add(create(fileIni));
                }
            }
            debugSaveRenderAll(maps);
        }

        public static void debugSaveRenderAll(List<MapD2> maps)
        {
            maps.SortStable((x, y) => string.CompareOrdinal(x.mFileIni.Id, y.mFileIni.Id)); //Sort not needed, but console output looks nicer.
            RenderParams renderParams;
            renderParams.folderPath = Program.DebugOutPath + "maps\\";
            renderParams.folderPathRadar = Program.DebugOutPath + "radar\\";
            renderParams.image = new IndexedImage(Width, Height);
            renderParams.imagesRadar = RadarD2.preCreateImages();
            renderAll(maps, renderParams, debugSaveRenderMap);
        }

        private static void debugSaveRenderMap(MapD2 map, RenderParams renderParams)
        {
            map.render(map.FileIni.Id.ToLowerInvariant(), renderParams);
        }

        private void render(string fileName, RenderParams renderParams)
        {
            GroundLayerD2 groundLayer;
            List<SpriteD2> sprites;
            load(out groundLayer, out sprites);

            fileName = fileName + mNamesakeCount;
            saveMapImage(groundLayer, sprites, renderParams.folderPath, fileName, renderParams.image);
            RadarD2.saveRadarImage(this, groundLayer, sprites, renderParams.folderPathRadar, fileName, renderParams.imagesRadar);
        }

        private void saveMapImage(GroundLayerD2 groundLayer, List<SpriteD2> sprites, string folderPath, string fileName, IndexedImage image)
        {
            Rectangle borders = toInPixels(mBordersInTiles);
            Rectangle trimRect = getTrimRect(borders, GameD2.Config.OutsideMapBorders, Area); //Used later when saving image.
            image.Clip = trimRect; //Store visible area info. Can be used to optimize some drawing.

            groundLayer.draw(image); //Draw tiled ground layer.
            foreach (SpriteD2 sprite in sprites) //Draw sprites over ground layer.
            {
                sprite.draw(mTheater, image);
            }

            if (GameD2.Config.ShadeOutsideMapBorders) //Shade area outside borders?
            {
                image.drawOutsideShade(borders, mTheater.getOutsideRemap());
            }

            mTheater.MapInfoDrawer.drawExtra(mFileIni, sprites, mTheater, image,
                GameD2.Config.DrawSpriteActions);

            System.Diagnostics.Debug.Assert(image.Width == Width && image.Height == Height, "Image should be the size of a full map!");
            saveImage(trimRect, mTheater.GamePalette, mTheater.MapInfoDrawer, GameD2.Config.DrawMapHeader,
                Path.Combine(folderPath, fileName + ".png"), image);
        }

        private void load(out GroundLayerD2 groundLayer, out List<SpriteD2> sprites)
        {
            List<SpriteStructureD2> structures = new List<SpriteStructureD2>();
            List<SpriteUnitD2> units = new List<SpriteUnitD2>();

            //Add sprites from current map.
            SpriteStructureD2.add(this, structures);
            SpriteUnitD2.add(this, units);

            //Finalize add sprites.
            SpriteStructureD2.endAdd(this, structures);
            SpriteUnitD2.endAdd(this, units);

            //Create ground layer.
            groundLayer = GroundLayerD2.create(this, structures);

            //Compile list of all sprites and sort them in draw order.
            sprites = new List<SpriteD2>();
            sprites.AddDerivedRange(structures);
            sprites.AddDerivedRange(units);
            sprites.SortStable(SpriteD2.compareDrawOrder);
        }
    }

    static class FileIniExt
    {
        public static bool isMapD2(this FileIni fileIni) //Determine if this INI-file may be a map.
        {
            //Dune 2 maps should at least have a [MAP] section.
            return fileIni.findSection("MAP") != null;
        }
    }
}
