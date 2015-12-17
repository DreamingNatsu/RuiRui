using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Modules;

namespace RuiRuiBot.ExtensionMethods
{
    public static class CommandBuilderExtension
    {
        public static void CreateCommands(this ModuleManager cgb, Action<CommandGroupBuilder> action)
        {
            cgb.CreateCommands("",action);
        }

        public static CommandBuilder CreateCommand(this ModuleManager modman, string cmd){
           return modman.Client.GetService<CommandService>().CreateCommand(cmd).AddCheck(new ModuleChecker(modman));
        }



        public static void Do<T>(this CommandBuilder command, Action<CommandEventArgs, T> action) where T : IDisposable, new()
        {
            Action<CommandEventArgs> d = m =>
            {
                using (var db = new T())
                {
                    action.Invoke(m, db);
                }
            };
            command.Do(d);
        }
        public static void Do<T>(this CommandBuilder command, Func<CommandEventArgs, T, Task> action) where T : IDisposable, new()
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                using (var db = new T())
                {
                    await action.Invoke(m, db);
                }
            };
            command.Do(d);
        }
        public static void Do<T1,T2>(this CommandBuilder command, Action<CommandEventArgs, T1,T2> action) where T1 : IDisposable, new() where T2 : IDisposable, new()
        {
            Action<CommandEventArgs> d = m =>
            {
                using (var t1 = new T1())
                {
                    using (var t2 = new T2())
                    {
                        action.Invoke(m,t1,t2);
                    }
                }
            };
            command.Do(d);
        }
        public static void Do<T1,T2>(this CommandBuilder command, Func<CommandEventArgs, T1,T2, Task> action) where T1: IDisposable, new() where T2 : IDisposable, new()
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                using (var t1 = new T1())
                {
                    using (var t2 = new T2())
                    {
                        await action.Invoke(m, t1,t2);
                    }
                }
            };
            command.Do(d);
        }

        public static void Do(this CommandBuilder cb, Func<CommandEventArgs, Task<string>> action, bool isPrivate = false)
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                var v = await action.Invoke(m);
                if (isPrivate)
                {
                    await cb.Service.Client.SendBigMessage(m.User, v);
                }
                else
                {
                    await cb.Service.Client.SendBigMessage(m.Channel, v); 
                }
                
            };
            cb.Do(d);
        }

        /// <summary>
        /// Extension on CommandBuilder.Do(...) where the returned value will be outputted, will inject a disposable instance of T
        /// </summary>
        /// <typeparam name="T">Disposable type of which an instance will be created</typeparam>
        /// <param name="cb"></param>
        /// <param name="action">the delegate action to be taken when the command is invoked</param>
        /// <param name="isPrivate">if true the output will be sent in a private message to the user that invoked the command</param>
        public static void Do<T>(this CommandBuilder cb, Func<CommandEventArgs,T, Task<string>> action, bool isPrivate = false) where T : IDisposable, new()
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                using (var t = new T())
                {
                    var v = await action.Invoke(m,t);
                    if (isPrivate)
                    {
                        await cb.Service.Client.SendBigMessage(m.User, v);
                    }
                    else
                    {
                        await cb.Service.Client.SendBigMessage(m.Channel, v);
                    }
                }
            };
            cb.Do(d);
        }

        /// <summary>
        /// Extension on CommandBuilder.Do(...) where the returned value will be outputted
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="action">the delegate action to be taken when the command is invoked</param>
        /// <param name="isPrivate">if true the output will be sent in a private message to the user that invoked the command</param>
        public static void Do(this CommandBuilder cb, Func<CommandEventArgs, Task<IEnumerable<string>>> action, bool isPrivate = false)
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                var v = await action.Invoke(m);
                if (isPrivate)
                {
                    await cb.Service.Client.SendBigMessage(m.User, v);
                }
                else
                {
                    await cb.Service.Client.SendBigMessage(m.Channel, v);
                }

            };
            cb.Do(d);
        }

        /// <summary>
        /// Extension on CommandBuilder.Do(...) where the returned value will be outputted, will inject a disposable instance of T
        /// </summary>
        /// <typeparam name="T">Disposable type of which an instance will be created</typeparam>
        /// <param name="cb"></param>
        /// <param name="action">the delegate action to be taken when the command is invoked</param>
        /// <param name="isPrivate">if true the output will be sent in a private message to the user that invoked the command</param>
        public static void Do<T>(this CommandBuilder cb, Func<CommandEventArgs, T, Task<IEnumerable<string>>> action, bool isPrivate = false) where T : IDisposable, new()
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                using (var t = new T())
                {
                    var v = await action.Invoke(m, t);
                    if (isPrivate)
                    {
                        await cb.Service.Client.SendBigMessage(m.User, v);
                    }
                    else
                    {
                        await cb.Service.Client.SendBigMessage(m.Channel, v);
                    }
                }
            };
            cb.Do(d);
        }

        /// <summary>
        /// Extension on CommandBuilder.Do(...) where the returned value will be outputted
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="action">the delegate action to be taken when the command is invoked</param>
        /// <param name="isPrivate">if true the output will be sent in a private message to the user that invoked the command</param>
        public static void Do(this CommandBuilder cb, Func<CommandEventArgs, string> action, bool isPrivate = false)
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                var v = action.Invoke(m);
                if (isPrivate)
                {
                    await cb.Service.Client.SendBigMessage(m.User, v);
                }
                else
                {
                    await cb.Service.Client.SendBigMessage(m.Channel, v);
                }

            };
            cb.Do(d);
        }

        /// <summary>
        /// Extension on CommandBuilder.Do(...) where the returned value will be outputted, will inject a disposable instance of T
        /// </summary>
        /// <typeparam name="T">Disposable type of which an instance will be created</typeparam>
        /// <param name="cb"></param>
        /// <param name="action">the delegate action to be taken when the command is invoked</param>
        /// <param name="isPrivate">if true the output will be sent in a private message to the user that invoked the command</param>
        public static void Do<T>(this CommandBuilder cb, Func<CommandEventArgs, T, string> action, bool isPrivate = false) where T : IDisposable, new()
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                using (var t = new T())
                {
                    var v = action.Invoke(m, t);
                    if (isPrivate)
                    {
                        await cb.Service.Client.SendBigMessage(m.User, v);
                    }
                    else
                    {
                        await cb.Service.Client.SendBigMessage(m.Channel, v);
                    }
                }
            };
            cb.Do(d);
        }

        /// <summary>
        /// Extension on CommandBuilder.Do(...) where the returned value will be outputted
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="action">the delegate action to be taken when the command is invoked</param>
        /// <param name="isPrivate">if true the output will be sent in a private message to the user that invoked the command</param>
        public static void Do(this CommandBuilder cb, Func<CommandEventArgs, IEnumerable<string>> action, bool isPrivate = false)
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                var v = action.Invoke(m);
                if (isPrivate)
                {
                    await cb.Service.Client.SendBigMessage(m.User, v);
                }
                else
                {
                    await cb.Service.Client.SendBigMessage(m.Channel, v);
                }

            };
            cb.Do(d);
        }

        /// <summary>
        /// Extension on CommandBuilder.Do(...) where the returned value will be outputted, will inject a disposable instance of T
        /// </summary>
        /// <typeparam name="T">Disposable type of which an instance will be created</typeparam>
        /// <param name="cb"></param>
        /// <param name="action">the delegate action to be taken when the command is invoked</param>
        /// <param name="isPrivate">if true the output will be sent in a private message to the user that invoked the command</param>
        public static void Do<T>(this CommandBuilder cb, Func<CommandEventArgs, T, IEnumerable<string>> action, bool isPrivate = false) where T : IDisposable, new()
        {
            Func<CommandEventArgs, Task> d = async m =>
            {
                using (var t = new T())
                {
                    var v = action.Invoke(m, t);
                    if (isPrivate)
                    {
                        await cb.Service.Client.SendBigMessage(m.User, v);
                    }
                    else
                    {
                        await cb.Service.Client.SendBigMessage(m.Channel, v);
                    }
                }
            };
            cb.Do(d);
        }
    }
    


    internal class ExceptionBehaviorNotFoundException : Exception
    {
        public ExceptionBehaviorNotFoundException(string message,Exception ex):base(message,ex)
        {
        }
    }
}
