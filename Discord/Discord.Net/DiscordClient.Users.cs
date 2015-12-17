﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Discord
{
	internal sealed class GlobalUsers : AsyncCollection<long, GlobalUser>
	{
		public GlobalUsers(DiscordClient client, object writerLock)
			: base(client, writerLock) { }

		public GlobalUser GetOrAdd(long id) => GetOrAdd(id, () => new GlobalUser(_client, id));
	}
	internal sealed class Users : AsyncCollection<User.CompositeKey, User>
	{
		public Users(DiscordClient client, object writerLock)
			: base(client, writerLock)
		{ }

		public User this[long userId, long? serverId]
			=> base[new User.CompositeKey(userId, serverId)];
		public User GetOrAdd(long userId, long? serverId)
			=> GetOrAdd(new User.CompositeKey(userId, serverId), () => new User(_client, userId, serverId));
		public User TryRemove(long userId, long? serverId)
			=> TryRemove(new User.CompositeKey(userId, serverId));
	}

	public class UserEventArgs : EventArgs
	{
		public User User { get; }
		public Server Server => User.Server;

		public UserEventArgs(User user) { User = user; }
	}
	public class UserChannelEventArgs : UserEventArgs
	{
		public Channel Channel { get; }

		public UserChannelEventArgs(User user, Channel channel)
			: base(user)
		{
			Channel = channel;
		}
	}
	public class UserIsSpeakingEventArgs : UserChannelEventArgs
	{
		public bool IsSpeaking { get; }

		public UserIsSpeakingEventArgs(User user, Channel channel, bool isSpeaking)
			: base(user, channel)
		{
			IsSpeaking = isSpeaking;
		}
	}
	public class BanEventArgs : EventArgs
	{
		public long UserId { get; }
		public Server Server { get; }

		public BanEventArgs(long userId, Server server)
		{
			UserId = userId;
			Server = server;
		}
	}

	public partial class DiscordClient
	{
		public event EventHandler<UserEventArgs> UserJoined;
		private void RaiseUserJoined(User user)
		{
			if (UserJoined != null)
				RaiseEvent(nameof(UserJoined), () => UserJoined(this, new UserEventArgs(user)));
		}
		public event EventHandler<UserEventArgs> UserLeft;
		private void RaiseUserLeft(User user)
		{
			if (UserLeft != null)
				RaiseEvent(nameof(UserLeft), () => UserLeft(this, new UserEventArgs(user)));
		}
		public event EventHandler<UserEventArgs> UserUpdated;
		private void RaiseUserUpdated(User user)
		{
			if (UserUpdated != null)
				RaiseEvent(nameof(UserUpdated), () => UserUpdated(this, new UserEventArgs(user)));
		}
		public event EventHandler<UserEventArgs> UserPresenceUpdated;
		private void RaiseUserPresenceUpdated(User user)
		{
			if (UserPresenceUpdated != null)
				RaiseEvent(nameof(UserPresenceUpdated), () => UserPresenceUpdated(this, new UserEventArgs(user)));
		}
		public event EventHandler<UserEventArgs> UserVoiceStateUpdated;
		private void RaiseUserVoiceStateUpdated(User user)
		{
			if (UserVoiceStateUpdated != null)
				RaiseEvent(nameof(UserVoiceStateUpdated), () => UserVoiceStateUpdated(this, new UserEventArgs(user)));
		}
		public event EventHandler<UserChannelEventArgs> UserIsTypingUpdated;
		private void RaiseUserIsTyping(User user, Channel channel)
		{
			if (UserIsTypingUpdated != null)
				RaiseEvent(nameof(UserIsTypingUpdated), () => UserIsTypingUpdated(this, new UserChannelEventArgs(user, channel)));
		}
		public event EventHandler<UserIsSpeakingEventArgs> UserIsSpeakingUpdated;
		private void RaiseUserIsSpeaking(User user, Channel channel, bool isSpeaking)
		{
			if (UserIsSpeakingUpdated != null)
				RaiseEvent(nameof(UserIsSpeakingUpdated), () => UserIsSpeakingUpdated(this, new UserIsSpeakingEventArgs(user, channel, isSpeaking)));
		}
		public event EventHandler ProfileUpdated;
		private void RaiseProfileUpdated()
		{
			if (ProfileUpdated != null)
				RaiseEvent(nameof(ProfileUpdated), () => ProfileUpdated(this, EventArgs.Empty));
		}
		public event EventHandler<BanEventArgs> UserBanned;
		private void RaiseUserBanned(long userId, Server server)
		{
			if (UserBanned != null)
				RaiseEvent(nameof(UserBanned), () => UserBanned(this, new BanEventArgs(userId, server)));
		}
		public event EventHandler<BanEventArgs> UserUnbanned;
		private void RaiseUserUnbanned(long userId, Server server)
		{
			if (UserUnbanned != null)
				RaiseEvent(nameof(UserUnbanned), () => UserUnbanned(this, new BanEventArgs(userId, server)));
		}

		/// <summary> Returns the current logged-in user in a private channel. </summary>
		internal User PrivateUser => _privateUser;
		private User _privateUser;

		/// <summary> Returns information about the currently logged-in account. </summary>
		public GlobalUser CurrentUser => _privateUser.GlobalUser;

		/// <summary> Returns a collection of all users this client can currently see. </summary>
		internal GlobalUsers GlobalUsers => _globalUsers;
		private readonly GlobalUsers _globalUsers;

		internal Users Users => _users;
		private readonly Users _users;

		/// <summary> Returns the user with the specified id, along with their server-specific data, or null if none was found. </summary>
		public User GetUser(Server server, long userId)
		{
			if (server == null) throw new ArgumentNullException(nameof(server));
			if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
			CheckReady();

			return _users[userId, server.Id];
		}
		/// <summary> Returns the user with the specified name and discriminator, along withtheir server-specific data, or null if they couldn't be found. </summary>
		/// <remarks> Name formats supported: Name and @Name. Search is case-insensitive. </remarks>
		public User GetUser(Server server, string username, short discriminator)
		{
			if (server == null) throw new ArgumentNullException(nameof(server));
			if (username == null) throw new ArgumentNullException(nameof(username));
			if (discriminator <= 0) throw new ArgumentOutOfRangeException(nameof(discriminator));
			CheckReady();

			return FindUsers(server, username, discriminator, true).FirstOrDefault();
		}

		/// <summary> Returns all users with the specified server and name, along with their server-specific data. </summary>
		/// <remarks> Name formats supported: Name and @Name. Search is case-insensitive.</remarks>
		public IEnumerable<User> FindUsers(Server server, string name, short? discriminator = null, bool exactMatch = false)
		{
			if (server == null) throw new ArgumentNullException(nameof(server));
			if (name == null) throw new ArgumentNullException(nameof(name));
			CheckReady();

			return FindUsers(server.Members, server.Id, name, discriminator, exactMatch);
		}
		/// <summary> Returns all users with the specified channel and name, along with their server-specific data. </summary>
		/// <remarks> Name formats supported: Name and @Name. Search is case-insensitive.</remarks>
		public IEnumerable<User> FindUsers(Channel channel, string name, short? discriminator = null, bool exactMatch = false)
		{
			if (channel == null) throw new ArgumentNullException(nameof(channel));
			if (name == null) throw new ArgumentNullException(nameof(name));
			CheckReady();

			return FindUsers(channel.Members, channel.IsPrivate ? (long?)null : channel.Server.Id, name, discriminator, exactMatch);
        }

		private IEnumerable<User> FindUsers(IEnumerable<User> users, long? serverId, string name, short? discriminator = null, bool exactMatch = false)
		{
			var query = users.Where(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

			if (!exactMatch && name.Length >= 2)
			{
				if (name[0] == '<' && name[1] == '@' && name[name.Length - 1] == '>') //Parse mention
				{
					long id = IdConvert.ToLong(name.Substring(2, name.Length - 3));
					var channel = _users[id, serverId];
					if (channel != null)
						query = query.Concat(new User[] { channel });
				}
				else if (name[0] == '@') //If we somehow get text starting with @ but isn't a mention
				{
					string name2 = name.Substring(1);
					query = query.Concat(users.Where(x => string.Equals(x.Name, name2, StringComparison.OrdinalIgnoreCase)));
				}
			}

			if (discriminator != null)
				query = query.Where(x => x.Discriminator == discriminator.Value);
			return query;
		}

		public Task EditUser(User user, bool? mute = null, bool? deaf = null, IEnumerable<Role> roles = null)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (user.IsPrivate) throw new InvalidOperationException("Unable to edit users in a private channel");
			CheckReady();

			var serverId = user.Server.Id;
            return _api.EditUser(serverId, user.Id, 
				mute: mute, deaf: deaf, 
				roleIds: roles.Select(x => x.Id).Where(x  => x != serverId));
		}

		public Task KickUser(User user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (user.IsPrivate) throw new InvalidOperationException("Unable to kick users from a private channel");
			CheckReady();

			return _api.KickUser(user.Server.Id, user.Id);
		}
		public Task BanUser(User user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (user.IsPrivate) throw new InvalidOperationException("Unable to ban users from a private channel");
			CheckReady();

			return _api.BanUser(user.Server.Id, user.Id);
		}
		public async Task UnbanUser(Server server, long userId)
		{
			if (server == null) throw new ArgumentNullException(nameof(server));
			if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
			CheckReady();

			try { await _api.UnbanUser(server.Id, userId).ConfigureAwait(false); }
			catch (HttpException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
		}

		public async Task<int> PruneUsers(Server server, int days, bool simulate = false)
		{
			if (server == null) throw new ArgumentNullException(nameof(server));
			if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days));
			CheckReady();

			var response = await _api.PruneUsers(server.Id, days, simulate);
			return response.Pruned ?? 0;
		}

		/// <summary>When Config.UseLargeThreshold is enabled, running this command will request the Discord server to provide you with all offline users for a particular server.</summary>
		public void RequestOfflineUsers(Server server)
		{
			if (server == null) throw new ArgumentNullException(nameof(server));

			_dataSocket.SendGetUsers(server.Id);
		}

		public Task EditProfile(string currentPassword = "",
			string username = null, string email = null, string password = null,
			ImageType avatarType = ImageType.Png, byte[] avatar = null)
		{
			if (currentPassword == null) throw new ArgumentNullException(nameof(currentPassword));
			CheckReady();

			return _api.EditUser(currentPassword: currentPassword, 
				username: username ?? _privateUser?.Name,  email: email ?? _privateUser?.GlobalUser.Email, password: password,
				avatarType: avatarType, avatar: avatar);
		}

		public Task SetStatus(UserStatus status)
		{
			if (status == null) throw new ArgumentNullException(nameof(status));
			if (status != UserStatus.Online && status != UserStatus.Idle)
				throw new ArgumentException($"Invalid status, must be {UserStatus.Online} or {UserStatus.Idle}", nameof(status));
			CheckReady();
			
			_status = status;
			return SendStatus();
		}
		public Task SetGame(int? gameId)
		{
			CheckReady();

			_gameId = gameId;
			return SendStatus();
		}
		private Task SendStatus()
		{
			_dataSocket.SendStatus(_status == UserStatus.Idle ? EpochTime.GetMilliseconds() - (10 * 60 * 1000) : (long?)null, _gameId);
			return TaskHelper.CompletedTask;
		}
	}
}