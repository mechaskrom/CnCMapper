using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    //Deals with extra info text in map like title, base structure numbers, waypoints, cell triggers, etc.
    abstract class MapInfoDrawerTDRA : MapInfoDrawerCnC
    {
#if DRAW_CREATED_INFO
        private const string CreatedInfo = "Compiled 20210808 by mechaskrom@gmail.com";
#endif
        private static readonly Color IdealColorFgWaypoint = Color.FromArgb(240, 240, 80); //Yellow.
        private static readonly Color IdealColorBgWaypoint = Color.Black;
        private static readonly Color IdealColorFgCellTrigger = Color.FromArgb(240, 240, 80); //Yellow.
        private static readonly Color IdealColorBgCellTrigger = Color.Black;
        private static readonly Color IdealColorFgBaseNumber = Color.FromArgb(80, 240, 240); //Cyan.
        private static readonly Color IdealColorBgBaseNumber = Color.Black;
        private static readonly Color IdealColorFgSprTrigger = Color.FromArgb(80, 240, 80); //Green.
        private static readonly Color IdealColorBgSprTrigger = Color.Black;

        //Size and color of the frame used to highlight tiles.
        private static readonly Color IdealColorTileFrame = Color.FromArgb(160, 0, 160); //Dark magenta.
        //private static readonly Color IdealColorTileFrame = Color.FromArgb(0, 160, 160); //Dark cyan. Also good, but a bit invisible on water and snow?
        private const int CornerLengthTileFrame = 5; //In pixels.
        private byte mPaletteIndexTileFrame = 0; //Init, set actual value later.

        //Size and color of the frame used to highlight crates.
        private static readonly Color IdealColorCrateFrame = Color.FromArgb(80, 240, 80); //Green.
        private const int CornerLengthCrateFrame = 5; //In pixels.
        private byte mPaletteIndexCrateFrame = 0; //Init, set actual value later.

        private TextDrawer mTextDrawerWaypoint = null;
        private TextDrawer mTextDrawerCellTrigger = null;
        private TextDrawer mTextDrawerBaseNumber = null;
        private TextDrawer mTextDrawerSprTrigger = null;
        private Frame mTileSetEmptyTile = null; //Missing tile set graphics (Hall-Of-Mirrors (HOM) effect in map tile layer).

        protected MapInfoDrawerTDRA(Palette6Bit palette) //Should use a game palette i.e. after any settings adjustment.
            : base(palette)
        {
        }

        private byte getPaletteIndexTileFrame()
        {
            if (mPaletteIndexTileFrame == 0)
            {
                mPaletteIndexTileFrame = findClosestPaletteIndex(IdealColorTileFrame);
            }
            return mPaletteIndexTileFrame;
        }

        private byte getPaletteIndexCrateFrame()
        {
            if (mPaletteIndexCrateFrame == 0)
            {
                mPaletteIndexCrateFrame = findClosestPaletteIndex(IdealColorCrateFrame);
            }
            return mPaletteIndexCrateFrame;
        }

        private TextDrawer getTextDrawerWaypoint()
        {
            if (mTextDrawerWaypoint == null)
            {
                mTextDrawerWaypoint = createTextDrawer(getFileFnt6x10(), IdealColorFgWaypoint, IdealColorBgWaypoint);
            }
            return mTextDrawerWaypoint;
        }

        private TextDrawer getTextDrawerCellTrigger()
        {
            if (mTextDrawerCellTrigger == null)
            {
                mTextDrawerCellTrigger = createTextDrawer(getFileFnt6x10(), IdealColorFgCellTrigger, IdealColorBgCellTrigger);
            }
            return mTextDrawerCellTrigger;
        }

        private TextDrawer getTextDrawerBaseNumber()
        {
            if (mTextDrawerBaseNumber == null)
            {
                mTextDrawerBaseNumber = createTextDrawer(getFileFnt6x10(), IdealColorFgBaseNumber, IdealColorBgBaseNumber);
            }
            return mTextDrawerBaseNumber;
        }

        private TextDrawer getTextDrawerSprTrigger()
        {
            if (mTextDrawerSprTrigger == null)
            {
                mTextDrawerSprTrigger = createTextDrawer(getFileFnt6x10(), IdealColorFgSprTrigger, IdealColorBgSprTrigger);
            }
            return mTextDrawerSprTrigger;
        }

        public Frame getTileSetEmptyTile(bool drawTileSetEmptyEffect)
        {
            if (mTileSetEmptyTile == null)
            {
                //Create a solid black frame.
                Frame frame = new Frame(MapTDRA.TileWidth, MapTDRA.TileHeight);
                frame.clear(findClosestPaletteIndex(Color.Black));

                if (drawTileSetEmptyEffect)
                {
                    //Draw a "hall/corridor" effect with increasingly larger white rectangles.
                    Rectangle dstRect = new Rectangle(0, 0, MapTDRA.TileWidth, MapTDRA.TileHeight);
                    byte dstRectValue = findClosestPaletteIndex(Color.White);
                    for (int i = 3; dstRect.Y < frame.Height || dstRect.X < frame.Width; dstRect.Y += i, dstRect.X += i, i++)
                    {
                        frame.drawRect(dstRect, dstRectValue);
                    }
                }
                mTileSetEmptyTile = frame;
            }
            return mTileSetEmptyTile;
        }

        public void drawExtra(FileIni fileIni, List<SpriteTDRA> sprites, Size mapSizeInTiles, IndexedImage image,
            bool doWaypoints, bool doCellTriggers, bool doHighlightCrates, bool doBaseNumbers, bool doSprActions, bool doSprTriggers)
        {
            if (doWaypoints || doCellTriggers)
            {
                drawWaypointsAndCellTriggers(fileIni, mapSizeInTiles, image, doWaypoints, doCellTriggers);
            }

            if (doHighlightCrates)
            {
                highlightCrates(sprites, image);
            }

            if (doBaseNumbers) //Will also include repair field in INI-key if Red Alert.
            {
                drawBaseNumbers(sprites, getTextDrawerBaseNumber(), image);
            }

            if (doSprActions || doSprTriggers)
            {
                drawSprActionsAndTriggers(sprites, image, doSprActions, doSprTriggers);
            }
        }

        private void drawWaypointsAndCellTriggers(FileIni fileIni, Size mapSizeInTiles, IndexedImage image,
            bool doWaypoints, bool doCellTriggers)
        {
            TileTracker<TileInfo> infoTracker = new TileTracker<TileInfo>(mapSizeInTiles);
            if (doWaypoints)
            {
                addWaypoints(fileIni, infoTracker);
            }
            if (doCellTriggers)
            {
                addCellTriggers(fileIni, infoTracker);
            }

            IEnumerable<TileInfo> tileInfos = infoTracker.getTileItems();
            //Draw all tile frames first so text is drawn over them afterwards.
            byte tfPaletteIndex = getPaletteIndexTileFrame();
            Rectangle tfRect = new Rectangle(0, 0, MapTDRA.TileWidth, MapTDRA.TileHeight);
            foreach (TileInfo tileInfo in tileInfos)
            {
                TilePos tilePos = tileInfo.TilePos;
                tfRect.X = tilePos.X * MapTDRA.TileWidth;
                tfRect.Y = tilePos.Y * MapTDRA.TileHeight;
                image.drawRectCorners(tfRect, CornerLengthTileFrame, tfPaletteIndex,
                    infoTracker.getAdjacentN(tilePos) == null, //Draw sides with no adjacent tile info.
                    infoTracker.getAdjacentS(tilePos) == null,
                    infoTracker.getAdjacentW(tilePos) == null,
                    infoTracker.getAdjacentE(tilePos) == null);
            }
            //Draw tile info texts.
            TextDrawer tdWaypoint = getTextDrawerWaypoint();
            TextDrawer tdCellTrigger = getTextDrawerCellTrigger();
            foreach (TileInfo tileInfo in tileInfos)
            {
                tileInfo.draw(tdWaypoint, tdCellTrigger, image);
            }
        }

        private void highlightCrates(List<SpriteTDRA> sprites, IndexedImage image)
        {
            byte cfPaletteIndex = getPaletteIndexCrateFrame();
            foreach (SpriteTDRA spr in sprites)
            {
                if (spr.Id == "SCRATE" || spr.Id == "WCRATE")
                {
                    Rectangle drawBox = spr.getDrawBox();
                    drawBox.Inflate(3, 3);
                    image.drawRectCorners(drawBox, CornerLengthCrateFrame, cfPaletteIndex);
                }
            }
        }

        protected abstract void drawBaseNumbers(List<SpriteTDRA> sprites, TextDrawer tdBaseNumber, IndexedImage image);

        private void drawSprActionsAndTriggers(List<SpriteTDRA> sprites, IndexedImage image,
            bool doSprActions, bool doSprTriggers)
        {
            TextDrawer tdSprAction = doSprActions ? getTextDrawerSprAction() : null;
            TextDrawer tdSprTrigger = doSprTriggers ? getTextDrawerSprTrigger() : null;
            foreach (SpriteTDRA spr in sprites)
            {
                //Check first if there is anything to draw.
                string action = null;
                if (doSprActions)
                {
                    action = spr.Action;
                }
                string trigger = null;
                if (doSprTriggers)
                {
                    trigger = spr.Trigger;
                    if (trigger == "None")
                    {
                        trigger = null;
                    }
                }
                if (action != null || trigger != null) //Any text to draw?
                {
                    Rectangle sprDrawBox = spr.getDrawBox();
                    Point sprPos = new Point(sprDrawBox.X + (sprDrawBox.Width / 2), sprDrawBox.Y);
                    if (action != null)
                    {
                        TextDrawer.TextDrawInfo textDi = tdSprAction.getTextDrawInfo(action);
                        Point pos = sprPos.getOffset(-(textDi.Width / 2), 0);
                        textDi.draw(pos, image);
                        sprPos.Y += textDi.Height; //New line.
                    }
                    if (trigger != null)
                    {
                        TextDrawer.TextDrawInfo textDi = tdSprTrigger.getTextDrawInfo(trigger);
                        Point pos = sprPos.getOffset(-(textDi.Width / 2), 0);
                        textDi.draw(pos, image);
                    }
                }
            }
        }

        private static void addWaypoints(FileIni fileIni, TileTracker<TileInfo> infoTracker)
        {
            //Format: number=tileNumber
            //Example: 15=2087
            //Tiberian Dawn:---------------------------------------------------------------------------------
            //Max 32 (0-31) waypoints and tileNumber=-1 means that it isn't used. 0-7 and 25-27 are special.
            //26 is the initial (x+0, y+0 top left) location of the player's view in a singleplayer mission.
            //0-7 are used as starting points in multiplayer games.
            //Red Alert:-------------------------------------------------------------------------------------
            //Max 100 (0-99) waypoints. 0-7, 98 and 99(?) are special.
            //98 is the initial (x+9, y+7 top left) location of the player's view in a singleplayer mission.
            //0-7 are used as starting points in multiplayer games.
            IniSection iniSection = fileIni.findSection("Waypoints");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    int tileNumber = key.valueAsInt32();
                    if (tileNumber >= 0)
                    {
                        getTileInfo(infoTracker.toTilePos(tileNumber), infoTracker).addWaypoint(key.Id);
                    }
                }
            }
        }

        private static void addCellTriggers(FileIni fileIni, TileTracker<TileInfo> infoTracker)
        {
            //Format: tileNumber=trigger
            //Example: 2716=alrt
            //Same in both Tiberian Dawn and Red Alert.
            IniSection iniSection = fileIni.findSection("CellTriggers");
            if (iniSection != null)
            {
                foreach (IniKey key in iniSection.Keys)
                {
                    getTileInfo(infoTracker.toTilePos(key.idAsInt32()), infoTracker).addCellTrigger(key.Value);
                }
            }
        }

        private static TileInfo getTileInfo(TilePos tilePos, TileTracker<TileInfo> infoTracker)
        {
            TileInfo tileInfo = infoTracker[tilePos];
            if (tileInfo == null)
            {
                tileInfo = new TileInfo(tilePos);
                infoTracker.replaceItem(tileInfo); //Tile is empty so replace can be used.
            }
            return tileInfo;
        }

        private class TileInfo : ITilePos
        {
            //Helper class for storing and drawing waypoints and cell triggers.
            private readonly TilePos mTilePos;
            private readonly List<string> mWaypoints; //Only one per tile allowed?
            private readonly List<string> mCellTriggers; //Only one per tile allowed?

            public TileInfo(TilePos tilePos)
            {
                mTilePos = tilePos;
                mWaypoints = new List<string>();
                mCellTriggers = new List<string>();
            }

            public TilePos TilePos
            {
                get { return mTilePos; }
            }

            public void addWaypoint(string number)
            {
                mWaypoints.Add("." + number);
            }

            public void addCellTrigger(string cellTriggerId)
            {
                mCellTriggers.Add(cellTriggerId);
            }

            public void draw(TextDrawer tdWaypoint, TextDrawer tdCellTrigger, IndexedImage image)
            {
                Point pos = new Point(mTilePos.X * MapTDRA.TileWidth, mTilePos.Y * MapTDRA.TileHeight);
                pos.Offset(MapTDRA.TileWidth / 2, MapTDRA.TileHeight / 2); //Center tile.
                int waypointsHeight = tdWaypoint.LineHeight * mWaypoints.Count;
                int cellTriggersHeight = tdCellTrigger.LineHeight * mCellTriggers.Count;
                int totalHeight = waypointsHeight + cellTriggersHeight;
                pos.Y -= totalHeight / 2; //Center text lines.
                drawStrings(mWaypoints, pos, tdWaypoint, image);
                pos.Y += waypointsHeight;
                drawStrings(mCellTriggers, pos, tdCellTrigger, image);
            }

            private static void drawStrings(List<string> strings, Point pos, TextDrawer textDrawer, IndexedImage image)
            {
                foreach (string str in strings)
                {
                    TextDrawer.TextDrawInfo textDi = textDrawer.getTextDrawInfo(str);
                    textDi.draw(pos.getOffset(-textDi.Width / 2, 0), image); //Center text line.
                    pos.Y += textDrawer.LineHeight;
                }
            }
        }

        protected abstract class WrenchDrawerTDRA
        {
            //Helper class for drawing a wrench with text at structures that are base, rebuilt or repaired.
            private readonly Frame mFrame;
            private readonly Rectangle mBox;

            protected WrenchDrawerTDRA(FileShpSpriteSetTDRA wrenchShp, int frameIndex)
            {
                mFrame = wrenchShp.getFrame(frameIndex);
                mBox = wrenchShp.getBoundingBox(frameIndex);
            }

            public void draw(SpriteStructureTDRA spr, TextDrawer.TextDrawInfo textDi, IndexedImage image)
            {
                Rectangle wrenchDrawBox = getWrenchDrawBox(spr.getDrawBox(), mBox);
                //Draw wrench sprite.
                image.draw(mFrame, mBox, wrenchDrawBox.Location);
                //Draw wrench text.
                textDi.draw(getTextDrawPos(wrenchDrawBox), image);
            }

            private static Rectangle getWrenchDrawBox(Rectangle sprDrawBox, Rectangle wrenchBox)
            {
                //Place wrench centered at bottom of structure's visible draw box.
                return new Rectangle(
                    wrenchBox.X = sprDrawBox.X + (sprDrawBox.Width / 2) - (wrenchBox.Width / 2),
                    wrenchBox.Y = sprDrawBox.Bottom - ((wrenchBox.Height / 3) * 2),
                    wrenchBox.Width, wrenchBox.Height);

                //Wrench and text could be outside image clip region if rebuilt structure is at the
                //bottom edge. Maybe adjust the position to fix that, but could get weird if for
                //example the clip region is far away for some reason (wrench is then moved a long way).
                //Probably should keep the wrench somewhere near the structure regardless of clip.
                //A bit complicated and I'm not sure if this is enough of a problem to be worth fixing.

                //Also a problem for pretty much all other types of map info drawing so either
                //all or none of them should be fixed. Probably easier to just add a small margin
                //around the image to catch outside clip drawing.
            }

            private static Point getTextDrawPos(Rectangle wrenchDrawBox)
            {
                //Draw text slightly down-right of wrench center.
                return new Point(
                    wrenchDrawBox.X + (wrenchDrawBox.Width / 2) + 2,
                    wrenchDrawBox.Y + (wrenchDrawBox.Height / 2) + 2);
            }
        }
    }
}
