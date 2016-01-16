using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dba.DAL;
using Dba.DTO;
using Logic.Models;

namespace Logic
{
    public class SoundboardManager
    {
        private DbCtx db;
        public SoundboardManager()
        {
            db = new DbCtx();
        }

        public List<SoundEntry> GetSoundboard()
        {
            return db.SoundEntries.ToList();
        }

        //public bool UploadSounds(UploadViewModel model, string serverpath)
        //{
        //    if (model.Files.First() == null) return false;
        //    foreach (var file in model.Files)
        //    {
        //        var name = Path.GetFileNameWithoutExtension(file.FileName);
        //        var path = Guid.NewGuid() + ".mp3";
        //        file.SaveAs(Path.Combine(serverpath,path));
        //        db.SoundEntries.Add(new SoundEntry
        //        {
        //            Name = name,
        //            Path = "/Audio/"+ path,
        //            Category = model.Category
        //        });
        //    }
        //    db.SaveChanges();
        //    return true;
        //}

        public List<SoundEntry> GetSoundboard(string s)
        {
            return db.SoundEntries
                //.Where(sb=>sb.Category == s)
                .ToList();
        }
    }
}