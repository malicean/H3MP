using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace H3MP.Networking.Implementations
{
	public class MessageServerListener : MessageListener
	{
		private readonly IMessageServerEvents _events;
		private readonly Version _version;

		public MessageServerListener(ManualLogSource log, Dictionary<Type, MessageDefinition> messageDefinitions, IMessageServerEvents events, Version version) : base(log, messageDefinitions)
		{
			_events = events;
			_version = version;
		}

		private void OnConnectionRequestSafe(ConnectionRequest request, NetDataWriter writer)
		{
			var reader = request.Data;

			Version version;
			try
			{
				version = reader.GetVersion();
			}
			catch
			{
				Log.LogWarning($"Connection request from {request.RemoteEndPoint} denied because of a malformed version.");

				writer.Put(ConnectionError.MalformedVersion);
				request.Reject(writer);

				return;
			}

			// Build/revision doesn't matter
			if (version.Major != _version.Major || version.Minor != _version.Minor)
			{
				Log.LogWarning($"Connection request from {request.RemoteEndPoint} denied because of version mismatch (server: {_version}; client: {version})");

				writer.Put(ConnectionError.MismatchedVersion);
				writer.Put(new MismatchedVersionErrorMessage(_version, version));
				request.Reject(writer);

				return;
			}
		}

		public override void OnConnectionRequest(ConnectionRequest request)
		{
			using (WriterPool.Instance.Borrow(out var writer))
			{
				try
				{
					OnConnectionRequestSafe(request, writer);
				}
				catch (Exception e)
				{
					Log.LogError($"Failed to process connection request from {request.RemoteEndPoint}:\n{e}");

					writer.Reset();
					writer.Put(ConnectionError.InternalError);

					request.RejectForce(writer);

					return;
				}

				writer.Reset();
				writer.Put(ConnectionError.UserDefined);

				_events.OnConnectionRequest(request, writer);
			}
		}

		protected override void OnPeerConnected(MessagePeer peer)
		{
			_events.OnPeerConnected(peer);
		}

		protected override void OnPeerDisconnected(MessagePeer peer, DisconnectInfo info)
		{
			_events.OnPeerDisconnected(peer, info);
		}
	}
}
