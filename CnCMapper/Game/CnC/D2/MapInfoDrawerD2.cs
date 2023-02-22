using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.D2
{
    class MapInfoDrawerD2 : MapInfoDrawerCnC
    {
        private static readonly Color IdealColorFgMapData = Color.FromArgb(252, 212, 136); //Light beige.
        private static readonly Color IdealColorBgMapData = Color.FromArgb(104, 80, 4); //Dark beige.

        public MapInfoDrawerD2(Palette6Bit palette)
            : base(palette)
        {
        }

        protected override TextDrawer getTextDrawerMapDataInner()
        {
            return createTextDrawer(getFileFnt6x10(), IdealColorFgMapData, IdealColorBgMapData, 1);
        }

        public override void drawHeader(FileIni fileIni, IndexedImage image)
        {
            drawHeader(MissionDataD2.getHeader(fileIni), image);
        }

        public void drawExtra(FileIni fileIni, List<SpriteD2> sprites, TheaterD2 theater, IndexedImage image,
            bool doSprActions)
        {
            if (doSprActions)
            {
                drawSprActions(sprites, theater, image);
            }
        }

        private void drawSprActions(List<SpriteD2> sprites, TheaterD2 theater, IndexedImage image)
        {
            TextDrawer tdSprAction = getTextDrawerSprAction();
            foreach (SpriteD2 spr in sprites)
            {
                string action = spr.Action;
                if (action != null) //Any text to draw?
                {
                    Rectangle sprDrawBox = spr.getDrawBox(theater);
                    Point sprPos = new Point(sprDrawBox.X + (sprDrawBox.Width / 2), sprDrawBox.Y);

                    TextDrawer.TextDrawInfo textDi = tdSprAction.getTextDrawInfo(action);
                    Point pos = sprPos.getOffset(-(textDi.Width / 2), 0);
                    textDi.draw(pos, image);
                    sprPos.Y += textDi.Height;
                }
            }
        }
    }
}
