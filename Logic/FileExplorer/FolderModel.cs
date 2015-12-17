using System.Collections.Generic;
using System.IO;

namespace Logic.FileExplorer
{
    public class FolderModel
    {
        public string Path;
        public IEnumerable<FileModel> Files { get; set; }
        public IEnumerable<SubDirModel> Subdirectories { get; set; }

    }

    public class SubDirModel
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public SubDirModel(string path)
        {
            Path = path;
            Name = new FileInfo(path).Name;
            
        }
        
        
    }

    public class FileModel : SubDirModel
    {   
        public string Size { get; set; }
        public FileModel(string path) : base(path)
        {
            Size = FileExplorerTools.BytesToString(new FileInfo(path).Length);
        }
    }


}