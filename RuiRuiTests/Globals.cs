﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuiRuiBot;

namespace RuiRuiTests
{
    [TestClass]
    public class DiscordTest {
        private const int EventTimeout = 5000; //Max time in milliseconds to wait for an event response from our test actions

        protected static DiscordClient HostClient;
        protected static DiscordClient TargetBot;
        protected static DiscordClient ObserverBot;
        protected static Server TestServer;
        protected static Channel TestServerChannel;
        protected static Random Random;

        [ClassInitialize]
        public static void Initialize(params IModule[] modules)
        {
            var settings = Settings.Instance;
            Random = new Random();

            HostClient = new DiscordClient();
            TargetBot = new DiscordClient();
            var commandService = new CommandService(new CommandServiceConfig() { CommandChar = '/', HelpMode = HelpMode.Private });
            var moduleService = new ModuleService();
            var pluginInvokerService = new PluginInvokerService(modules);
            TargetBot.AddService(commandService);
            TargetBot.AddService(moduleService);
            TargetBot.AddService(pluginInvokerService);
            ObserverBot = new DiscordClient();

            HostClient.Connect(settings.User1.Email, settings.User1.Password).Wait();
            TargetBot.Connect(settings.User2.Email, settings.User2.Password).Wait();
            ObserverBot.Connect(settings.User3.Email, settings.User3.Password).Wait();

            //Cleanup existing servers
            WaitMany(
                HostClient.AllServers.Select(x => HostClient.LeaveServer(x)),
                TargetBot.AllServers.Select(x => TargetBot.LeaveServer(x)),
                ObserverBot.AllServers.Select(x => ObserverBot.LeaveServer(x)));

            //Create new server and invite the other bots to it
            TestServer = HostClient.CreateServer("Discord.Net Testing", Region.USEast).Result;
            TestServerChannel = TestServer.DefaultChannel;
            Invite invite = HostClient.CreateInvite(TestServer, 60, 1, false, false).Result;
            WaitAll(
                TargetBot.AcceptInvite(invite),
                ObserverBot.AcceptInvite(invite));
        }
        [ClassCleanup]
        public static void Cleanup()
        {
            WaitMany(
                HostClient.State == DiscordClientState.Connected ? HostClient.AllServers.Select(x => HostClient.LeaveServer(x)) : null,
                TargetBot.State == DiscordClientState.Connected ? TargetBot.AllServers.Select(x => TargetBot.LeaveServer(x)) : null,
                ObserverBot.State == DiscordClientState.Connected ? ObserverBot.AllServers.Select(x => ObserverBot.LeaveServer(x)) : null);

            WaitAll(
                HostClient.Disconnect(),
                TargetBot.Disconnect(),
                ObserverBot.Disconnect());
        }

        internal static void AssertEvent<TArgs>(string msg, Func<Task> action, Action<EventHandler<TArgs>> addEvent, Action<EventHandler<TArgs>> removeEvent, Func<object, TArgs, bool> test = null)
        {
            AssertEvent(msg, action, addEvent, removeEvent, test, true);
        }

        internal static void AssertNoEvent<TArgs>(string msg, Func<Task> action, Action<EventHandler<TArgs>> addEvent, Action<EventHandler<TArgs>> removeEvent, Func<object, TArgs, bool> test = null)
        {
            AssertEvent(msg, action, addEvent, removeEvent, test, false);
        }

        internal static void AssertEvent<TArgs>(string msg, Func<Task> action, Action<EventHandler<TArgs>> addEvent, Action<EventHandler<TArgs>> removeEvent, Func<object, TArgs, bool> test, bool assertTrue)
        {
            ManualResetEventSlim trigger = new ManualResetEventSlim(false);
            bool result = false;

            EventHandler<TArgs> handler = (s, e) =>
            {
                if (test != null)
                {
                    result |= test(s, e);
                    trigger.Set();
                }
                else
                    result = true;
            };

            addEvent(handler);
            var task = action();
            trigger.Wait(EventTimeout);
            task.Wait();
            removeEvent(handler);

            Assert.AreEqual(assertTrue, result, msg);
        }

        internal static void WaitAll(params Task[] tasks)
        {
            Task.WaitAll(tasks);
        }

        internal static void WaitAll(IEnumerable<Task> tasks)
        {
            Task.WaitAll(tasks.ToArray());
        }

        internal static void WaitMany(params IEnumerable<Task>[] tasks)
        {
            Task.WaitAll(tasks.Where(x => x != null).SelectMany(x => x).ToArray());
        }
    }
}