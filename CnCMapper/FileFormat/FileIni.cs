using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    //This is just a simple solution and will not handle ill-formatted INI-files well.
    partial class FileIni : FileBase
    {
        private enum IniParseMode
        {
            Unknown,
            Section,
            Key,
            Comment,
        }

        private IniSection mSectionGlobal = null; //Keys at the start before any section has been parsed yet.
        private readonly List<IniSection> mSections = new List<IniSection>(); //Use a list to keep order and duplicate sections.

        public FileIni()
        {
        }

        public FileIni(string filePath)
            : base(filePath)
        {
        }

        public FileIni(FileProto fileProto)
            : base(fileProto)
        {
        }

        private static MemoryStream mDummyStream = null;
        public static FileIni createDummy(string fileName, Origin origin)
        {
            if (mDummyStream == null)
            {
                mDummyStream = new MemoryStream();
                mDummyStream.writeString(";This is an empty dummy file.");
            }
            FileIni fileIni = new FileIni(new FileProto(fileName, mDummyStream, "InternalDummy"));
            fileIni.mOrigin = origin;
            return fileIni;
        }

        public override bool isSuitableExt(string ext, string gfxFileExt)
        {
            return ext == "INI" || ext == "MPR"; //MPR=Regular INI-file for TD&RA multiplayer map.
        }

        protected override void parseInit(Stream stream)
        {
            //INI-files can have keys at the start before any section has been parsed yet.
            //Put these keys in a nameless global section.
            IniSection globalSection = new IniSection(this, "{Global}");
            char[] charBuffer = new char[64];
            int count = 0;
            IniParseMode mode = IniParseMode.Unknown;
            IniSection currentSection = globalSection; //Start with nameless global section.
            string keyId = null;
            for (long i = 0; i < Length; i++)
            {
                int b = stream.ReadByte();
                if (b == '[' && mode == IniParseMode.Unknown)
                {
                    currentSection = null; //New section coming up.
                    count = 0;
                    mode = IniParseMode.Section;
                }
                else if (b == ']' && mode == IniParseMode.Section)
                {
                    string id = new string(charBuffer, 0, count);
                    currentSection = new IniSection(this, id);
                    mSections.Add(currentSection);
                    count = 0;
                    mode = IniParseMode.Unknown;
                }
                else if (b == ';')
                {
                    if (mode == IniParseMode.Unknown)
                    {
                        count = 0;
                        mode = IniParseMode.Comment;
                    }
                    else if (mode == IniParseMode.Key) //Comment after key?
                    {
                        string keyValue = new string(charBuffer, 0, count).Trim();
                        currentSection.addKey(keyId, keyValue);
                        count = 0;
                        mode = IniParseMode.Comment;
                    }
                }
                else if (b == '=' && mode == IniParseMode.Unknown)
                {
                    keyId = new string(charBuffer, 0, count);
                    count = 0;
                    mode = IniParseMode.Key;
                }
                else if (b == '\n')
                {
                    if (mode == IniParseMode.Key)
                    {
                        string keyValue = new string(charBuffer, 0, count).Trim();
                        currentSection.addKey(keyId, keyValue);
                    }
                    count = 0;
                    mode = IniParseMode.Unknown;
                }
                else if (b == '\r') //Ignore.
                {
                }
                else //Add char.
                {
                    if (count >= charBuffer.Length) //Need to expand char buffer?
                    {
                        char[] oldCharBuffer = charBuffer;
                        charBuffer = new char[oldCharBuffer.Length * 2];
                        Array.Copy(oldCharBuffer, charBuffer, oldCharBuffer.Length);
                    }
                    charBuffer[count++] = (char)b;
                }
            }

            if (globalSection.Keys.Count > 0) //Add global keys if any present.
            {
                mSectionGlobal = globalSection;
            }
        }

        public List<IniSection> Sections
        {
            get { return mSections; }
        }

        public IniSection findSection(string id) //Case sensitive.
        {
            return findSection(id, StringComparison.Ordinal);
        }

        public IniSection findSectionIgnoreCase(string id) //Case insensitive.
        {
            return findSection(id, StringComparison.OrdinalIgnoreCase);
        }

        private IniSection findSection(string id, StringComparison stringComparison) //Returns first section found with id. Null if not found.
        {
            return mSections.Find((IniSection s) => s.Id.Equals(id, stringComparison));
        }

        public IniSection findSection(Predicate<IniSection> match)
        {
            return mSections.Find(match);
        }

        public IniSection getSection(string id) //Case sensitive.
        {
            return getSection(id, StringComparison.Ordinal);
        }

        public IniSection getSectionIgnoreCase(string id) //Case insensitive.
        {
            return getSection(id, StringComparison.OrdinalIgnoreCase);
        }

        private IniSection getSection(string id, StringComparison stringComparison) //Returns first section found with id. Throws if not found.
        {
            IniSection section = findSection(id, stringComparison);
            if (section != null)
            {
                return section;
            }
            throw newArgError(string.Format("Couldn't find section '{0}'!", id));
        }

        public void debugSaveContent()
        {
            debugSaveContent(Program.DebugOutPath + "ini\\");
        }

        public void debugSaveIniContentParsed()
        {
            string folderPath = Program.DebugOutPath + "ini\\";
            Directory.CreateDirectory(folderPath);
            StringBuilder sb = new StringBuilder();
            foreach (IniSection s in mSections)
            {
                sb.AppendLine("Section = {0}", s.Id);
                foreach (IniKey k in s.Keys)
                {
                    sb.AppendLine("Key = {0}, Value = {1}", k.Id, k.Value);
                }
                sb.AppendLine();
            }
            File.WriteAllText(folderPath + Name + " parsed.txt", sb.ToString());
        }
    }

    class IniSection
    {
        private const int PackLineWidthMax = 70; //Max line width of pack data strings.

        private readonly FileIni mParentFile;
        private readonly string mId;
        private readonly List<IniKey> mKeys; //Use a list to keep order (ascending line number) and duplicate keys.

        public IniSection(FileIni parentFile, string id)
        {
            mParentFile = parentFile;
            mId = id;
            mKeys = new List<IniKey>();
        }

        public FileIni ParentFile
        {
            get { return mParentFile; }
        }

        public string Id
        {
            get { return mId; }
        }

        public List<IniKey> Keys
        {
            get { return mKeys; }
        }

        public void addKey(string id, string value)
        {
            mKeys.Add(new IniKey(this, id, value));
        }

        public void insertKey(int index, string id, string value)
        {
            mKeys.Insert(index, new IniKey(this, id, value));
        }

        public IniKey findKey(string id) //Returns first key found with id (case sensitive). Null if not found.
        {
            return mKeys.Find((IniKey k) => k.Id == id);
        }

        public IniKey getKey(string id) //Returns first key found with id (case sensitive). Throws if not found.
        {
            IniKey key = findKey(id);
            if (key != null)
            {
                return key;
            }
            throw mParentFile.newArgError(string.Format("Couldn't find key '{0}' in section '{1}'!", id, mId));
        }

        public string getPackDataAsBase64()
        {
            //Returns key values in a section with pack-data, e.g. [MapPack] and [OverlayPack], as one base64 string.
            StringBuilder sb = new StringBuilder();
            foreach (IniKey key in mKeys)
            {
                sb.Append(key.Value);
            }
            return sb.ToString();
        }

        public static List<KeyValuePair<string, string>> toPackDataFormatted(string packData)
        {
            //Formats pack data string as a list of {id,value} lines. Ready to be inserted into a pack data section.
            List<KeyValuePair<string, string>> lines = new List<KeyValuePair<string, string>>();
            int length = packData.Length;
            for (int i = 0; length > 0; i++, length -= PackLineWidthMax)
            {
                string keyId = (i + 1).ToString();
                string keyValue = packData.Substring(
                    i * PackLineWidthMax, Math.Min(length, PackLineWidthMax));
                lines.Add(new KeyValuePair<string, string>(keyId, keyValue));
            }
            return lines;
        }
    }

    class IniKey
    {
        private readonly IniSection mParentSection;
        private readonly string mId;
        private string mValue;

        public IniKey(IniSection parentSection, string id, string value)
        {
            mParentSection = parentSection;
            mId = id;
            mValue = value;
        }

        public IniSection ParentSection
        {
            get { return mParentSection; }
        }

        public string Id
        {
            get { return mId; }
        }

        public string Value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        public Int32 idAsInt32()
        {
            Int32 result;
            if (Int32.TryParse(mId, out result))
            {
                return result;
            }
            throw mParentSection.ParentFile.newArgError(string.Format(
                "Couldn't parse key '{0}/{1}' id as an integer!", mParentSection.Id, mId));
        }

        public bool valueAsBool()
        {
            if (mValue == "yes" || mValue == "Yes" || mValue == "true" || mValue == "True" || mValue == "1")
            {
                return true;
            }
            if (mValue == "no" || mValue == "No" || mValue == "false" || mValue == "False" || mValue == "0")
            {
                return false;
            }
            throw mParentSection.ParentFile.newArgError(string.Format(
                "Couldn't parse '{0}/{1}/{2}' as a boolean!", mParentSection.Id, mId, mValue));
        }

        public Int32 valueAsInt32()
        {
            Int32 result;
            if (Int32.TryParse(mValue, out result))
            {
                return result;
            }
            throw mParentSection.ParentFile.newArgError(string.Format(
                "Couldn't parse '{0}/{1}/{2}' as an integer!", mParentSection.Id, mId, mValue));
        }

        public UInt32 valueAsUInt32()
        {
            UInt32 result;
            if (UInt32.TryParse(mValue, out result))
            {
                return result;
            }
            throw mParentSection.ParentFile.newArgError(string.Format(
                "Couldn't parse '{0}/{1}/{2}' as an unsigned integer!", mParentSection.Id, mId, mValue));
        }

        public T valueAsEnum<T>()
        {
            try
            {
                return (T)Enum.Parse(typeof(T), mValue);
            }
            catch (Exception ex)
            {
                throw mParentSection.ParentFile.newArgError(string.Format(
                    "Couldn't parse '{0}/{1}/{2}' as an enum! Reason: {3}", mParentSection.Id, mId, mValue, ex.Message));
            }
        }
    }
}
