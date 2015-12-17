﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Tests
{
	[TestClass]
	public class Tests
	{
		private const int EventTimeout = 5000; //Max time in milliseconds to wait for an event response from our test actions
		
        private static DiscordClient _hostClient, _targetBot, _observerBot;
		private static Server _testServer;
		private static Channel _testServerChannel;
		private static Random _random;

		[ClassInitialize]
		public static void Initialize(TestContext testContext)
		{
			var settings = Settings.Instance;
			_random = new Random();

			_hostClient = new DiscordClient();
			_targetBot = new DiscordClient();
			_observerBot = new DiscordClient();

			_hostClient.Connect(settings.User1.Email, settings.User1.Password).Wait();
			_targetBot.Connect(settings.User2.Email, settings.User2.Password).Wait();
			_observerBot.Connect(settings.User3.Email, settings.User3.Password).Wait();

			//Cleanup existing servers
			WaitMany(
				_hostClient.AllServers.Select(x => _hostClient.LeaveServer(x)),
				_targetBot.AllServers.Select(x => _targetBot.LeaveServer(x)),
				_observerBot.AllServers.Select(x => _observerBot.LeaveServer(x)));

			//Create new server and invite the other bots to it
			_testServer = _hostClient.CreateServer("Discord.Net Testing", Region.USEast).Result;
			_testServerChannel = _testServer.DefaultChannel;
			Invite invite = _hostClient.CreateInvite(_testServer, 60, 1, false, false).Result;
			WaitAll(
				_targetBot.AcceptInvite(invite),
				_observerBot.AcceptInvite(invite));
		}

		//Channels
		[TestMethod]
		public void TestCreateTextChannel()
			=> TestCreateChannel(ChannelType.Text);
		[TestMethod]
		public void TestCreateVoiceChannel()
			=> TestCreateChannel(ChannelType.Voice);
		private void TestCreateChannel(ChannelType type)
		{
			Channel channel = null;
			string name = $"#test_{_random.Next()}";
			AssertEvent<ChannelEventArgs>(
				"ChannelCreated event never received",
				async () => channel = await _hostClient.CreateChannel(_testServer, name.Substring(1), type),
				x => _targetBot.ChannelCreated += x,
				x => _targetBot.ChannelCreated -= x,
				(s, e) => e.Channel.Name == name);

			AssertEvent<ChannelEventArgs>(
				"ChannelDestroyed event never received",
				async () => await _hostClient.DeleteChannel(channel),
				x => _targetBot.ChannelDestroyed += x,
				x => _targetBot.ChannelDestroyed -= x,
				(s, e) => e.Channel.Name == name);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task TestCreateChannel_NoName()
		{
			await _hostClient.CreateChannel(_testServer, $"", ChannelType.Text);
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task TestCreateChannel_NoType()
		{
			string name = $"#test_{_random.Next()}";
			await _hostClient.CreateChannel(_testServer, $"", ChannelType.FromString(""));
		}
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task TestCreateChannel_BadType()
		{
			string name = $"#test_{_random.Next()}";
			await _hostClient.CreateChannel(_testServer, $"", ChannelType.FromString("badtype"));
		}

		//Messages
		[TestMethod]
		public void TestSendMessage()
		{
			string text = $"test_{_random.Next()}";
			AssertEvent<MessageEventArgs>(
				"MessageCreated event never received",
				() => _hostClient.SendMessage(_testServerChannel, text),
				x => _targetBot.MessageReceived += x,
				x => _targetBot.MessageReceived -= x,
				(s, e) => e.Message.Text == text);
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			WaitMany(
				_hostClient.State == DiscordClientState.Connected ? _hostClient.AllServers.Select(x => _hostClient.LeaveServer(x)) : null,
				_targetBot.State == DiscordClientState.Connected ? _targetBot.AllServers.Select(x => _targetBot.LeaveServer(x)) : null,
				_observerBot.State == DiscordClientState.Connected ? _observerBot.AllServers.Select(x => _observerBot.LeaveServer(x)) : null);

			WaitAll(
				_hostClient.Disconnect(),
				_targetBot.Disconnect(),
				_observerBot.Disconnect());
		}

		private static void AssertEvent<TArgs>(string msg, Func<Task> action, Action<EventHandler<TArgs>> addEvent, Action<EventHandler<TArgs>> removeEvent, Func<object, TArgs, bool> test = null)
		{
			AssertEvent(msg, action, addEvent, removeEvent, test, true);
		}
		private static void AssertNoEvent<TArgs>(string msg, Func<Task> action, Action<EventHandler<TArgs>> addEvent, Action<EventHandler<TArgs>> removeEvent, Func<object, TArgs, bool> test = null)
		{
			AssertEvent(msg, action, addEvent, removeEvent, test, false);
        }
		private static void AssertEvent<TArgs>(string msg, Func<Task> action, Action<EventHandler<TArgs>> addEvent, Action<EventHandler<TArgs>> removeEvent, Func<object, TArgs, bool> test, bool assertTrue)
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

		private static void WaitAll(params Task[] tasks)
		{
			Task.WaitAll(tasks);
		}
		private static void WaitAll(IEnumerable<Task> tasks)
		{
			Task.WaitAll(tasks.ToArray());
		}

	    public static void WaitMany(params IEnumerable<Task>[] tasks)
		{
			Task.WaitAll(tasks.Where(x => x != null).SelectMany(x => x).ToArray());
		}
	}
}
