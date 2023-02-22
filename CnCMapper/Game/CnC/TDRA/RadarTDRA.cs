using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    class RadarTDRA : RadarCnC
    {
        protected const int ScaleMin = 1;
        protected const int ScaleMax = 12;
        public const string ConfigScaleValues = "comma separated integers, 1 to 12"; //For config file attribute. Must be const.

        private const int MiniTileWidth = 3; //In pixels. Always 3 in both Tiberian Dawn and Red Alert. Checked in source.
        private const int MiniTileHeight = 3;
        private const int MiniTileZoomFactor = 3; //Always 3 in both Tiberian Dawn and Red Alert. Checked in source.
        private const int ZoomedTileWidth = MapTDRA.TileWidth / MiniTileZoomFactor;
        private const int ZoomedTileHeight = MapTDRA.TileHeight / MiniTileZoomFactor;

        private static readonly Dictionary<Frame, Frame[,]> mMiniTiles = new Dictionary<Frame, Frame[,]>(); //Frame to mini tiles lookup.

        public static int[] getConfigScales(IniKey iniKey)
        {
            return getConfigScales(iniKey, ScaleMin, ScaleMax);
        }

        private static Frame[,] getFrameAsMiniTiles(FileShpSpriteSetTDRA fileShp, int frameIndex)
        {
            Frame[,] miniTiles;
            Frame frame = fileShp.getFrame(frameIndex);
            if (!mMiniTiles.TryGetValue(frame, out miniTiles))
            {
                miniTiles = frameToMiniTiles(frame, fileShp, frameIndex);
                mMiniTiles.Add(frame, miniTiles);
            }
            return miniTiles;
        }

        private static Frame[,] frameToMiniTiles(Frame frame, FileShpSpriteSetTDRA fileShp, int frameIndex)
        {
            //Creates a mini tile version of a SHP-file frame where each 24*24 pixels tile becomes a 3*3 pixels tile.
            //Essentially a 1/8 scale of the frame, but in whole mini tiles. Used for terrain and overlay sprites in the radar.
            //See "Get_Radar_Icon()" in "CONQUER.CPP".
            int frameWidthInTiles = (frame.Width + (MapTDRA.TileWidth / 2)) / MapTDRA.TileWidth; //Round with half tile.
            int frameHeightInTiles = (frame.Height + (MapTDRA.TileHeight / 2)) / MapTDRA.TileHeight;
            Frame[,] miniTiles = new Frame[frameHeightInTiles, frameWidthInTiles];
            int[] offsets = new int[] { 0, 1, -1 };
            bool wasOutOfBounds = false;

            //DOS version (Tiberian Dawn). Matches scale 3 at least. See comment at end in draw scaled method about DOS version scaler.
            //int getTweak = 4;

            //Windows version (Tiberian Dawn and Red Alert).
            int getTweak = 1;

            for (int tileY = 0; tileY < frameHeightInTiles; tileY++)
            {
                for (int tileX = 0; tileX < frameWidthInTiles; tileX++)
                {
                    Frame miniTile = new Frame(MiniTileWidth, MiniTileHeight);
                    int tileInd = 0;
                    for (int y = 0; y < MiniTileHeight; y++)
                    {
                        int getY = (tileY * MapTDRA.TileHeight) + (y * ZoomedTileHeight) + getTweak;
                        for (int x = 0; x < MiniTileWidth; x++)
                        {
                            byte pixel = 0;
                            int getX = (tileX * MapTDRA.TileWidth) + (x * ZoomedTileWidth) + getTweak;
                            if ((getX < frame.Width) && (getY < frame.Height)) //Inside frame?
                            {
                                //Look for a pixel that isn't transparent or unit shadow green (index 4).
                                for (int i = 0; i < offsets.Length; i++) //Try different offsets.
                                {
                                    int pixelsInd = ((getY + offsets[i]) * frame.Stride) + getX + offsets[i];
                                    if (pixelsInd < frame.Pixels.Length) //Read is inside bounds?
                                    {
                                        byte p = frame[pixelsInd];
                                        if (p != 0 && p != 4)
                                        {
                                            pixel = p;
                                            break;
                                        }
                                    }
                                    else //Read was out of bounds.
                                    {
                                        wasOutOfBounds = true;
                                    }
                                }
                            }
                            miniTile[tileInd++] = pixel;
                        }
                    }
                    miniTiles[tileY, tileX] = miniTile;
                }
            }

            if (wasOutOfBounds)
            {
                Program.warn(string.Format("Read outside frame when converting '{0}', index '{1}' to a radar mini tile!",
                    fileShp.Name, frameIndex));
                //"ROCK5.DES" is the only sprite (in both Tiberian Dawn and Red Alert) that is read outside frame here.
                //Its mini-tile had some extra greenish pixels in the game when I tested. There are no greenish pixels
                //in any frame in its file so probably garbage?
            }

            return miniTiles;
        }

        public static void drawMiniTiles(int scale, SpriteTDRA sprite, byte[] colorRemap, IndexedImage image, bool forceSingle)
        {
            Frame[,] miniTiles = getFrameAsMiniTiles(sprite.FileShp, sprite.FrameIndex);
            Point dstPos = new Point(0, sprite.TilePos.Y * scale);
            Size sizeInTiles = forceSingle ? new Size(1, 1) : new Size(miniTiles.GetLength(1), miniTiles.GetLength(0));
            for (int tileY = 0; tileY < sizeInTiles.Height; tileY++, dstPos.Y += scale)
            {
                dstPos.X = sprite.TilePos.X * scale;
                for (int tileX = 0; tileX < sizeInTiles.Width; tileX++, dstPos.X += scale)
                {
                    drawScaled(miniTiles[tileY, tileX], image, dstPos, colorRemap, scale);
                }
            }
        }

        public static void drawScaled(Frame srcFrame, Frame dstFrame, Point dstPos, byte[] remap, int scale)
        {
            drawScaled(srcFrame.Pixels, srcFrame.Stride, dstFrame.Pixels, dstFrame.Stride, dstPos, remap, scale);
        }

        private static void drawScaled(byte[] srcPixels, int srcStride, byte[] dstPixels, int dstStride, Point dstPos, byte[] remap, int scale)
        {
            //Draws "srcPixels" at "dstPos" in "dstPixels" with size set by "scale".
            //Source and destination are 8-bit indexed pixels.
            //Source pixels with a 0 index aren't drawn (transparent). Source is clipped inside destination.
            //Optionally use "remap"-table to remap pixel indices.

            System.Diagnostics.Debug.Assert(srcPixels.Length % srcStride == 0, "Source stride is not a multiple of source array length!");
            System.Diagnostics.Debug.Assert(dstPixels.Length % dstStride == 0, "Destination stride is not a multiple of destination array length!");

            //Calculate actual destination rectangle after clipping.
            Rectangle dstRect = new Rectangle(dstPos.X, dstPos.Y, scale, scale);
            Rectangle dstClip = dstPixels.getRect(dstStride);
            dstRect.Intersect(dstClip); //Clip draw rectangle inside destination.

            //Draw "srcPixels" at "dstPos" in "dstPixels" with size set by "scale".
            int dstInd = dstRect.X + (dstRect.Y * dstStride);
            Size srcSize = srcPixels.getRect(srcStride).Size;

            //DOS version (Tiberian Dawn). Matches scale 3 at least. See comment below at end.
            //int xTweak = (srcSize.Width / scale) / 2;
            //int yTweak = (srcSize.Height / scale) / 2;
            //int upTweak = 0; //??? DOS upscaling not tested.

            //Windows version (Tiberian Dawn and Red Alert).
            int xTweak = 0;
            int yTweak = 0;
            int upTweak = scale > srcSize.Height ? -1 : 0; //To better match the games when upscaling.

            for (int y = 0; y < dstRect.Height; y++, dstInd += dstStride)
            {
                int getY = (((y * srcSize.Height) + upTweak) / scale) + yTweak;
                int srcInd = getY * srcStride;
                for (int x = 0; x < dstRect.Width; x++)
                {
                    int getX = ((x * srcSize.Width) / scale) + xTweak;
                    byte b = srcPixels[srcInd + getX];
                    if (remap != null) //Remap index?
                    {
                        b = remap[b];
                    }
                    if (b != 0) //Not a transparent pixel?
                    {
                        dstPixels[dstInd + x] = b;
                    }
                }
            }

            //This method seems to match Tiberian Dawn (windows gold version) radar at scale 2 and 6 and Red Alert radar at scale 3.
            //Unfortunately it doesn't match Tiberian Dawn (DOS version) radar at scale 3. Not sure if DOS version uses a
            //different scaling method or if Tiberian Dawn is different than Red Alert at scale 3.
            //Considering how bad overlays look at scale 3 (a straight concrete wall is a jagged mess of pixels) in the DOS version
            //I'm suspecting the radar scaling was fixed/changed in the windows gold version.

            //I haven't tested the Red Alert DOS version at all.

            //Update! Nyerguds C&C Gold patch (V.1.06c r3) can run Tiberian Dawn Gold in windowed mode which makes it easy to
            //use Cheat Engine to force the radar scale to whatever. I tried scale 3 and it matched this scaling method.
            //So it's just the DOS version that is weird.
            //http://nyerguds.arsaneus-design.com/cnc95upd/cc95p106/
            //https://cheatengine.org/

            //Tweaks found so far to better match Tiberian Dawn DOS version at scale 3:
            //A 24*24 map tile is sampled like this at scale 3 in the DOS version (starts at x/y 4,4 and increases 8 per row/col):
            //[04,04],[12,04],[20,04]
            //[04,12],[12,12],[20,12]
            //[04,20],[12,20],[20,20]
            //Setting "xTweak" and "yTweak" to 4 if 24*24 map tile or 0 if 3*3 terrain mini-tile ((size / scale) / 2)
            //will match DOS version at scale 3 at least.
            //Setting "getTweak" to 4 in the frame-to-mini-tiles method above will also make terrains match.
            //Overlays are still weird though. Seems like their 3*3 mini-tiles are incorrectly drawn and
            //several pixels are garbage data from out of bounds reads?
        }
    }
}
