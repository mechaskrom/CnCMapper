using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    class GroundLayerTDRA : GroundLayerCnC
    {
        protected static Rectangle toInTiles(Rectangle rectInPixels)
        {
            return toInTiles(rectInPixels, MapTDRA.TileWidth, MapTDRA.TileHeight);
        }

        protected static byte toTileSetIndexClear(int tileX, int tileY)
        {
            //Calculate clear template index (4*4 tiles) from row and column.
            return (byte)((tileX % 4) + (4 * (tileY % 4)));
        }

        protected static Frame getTileFrame(FileIcnTileSetTDRA fileIcn, byte tileSetIndex, TheaterTDRA theater, bool drawTileSetEmptyEffect)
        {
            Frame tileFrame = fileIcn.getTile(tileSetIndex);
            if (!tileFrame.IsEmpty)
            {
                return tileFrame;
            }
            return theater.MapInfoDrawer.getTileSetEmptyTile(drawTileSetEmptyEffect);
        }
    }
}
