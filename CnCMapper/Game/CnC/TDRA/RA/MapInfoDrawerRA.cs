#define USE_CUSTOM_FAKE_STRUCT_SIGN //Use a custom sign instead of the ingame for fake structures?

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA.RA
{
    class MapInfoDrawerRA : MapInfoDrawerTDRA
    {
        private static readonly Color IdealColorFgMapData = Color.FromArgb(240, 80, 80); //Light red.
        private static readonly Color IdealColorBgMapData = Color.FromArgb(80, 0, 0); //Dark red.

        private static WrenchDrawerRA mWrenchDrawer = null;
        private FileShpSpriteSetTDRA mFakeStructSign = null; //Sign drawn over fake structures.

        public MapInfoDrawerRA(Palette6Bit palette)
            : base(palette)
        {
        }

        protected override TextDrawer getTextDrawerMapDataInner()
        {
            return createTextDrawer(getFileFnt6x10(), IdealColorFgMapData, IdealColorBgMapData, 1);
        }

        public FileShpSpriteSetTDRA getFakeStructSign()
        {
            //"REDALERT.MIX\HIRES.MIX\PIPS.SHP" has frames with symbols/texts. "FAKE" is in frame 18,
            //but it's a bit small and not very visible on large maps. Use this method to create a
            //similar but slightly larger sign.
            if (mFakeStructSign == null)
            {
                byte backgroundIndex = 4; //Unit shadow (green) background.
                //byte backgroundIndex = mPalette.findClosestMatch(Color.Black); //Black background.
#if USE_CUSTOM_FAKE_STRUCT_SIGN
                //Use our custom font to create the sign.
                TextDrawer td = createTextDrawer(getFileFnt6x10(), Color.White, Color.Black);
                TextDrawer.TextDrawInfo textDi = td.getTextDrawInfo("FAKE"); //Text with...
                Frame frame = new Frame(((textDi.Width / 3) * 5), ((textDi.Height / 3) * 6));
                frame.clear(backgroundIndex); //...specified background color.
                Point pos = new Point(
                    (frame.Width / 2) - (textDi.Width / 2),
                    (frame.Height / 2) - (textDi.Height / 2) + 1);
                textDi.draw(pos, frame);
#else
                //Use ingame sign.
                FileShpSpriteSetTDRA pipsShp = new FileShpSpriteSetTDRA(GameRA.FileMixRedAlertHiRes.get().getFile("PIPS.SHP"));
                Frame frame = new Frame(pipsShp.Width, pipsShp.Height);
                Frame pipsFrame = pipsShp.getFrame(18);
                for (int i = 0; i < frame.Length; i++)
                {
                    byte b = pipsFrame[i];
                    if (b == 0) //Replace transparent with...
                    {
                        b = backgroundIndex; //...specified background color.
                    }
                    frame[i] = b;
                }
#endif
                mFakeStructSign = FileShpSpriteSetTDRA.create("fakesign.shp", new Frame[] { frame }, frame.Size);
            }
            return mFakeStructSign;
        }

        protected override void drawBaseNumbers(List<SpriteTDRA> sprites, TextDrawer tdBaseNumber, IndexedImage image)
        {
            if (mWrenchDrawer == null)
            {
                mWrenchDrawer = new WrenchDrawerRA();
            }
            foreach (SpriteTDRA spr in sprites)
            {
                SpriteStructureRA sprStruct = spr as SpriteStructureRA;
                if (sprStruct != null)
                {
                    string str = null;
                    if (sprStruct.IsBase) //Base structure?
                    {
                        str = (sprStruct.BaseNumber + 1).ToString();
                    }
                    if (sprStruct.IsRepaired) //Repair field in INI-key set?
                    {
                        str += '?'; //Sort of an unknown base number?
                    }
                    if (str != null)
                    {
                        mWrenchDrawer.draw(sprStruct, tdBaseNumber.getTextDrawInfo(str), image);
                    }
                }
            }
        }

        public override void drawHeader(FileIni fileIni, IndexedImage image)
        {
            drawHeader(MissionDataRA.getHeader(fileIni), image);
        }

        private class WrenchDrawerRA : WrenchDrawerTDRA
        {
            public WrenchDrawerRA()
                : base(new FileShpSpriteSetTDRA(GameRA.FileMixMainConquer.get().getFile("SELECT.SHP")), 3)
            {
            }
        }
    }
}
