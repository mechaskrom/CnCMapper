using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.Game.CnC
{
    abstract class TheaterCnC
    {
        protected const byte GfxSheetBackgroundIndex = 2; //Background palette index in debug saved graphics sheets.

        private readonly string mName;

        protected TheaterCnC(string name)
        {
            mName = name;
        }

        public string Name
        {
            get { return mName; }
        }

        public abstract PaletteCnC getFilePaletteCnC();
        public abstract PaletteCnC getGamePaletteCnC();

        protected static void debugSaveColorRemaps(TheaterCnC theater, List<KeyValuePair<string, byte[]>> remaps)
        {
            string folderPath = Program.DebugOutPath + "remaps\\";
            Directory.CreateDirectory(folderPath);
            foreach (KeyValuePair<string, byte[]> pair in remaps)
            {
                string filePath = folderPath + theater.Name + " " + pair.Key;

                //Save as an image.
                pair.Value.savePixels(16, theater.getFilePaletteCnC(), filePath + ".png"); //File palette.
                pair.Value.savePixels(16, theater.getGamePaletteCnC(), filePath + " adjusted palette.png"); //Game palette.

                //Save as text.
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    sb.Append(pair.Value[i].ToString("X2") + ",");
                    if ((i % 16) == 15)
                    {
                        sb.AppendLine();
                    }
                }
                File.WriteAllText(filePath + ".txt", sb.ToString());
            }
        }
    }
}
