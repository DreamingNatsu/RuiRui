using System;
using System.Data.Entity;
using System.Linq;
using Dba.DAL;
using Dba.DTO;

namespace Logic
{
    public class KeyValueManager : IDisposable {
        private readonly DbCtx _db;
        private DbSet<KeyValueStore> Store => _db.KeyValueStore;
        public KeyValueManager(){
            _db = new DbCtx();
        }

        public string GetValue(string key)
            => DoDb(key);

        public void SaveValue(string key, string value) 
            => DoDb(key, value);

        private string DoDb(string key, string value = ""){
            var kv = Store.FirstOrDefault(k => k.Key == key) ?? Store.Add(new KeyValueStore { Key = key, Value = value });
            if (!string.IsNullOrEmpty(value)&&kv!=null) {
                kv.Key = key;
                kv.Value = value;
            }
            _db.SaveChanges();
            if (kv != null) return string.IsNullOrEmpty(kv.Value)?null:kv.Value;
            return null;
        }

        public void Dispose()=>
            _db.Dispose();
        
    }
}
