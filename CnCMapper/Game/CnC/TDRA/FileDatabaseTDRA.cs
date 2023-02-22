using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CnCMapper.FileFormat;

namespace CnCMapper.Game.CnC.TDRA
{
    //Parse the commonly available "global mix database.dat" into a dictionary for looking up name of files in MIX-archives.
    //"global mix database.dat" was created by Olaf van der Spek? https://xhp.xwis.net/
    //The version I currently use is based on the one from the OpenRA project. https://github.com/OpenRA/OpenRA/

    //Layout: sections (order of games in the file I use is: "TiberianDawn", "RedAlert", "TiberianSun", "RedAlert2")

    //section:
    //-entry count UInt32: Number of entries in the section i.e. number of entries until the next section starts.
    //-entry char[entry count][2][]: Section entries, name + comment zero terminated ASCII strings.

    //section entry:
    //-name char[]: File name as a zero terminated ASCII string.
    //-comment char[]: Comment as a zero terminated ASCII string.

    abstract class FileDatabaseTDRA : FileMixArchiveWw.IFileDatabase
    {
        private const string FileDatabaseFileName = "global mix database.dat";

        protected enum DatSection //DAT-file section order.
        {
            TiberianDawn = 0,
            RedAlert = 1,
            TiberianSun = 2,
            RedAlert2 = 3,
        }

        private Dictionary<UInt32, string> mFileDatabase; //MIX-archive entry id to file name look-up.

        public string toFileName(UInt32 fileId)
        {
            if (mFileDatabase == null)
            {
                mFileDatabase = createFileDatabaseInner();
            }
            string fileName;
            if (mFileDatabase.TryGetValue(fileId, out fileName))
            {
                return fileName;
            }
            string id = fileId.ToString("X8"); //Convert value to hexadecimal.
            Program.warn(string.Format("File id '0x{0}' not found in database!", id));
            return id + "." + FileFormat.FileUnknown.Extension;

            //----------Unknown files found so far i.e. file name not in the current file database----------
            //-0xBDD2A397 = "\Command & Conquer Red Alert(tm)\MAIN.MIX\movies1.mix\BDD2A397.UNK", 20273998 bytes, offset 231343272.
            //-0xBDD2A397 = "\Command & Conquer Red Alert(tm)\MAIN.MIX\movies2.mix\BDD2A397.UNK", 20273998 bytes, offset 358232202.
            //Same file in both MIX-archives and it is a VQA-movie. Can be played in XCC Mixer. A trailer for "Lands of Lore: Guardians of Destiny"?

            //-0x27FF53F1 = "\Command & Conquer Red Alert(tm)\REDALERT.MIX\nchires.mix\27FF53F1.UNK", 1644804 bytes, offset 3402506.
            //-0x27FF53F1 = "\Command & Conquer Red Alert(tm)\REDALERT.MIX\nchires.mix\27FF53F1.UNK", 1644804 bytes, offset 7514714.
            //XCC Mixer thinks it's a VQP-file. A format used together with VQA-movies?
            //Exists at two different offsets. One is right after "ENGLISH.VQA" so it probably belongs to that movie.
        }

        public UInt32 toFileId(string fileName) //Interface member can't be static.
        {
            return toFileIdStatic(fileName);
        }

        protected static UInt32 toFileIdStatic(string fileName)
        {
            return Crypt.Crc.crcTDRA(fileName.ToUpperInvariant()); //File id is crc of file name in upper case.
        }

        protected abstract Dictionary<UInt32, string> createFileDatabaseInner();

        protected static Dictionary<UInt32, string> createFileDatabase(DatSection datSection)
        {
            //Reading database from embedded resource is around 6x slower (0.020s compared to 0.003s) than
            //external file for some reason. It's not because it's compressed (very small overhead), maybe
            //some file caching done by Windows? Having it embedded is a lot nicer though so let's use it
            //in the non-debug release at least.

            //The file database is mostly used in debug methods (print files in a MIX-archive for example).
            //In normal execution when rendering all maps it's only used for finding INI-files.
            //I tested if using an array with all known mission INI-files instead of using the file database
            //was faster. It maybe was a bit faster, but not obvious enough to be worth all the extra code.
            //Most of the running time is spent on rendering all the maps anyway.

#if DEBUG
            //Read from external file.
            using (FileStream fs = File.OpenRead(Program.ProgramPath + FileDatabaseFileName))
            {
                return createFileDatabase(fs, datSection);
            }
#else
            //Read from embedded resource.
            //3-9 times slower than external file. Not because of decompression. Still pretty fast though.
            using (MemoryStream msComp = new MemoryStream(CnCMapper.Properties.Resources.global_mix_database_TDRA_dat))
            {
                using (MemoryStream msDecomp = msComp.getDecompressed())
                {
                    return createFileDatabase(msDecomp, datSection);
                }
            }
#endif
        }

        private static Dictionary<UInt32, string> createFileDatabase(Stream stream, DatSection datSection)
        {
            //Seek to and parse section.
            seekDatSection(stream, datSection);
            Dictionary<UInt32, string> fileDatabase = new Dictionary<UInt32, string>();
            for (UInt32 count = stream.readUInt32(); count > 0; count--) //Number of entries (name + comment).
            {
                string name = stream.readString(); //Read name.
                UInt32 id = toFileIdStatic(name);
                fileDatabase.Add(id, name);

                stream.readSkip(0); //Skip comment.
            }
            return fileDatabase;
        }

        private static void seekDatSection(Stream stream, DatSection datSection)
        {
            stream.Seek(0, SeekOrigin.Begin);
            for (DatSection ds = 0; ds != datSection; ds++)
            {
                skipDatSection(stream);
            }
        }

        private static void skipDatSection(Stream stream)
        {
            for (UInt32 count = stream.readUInt32(); count > 0; count--) //Number of entries (name + comment).
            {
                stream.readSkip(0); //Skip name.
                stream.readSkip(0); //Skip comment.
            }
        }

        public static void debugSaveDatEntries()
        {
            //Save all entries stored in the DAT-file.
            StringBuilder sb = new StringBuilder();
            using (FileStream fs = File.OpenRead(Program.ProgramPath + FileDatabaseFileName))
            {
                string sectionBreak = "".PadRight(150, '#');
                for (DatSection section = 0; fs.Position < fs.Length; section++)
                {
                    sb.AppendLine(sectionBreak);
                    UInt32 count = fs.readUInt32(); //Number of entries (name + comment).
                    sb.AppendLine("Section={0}:{1}, Count={2}", (int)section, section, count);
                    for (int i = 0; i < count; i++)
                    {
                        string name = fs.readString(); //Read name.
                        string comment = fs.readString(); //Read comment.
                        sb.AppendLine("Name={0}, Comment={1}", name, comment);
                    }
                    sb.AppendLine();
                }
            }
            File.WriteAllText(Program.ProgramPath + "file database entries.txt", sb.ToString());
        }

        public static void debugSaveDatTDRA()
        {
            //Save a "global mix database.dat" with only Tiberian Dawn and Red Alert sections, and without comments.
            debugSaveDat(new DatSection[] { DatSection.TiberianDawn, DatSection.RedAlert }, false);
        }

        private static void debugSaveDat(DatSection[] includeDatSections, bool includeComments)
        {
            //Save a version of the original "global mix database.dat" with only specified
            //sections included and with or without comments.
            using (MemoryStream ms = new MemoryStream())
            {
                //Open the original DAT-file.
                using (FileStream fs = File.OpenRead(Program.ProgramPath + "global mix database org.dat"))
                {
                    for (DatSection section = 0; fs.Position < fs.Length; section++)
                    {
                        if (includeDatSections.Contains(section)) //Include section?
                        {
                            UInt32 count = fs.readUInt32(); //Number of entries (name + comment).
                            ms.writeUInt32(count);
                            for (; count > 0; count--)
                            {
                                //Copy name.
                                fs.copyToUntil(ms, 0);
                                ms.WriteByte(0);

                                //Copy comment if included.
                                if (includeComments)
                                {
                                    fs.copyToUntil(ms, 0);
                                }
                                else
                                {
                                    fs.readSkip(0);
                                }
                                ms.WriteByte(0);
                            }
                        }
                        else //Skip section.
                        {
                            skipDatSection(fs);
                            ms.writeUInt32(0); //Write an empty section with 0 count.
                        }
                    }
                }

                //Write as DAT-file.
                ms.Seek(0, SeekOrigin.Begin);
                File.WriteAllBytes(Program.ProgramPath + FileDatabaseFileName, ms.ToArray());

                //Write as compressed DAT-file.
                ms.Seek(0, SeekOrigin.Begin);
                ms.saveCompressed(Program.ProgramPath + FileDatabaseFileName + ".gz");
            }
        }
    }
}
