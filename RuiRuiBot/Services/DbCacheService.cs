using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Discord;
using Discord.Commands;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;

namespace RuiRuiBot.Services
{
    /// <summary>
    /// an in memory cache of EntityFramework database tables AND THIS SHIT IS FUCKING UNNESSECARY HAH
    /// </summary>
    public class DbCacheService<TDbCtx> : IService where TDbCtx : IDisposable, IObjectContextAdapter, new() {
        private readonly ConcurrentDictionary<Type, List<object>> _dbSets;

        public DbCacheService(){
            _dbSets = new ConcurrentDictionary<Type, List<object>>();
        }
        public List<T> GetCache<T>() where T : class{
            return _dbSets.ContainsKey(typeof (DbSet<T>)) ? _dbSets[typeof (T)].Select(entry=>(T)entry).ToList() : UpdateCache<T>();
        }
        /// <summary>
        /// updates 
        /// returns a list of all entries of a database store from the database
        /// </summary>
        /// <typeparam name="T">type of the database store</typeparam>
        /// <returns>the</returns>
        public List<T> UpdateCache<T>() where T : class{
            var dbseType = typeof (DbSet<T>);
            using (var db = new TDbCtx())
            {
                var dbset = (from prop in db.GetType().GetProperties() where prop.PropertyType == dbseType select (DbSet<T>)prop.GetValue(db)).FirstOrDefault();
                if (dbset == null) throw new NullReferenceException($"{typeof(TDbCtx)} doesn't contain a property for {typeof(DbSet<T>)}");
                return _dbSets.GetOrAdd(typeof(T), new List<object>(dbset.ToList())).Select(entry => (T)entry).ToList();
            }
        }

        //public void UpdateAllCaches(){
        //    using (var db = new TDbCtx())
        //    {
        //        foreach (var type in _dbSets.Keys) {
        //            var dbset = (from prop in db.GetType().GetProperties() where prop.PropertyType.GetGenericTypeDefinition() == type select prop.GetValue(db)).FirstOrDefault();
        //            _dbSets.GetOrAdd(type, dbset);
        //        }
        //    }
        //}

        public void Commands(CommandGroupBuilder cfg){
            cfg.MinPermissions(Roles.Triumvirate);
            cfg.CreateCommand("reloadcache").Do(m =>{
                //UpdateAllCaches();
                } 
            );
        }

        public void Install(DiscordClient client){
            
        }
    }
}