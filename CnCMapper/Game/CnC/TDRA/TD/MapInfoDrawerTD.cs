using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.TD
{
    class MapInfoDrawerTD : MapInfoDrawerTDRA
    {
        private static readonly Color IdealColorFgMapData = Color.FromArgb(240, 200, 80); //Light orange.
        private static readonly Color IdealColorBgMapData = Color.FromArgb(120, 80, 40); //Dark orange.

        private static WrenchDrawerTD mWrenchDrawer = null;

        public MapInfoDrawerTD(Palette6Bit palette)
            : base(palette)
        {
        }

        protected override TextDrawer getTextDrawerMapDataInner()
        {
            return createTextDrawer(getFileFnt6x10(), IdealColorFgMapData, IdealColorBgMapData, 1);
        }

        protected override void drawBaseNumbers(List<SpriteTDRA> sprites, TextDrawer tdBaseNumber, IndexedImage image)
        {
            if (mWrenchDrawer == null)
            {
                mWrenchDrawer = new WrenchDrawerTD();
            }
            foreach (SpriteTDRA spr in sprites)
            {
                SpriteStructureTD sprStruct = spr as SpriteStructureTD;
                if (sprStruct != null && sprStruct.IsBase) //Base structure?
                {
                    mWrenchDrawer.draw(sprStruct, tdBaseNumber.getTextDrawInfo((sprStruct.BaseNumber + 1).ToString()), image);
                }
            }
        }

        public override void drawHeader(FileIni fileIni, IndexedImage image)
        {
            drawHeader(MissionDataTD.getHeader(fileIni), image);
        }

        private class WrenchDrawerTD : WrenchDrawerTDRA
        {
            public WrenchDrawerTD()
                : base(new FileShpSpriteSetTDRA(GameTD.FileMixConquer.get().getFile("SELECT.SHP")), 3)
            {
            }
        }
    }
}
