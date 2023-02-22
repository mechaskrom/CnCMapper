using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CnCMapper.FileFormat
{
    interface IFileContainer
    {
        string Name { get; }
        string FullName { get; } //Path to this file also including any parent archive + name.
        string GfxFileExt { get; } //Theater graphic files extension if any. Return null if not applicable.
        bool hasFile(string name);
        FileProto findFile(string name); //Returns null if file is not found. Case is ignored.
        FileProto getFile(string name); //Throws if file is not found. Case is ignored.
        IEnumerable<FileProto> getFiles();
    }

    static class IFileContainerExt
    {
        public static T findFileAs<T>(this IFileContainer fc, string name) where T : FileBase, new()
        {
            return fc.findFile(name).parseAs<T>();
        }

        public static T getFileAs<T>(this IFileContainer fc, string name) where T : FileBase, new()
        {
            return fc.getFile(name).parseAs<T>();
        }

        public static IEnumerable<T> tryFilesAs<T>(this IFileContainer fc) where T : FileBase, new()
        {
            return tryFilesAs<T>(fc, fc.GfxFileExt);
        }

        public static IEnumerable<T> tryFilesAs<T>(this IFileContainer fc, string gfxFileExt) where T : FileBase, new()
        {
            return fc.getFiles().tryParseAs<T>(gfxFileExt);
        }

        public static void debugSaveFilesOfTypeInMix<T>(this IFileContainer fc)
            where T : FileBase, new()
        {
            debugSaveFilesOfType<T, FileMixArchiveWw>(fc, null);
        }

        public static void debugSaveFilesOfTypeInPak<T>(this IFileContainer fc)
            where T : FileBase, new()
        {
            debugSaveFilesOfType<T, FilePakArchiveWw>(fc, null);
        }

        public static void debugSaveFilesOfType<T, U>(this IFileContainer fc)
            where T : FileBase, new() //File type.
            where U : FileBase, IFileContainer, new() //Check in archive type.
        {
            debugSaveFilesOfType<T, U>(fc, null);
        }

        public static void debugSaveFilesOfTypeInMix<T>(this IFileContainer fc, Action<T> action)
            where T : FileBase, new()
        {
            debugSaveFilesOfType<T, FileMixArchiveWw>(fc, action);
        }

        public static void debugSaveFilesOfTypeInPak<T>(this IFileContainer fc, Action<T> action)
            where T : FileBase, new()
        {
            debugSaveFilesOfType<T, FilePakArchiveWw>(fc, action);
        }

        public static void debugSaveFilesOfType<T, U>(this IFileContainer fc, Action<T> action)
            where T : FileBase, new() //File type.
            where U : FileBase, IFileContainer, new() //Check in archive type.
        {
            //List all files of type T in the container. Also checks inside U archives.
            //Optional action to perform on file type can be provided.
            StringBuilder sb = new StringBuilder();
            debugSaveFilesOfType<T, U>(fc, action, sb); //Look for files of type T in container.
            if (fc is FolderContainer) //Container may also have sub-folders?
            {
                //If container is a folder also check in all its sub-folders.
                foreach (string folderPath in Directory.GetDirectories(fc.FullName, "*", SearchOption.AllDirectories))
                {
                    debugSaveFilesOfType<T, U>(new FolderContainer(folderPath), action, sb);
                }
            }
            File.WriteAllText(Program.DebugOutPath + "filesOfType.txt", sb.ToString());
        }

        private static void debugSaveFilesOfType<T, U>(IFileContainer fc, Action<T> action, StringBuilder sb)
            where T : FileBase, new() //File type.
            where U : FileBase, IFileContainer, new() //Check in archive type.
        {
            Program.message("debugSaveFilesOfType() " + fc.FullName);

            sb.AppendLine();
            sb.AppendLine(string.Empty.PadRight(120, '-'));
            sb.AppendLine(fc.FullName);
            foreach (T file in fc.tryFilesAs<T>()) //Check container.
            {
                sb.AppendLine(file.Name);
                if (action != null)
                {
                    action(file);
                }
            }
            foreach (U fileMix in fc.tryFilesAs<U>()) //Check inside archive files.
            {
                debugSaveFilesOfType<T, U>(fileMix, action, sb);
            }
        }
    }
}
