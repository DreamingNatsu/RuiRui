using System.IO;
using System.Linq;

namespace Logic.FileExplorer
{
    public class FolderManager
    {

        public static FolderModel GetFolderModel(string root)
        {
            var ignore = "DS_Store,desktop.ini".Split(',');
            root.Replace('/', '\\');
            var subdirs = Directory.EnumerateDirectories(root).Select(r => new SubDirModel(r));
            var files = Directory.EnumerateFiles(root)
                .Where(r=> !ignore.Any(i=>new FileModel(r).Name.Contains(i)))
                .Select(r => new FileModel(r));
            return new FolderModel{Files = files,Subdirectories = subdirs, Path = root};
        }
    }
}