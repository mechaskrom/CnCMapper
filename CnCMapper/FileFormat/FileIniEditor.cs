using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    partial class FileIni
    {
        //Simple INI-file editor. Mostly created for making mission INI-file tests.
        //Any comments and formatting isn't supported and will not be retained when file is saved.
        public class Editor
        {
            private readonly FileInfo mSourceInfo;
            private readonly FileIni mSourceIni;
            private readonly List<IniSection> mSections; //Edited sections. Use a list to keep order and duplicate sections.

            private Editor(FileInfo sourceInfo)
            {
                mSourceInfo = sourceInfo;
                //Copy source file to memory so it isn't locked (save can replace original file on disk).
                mSourceIni = new FileIni(FileProto.create(sourceInfo.FullName, true));
                mSections = mSourceIni.mSections;
            }

            public static Editor open(string path)
            {
                FileInfo sourceInfo = new FileInfo(path);
                Editor editor = new Editor(sourceInfo);
                return editor;
            }

            public FileInfo SourceInfo
            {
                get { return mSourceInfo; }
            }

            public IniSection findSection(string id)
            {
                return mSections.Find((IniSection s) => s.Id == id);
            }

            public IniSection getSection(string id)
            {
                IniSection section = findSection(id);
                if (section != null)
                {
                    return section;
                }
                throw new ArgumentException(string.Format(
                    "Couldn't find section '{0}' in edited INI-file '{1}'!", id, mSourceInfo.FullName));
            }

            public IniSection findOrAddSection(string id) //Create and insert at end if not found.
            {
                return findOrAddSection(id, false);
            }

            public IniSection findOrAddSectionStart(string id) //Create and insert at start if not found.
            {
                return findOrAddSection(id, true);
            }

            private IniSection findOrAddSection(string id, bool insertStart)
            {
                IniSection section = findSection(id);
                if (section == null)
                {
                    section = new IniSection(mSourceIni, id);
                    if (insertStart)
                    {
                        mSections.Insert(0, section);
                    }
                    else
                    {
                        mSections.Add(section);
                    }
                }
                return section;
            }

            public void save(string path) //Replaces file if it exists.
            {
                File.WriteAllText(path, getTextContent());
            }

            public string getTextContent() //Current text content of edited INI-file.
            {
                StringBuilder sb = new StringBuilder();
                //Save any global keys first.
                if (mSourceIni.mSectionGlobal != null)
                {
                    saveSectionKeys(sb, mSourceIni.mSectionGlobal);
                    sb.AppendLine();
                }

                //Save sections.
                foreach (IniSection section in mSections)
                {
                    saveSection(sb, section);
                    sb.AppendLine();
                }
                return sb.ToString();
            }

            private void saveSection(StringBuilder sb, IniSection section)
            {
                sb.AppendLine('[' + section.Id + ']');
                saveSectionKeys(sb, section);
            }

            private void saveSectionKeys(StringBuilder sb, IniSection section)
            {
                foreach (IniKey key in section.Keys)
                {
                    sb.AppendLine(key.Id + '=' + key.Value);
                }
            }
        }
    }
}
