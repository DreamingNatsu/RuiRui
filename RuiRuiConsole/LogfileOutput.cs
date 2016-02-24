using System;
using System.IO;
using System.Threading.Tasks;

namespace RuiRuiConsole {
    internal class LogfileOutput : IOutput {
        private readonly string _path;
        public LogfileOutput(string path = @"c:\Logs\"){
            if (!path.EndsWith(@"\")) path = path + @"\";
            _path = path;
            Directory.CreateDirectory(_path);
        }

        private StreamWriter GetFileWriter(){
            var t = DateTime.Now;
            var p = $"{_path}{t.Year}-{t.Month}-{t.Day}.log";
            return File.Exists(p) ? File.AppendText(p) : File.CreateText(p);
        }

        public Task WriteLine(string text){
            lock (this) {
                using (var f = GetFileWriter()) {
                    f.WriteLine(text);
                }
            }
            return Task.CompletedTask;
        }

        public void Clear(){
            throw new NotImplementedException();
        }
    }
}