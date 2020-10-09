using BepInEx.Logging;
using H3MP.Messages;
using H3MP.Networking;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP
{
	public class MessageServerEvents : IServerEvents
	{
		private readonly ManualLogSource _log;
		private readonly MessageDefinition _pong;
		private readonly ConnectionKey _key;

		public MessageServerEvents(ManualLogSource log, MessageDefinition pong, ConnectionKey key)
		{
			_log = log;
			_pong = pong;
			_key = key;
		}

		public void OnConnectionRequest(ConnectionRequest request, NetDataWriter rejectionContent)
		{
			var reader = request.Data;

			if (!reader.TryGet<ConnectionRequestMessage>(out var message))
			{
				_log.LogWarning($"Join request from {request.RemoteEndPoint} had a malformed request.");

				rejectionContent.Put(JoinError.MalformedMessage);
				request.Reject(rejectionContent);
				return;
			}

			if (message.Key != _key)
			{
				_log.LogWarning($"Join request {request.RemoteEndPoint} had an incorrect key.");

				rejectionContent.Put(JoinError.MismatchedKey);
				request.Reject(rejectionContent);
				return;
			}

			var peer = request.Accept();

			using (WriterPool.Instance.Borrow(out var writer))
			{
				_pong.Send(peer, writer, new PongMessage(message.ClientTime));
			}
		}

		public void OnClientConnected(Peer peer)
		{
			// TODO: hook to SteamVR_LoadLevel.Begin(string) and store the last result, + invoke a send if hosting.
			peer.Send(new LevelChangeMessage("IndoorRange"));
		}

		public void OnClientDisconnected(Peer peer, DisconnectInfo info)
		{
		}
	}
}
