using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC
{
    //Deals with extra info text in map like title and sprite actions.
    abstract class MapInfoDrawerCnC
    {
#if DRAW_CREATED_INFO
        private const string CreatedInfo = "Compiled 20230218 by mechaskrom@gmail.com";
#endif
        private static readonly Color IdealColorFgSprAction = Color.FromArgb(0, 160, 160); //Dark cyan.
        private static readonly Color IdealColorBgSprAction = Color.Black;

        private static FileFntFontWw mFileFnt6x10 = null;
        private readonly Palette6Bit mPalette;

        private TextDrawer mTextDrawerSprAction = null;
        private TextDrawer mTextDrawerMapData = null;

        protected MapInfoDrawerCnC(Palette6Bit palette) //Should use a game palette i.e. after any settings adjustment.
        {
            mPalette = palette;
        }

        protected byte findClosestPaletteIndex(Color color)
        {
            return mPalette.findClosestMatch(color);
        }

        public static FileFntFontWw getFileFnt6x10()
        {
            if (mFileFnt6x10 == null)
            {
                //"font6x10.fnt" is my custom font based on "https://dwarffortresswiki.org/index.php/File:Bedstead-10-df.png"
                MemoryStream stream = new MemoryStream(CnCMapper.Properties.Resources.font6x10_CnC);
                mFileFnt6x10 = new FileFntFontWw(new FileProto("font6x10.fnt", stream, "InternalResources"));
            }
            return mFileFnt6x10;
        }

        protected TextDrawer getTextDrawerSprAction()
        {
            if (mTextDrawerSprAction == null)
            {
                mTextDrawerSprAction = createTextDrawer(getFileFnt6x10(), IdealColorFgSprAction, IdealColorBgSprAction);
            }
            return mTextDrawerSprAction;
        }

        private TextDrawer getTextDrawerMapData()
        {
            if (mTextDrawerMapData == null)
            {
                mTextDrawerMapData = getTextDrawerMapDataInner();
            }
            return mTextDrawerMapData;
        }

        protected abstract TextDrawer getTextDrawerMapDataInner();

        protected TextDrawer createTextDrawer(FileFntFontWw fileFnt, Color idealFg, Color idealBg)
        {
            return createTextDrawer(fileFnt, idealFg, idealBg, 0);
        }

        protected TextDrawer createTextDrawer(FileFntFontWw fileFnt, Color idealFg, Color idealBg, int spacing)
        {
            return createTextDrawer(fileFnt, idealFg, idealBg, spacing, mPalette);
        }

        private static TextDrawer createTextDrawer(FileFntFontWw fileFnt, Color idealFg, Color idealBg, int spacing, Palette6Bit palette)
        {
            byte fg = palette.findClosestMatch(idealFg);
            byte bg = palette.findClosestMatch(idealBg);
            byte[] remap = new byte[] { 0, fg, bg, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            return new TextDrawer(fileFnt, remap, spacing);
        }

        public abstract void drawHeader(FileIni fileIni, IndexedImage image);

        protected void drawHeader(string header, IndexedImage image)
        {
            Rectangle clipRect = image.Clip;
            //Draw header with map info.
            TextDrawer td = getTextDrawerMapData();
            td.draw(header, clipRect.Location.getOffset(2, 2), image);
#if DRAW_CREATED_INFO
            //Draw footer with created info.
            td.draw(CreatedInfo, clipRect.Location.getOffset(2, clipRect.Height - td.LineHeight - 2), image);
#endif
        }
    }
}
