using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper.Game.CnC
{
    class GroundLayerCnC
    {
        protected static Rectangle toInTiles(Rectangle rectInPixels, int scale)
        {
            return toInTiles(rectInPixels, scale, scale);
        }

        protected static Rectangle toInTiles(Rectangle rectInPixels, int tileWidth, int tileHeight)
        {
            rectInPixels = rectInPixels.Snap(tileWidth, tileHeight); //First snap the rectangle to tiles.
            return new Rectangle(
                rectInPixels.X / tileWidth, rectInPixels.Y / tileHeight,
                rectInPixels.Width / tileWidth, rectInPixels.Height / tileHeight);
        }
    }
}
