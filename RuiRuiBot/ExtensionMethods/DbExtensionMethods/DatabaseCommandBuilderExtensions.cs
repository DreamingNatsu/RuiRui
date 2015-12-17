using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dba.DAL;
using Discord.Commands;

namespace RuiRuiBot.ExtensionMethods.DbExtensionMethods
{
    public static class DatabaseCommandBuilderExtensions
    {

        public static void Do<T>(this CommandBuilder command, Action<CommandEventArgs, T, DbCtx> action) where T : IDisposable, new()
        {
            command.Do<T, DbCtx>(action);
        }
        public static void Do<T>(this CommandBuilder command, Func<CommandEventArgs, T, DbCtx, Task> action) where T : IDisposable, new()
        {
            command.Do<T,DbCtx>(action);
        }
        public static void Do(this CommandBuilder command, Action<CommandEventArgs, DbCtx> action)
        {
            command.Do<DbCtx>(action);
        }
        public static void Do(this CommandBuilder command, Func<CommandEventArgs, DbCtx, Task> action)
        {
            command.Do<DbCtx>(action);
        }
        public static void Do(this CommandBuilder cb, Func<CommandEventArgs, DbCtx, Task<string>> action, bool isPrivate = false)
        {
            cb.Do<DbCtx>(action, isPrivate);
        }
        public static void Do(this CommandBuilder cb, Func<CommandEventArgs, DbCtx, Task<IEnumerable<string>>> action, bool isPrivate = false)
        {
            cb.Do<DbCtx>(action, isPrivate);
        }
        public static void Do(this CommandBuilder cb, Func<CommandEventArgs, DbCtx, string> action, bool isPrivate = false)
        {
            cb.Do<DbCtx>(action, isPrivate);
        }
        public static void Do(this CommandBuilder cb, Func<CommandEventArgs, DbCtx, IEnumerable<string>> action, bool isPrivate = false)
        {
            cb.Do<DbCtx>(action, isPrivate);
        }
    }
}
