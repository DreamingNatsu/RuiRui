using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Logic.FileExplorer
{
    public static class FileChangeHandler
    {
        private static string destination;
        private static List<FileSystemWatcher> DirectoryWatchers;
        private static List<FileSystemWatcher> FileWatchers;

        public static void Init(string path, string dest)
        {
            DirectoryWatchers = new List<FileSystemWatcher>();
            FileWatchers = new List<FileSystemWatcher>();
            destination = dest;
            CreateWatchers(path);
            foreach (var fsw in DirectoryWatchers.Union(FileWatchers))
            {
                fsw.IncludeSubdirectories = true;
            }
        }

        public static void CreateRecursiveWatchers(string path)
        {
            CreateWatchers(path);
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                CreateRecursiveWatchers(dir+"\\");
            }
        }

        private static void CreateWatchers(string path)
        {
            DirectoryWatchers.Add(CreateDirectoryWatcher(path));
            FileWatchers.Add(CreateFileWatcher(path));
        }

        private static FileSystemWatcher CreateFileWatcher(string path)
        {
            var fsw = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                               | NotifyFilters.FileName,
                Filter = "*.*"
            };
            fsw.Created += OnFile;
            fsw.Renamed += OnFile;
            fsw.Error += OnError;
            fsw.EnableRaisingEvents = true;
            return fsw;
        }

        
        private static FileSystemWatcher CreateDirectoryWatcher(string path)
        {
            var fsw = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                    | NotifyFilters.DirectoryName,
                Filter = "*",
            };
            fsw.Created += OnDirectory;
            fsw.Error += OnError;
            fsw.EnableRaisingEvents = true;
            return fsw;
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnDirectory(object sender, FileSystemEventArgs e)
        {
            CreateWatchers(e.FullPath);
        }

        private static void OnFile(object source, FileSystemEventArgs e)
        {
            var name = Path.GetFileNameWithoutExtension(e.Name);
            File.WriteAllText(destination + name, e.FullPath);
        }


        //private void CreateFile(string path, int number = 0)
        //{
        //    var writeOut = Path.GetFileNameWithoutExtension(path) + (number == 0 ? "" : number.ToString()) +
        //                      Path.GetExtension(path);
        //    if (File.Exists(writeOut))
        //    {
        //        CreateFile(path, number++);
        //    }
        //    else
        //    {
        //        File.WriteAllText(writeOut,"");
        //    }

        //}
    }
}