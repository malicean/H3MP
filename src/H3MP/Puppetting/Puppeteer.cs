using System;
using H3MP.Extensions;
using H3MP.Messages;
using H3MP.Peers;
using H3MP.Timing;
using H3MP.Utils;
using UnityEngine;

namespace H3MP.Puppetting
{
	public class Puppeteer : IRenderUpdatable, IDisposable
	{
		private readonly Client _client;
		private readonly Option<Puppet>[] _puppets;

		public readonly Transform Root;

		public WorldSnapshotMessage FittedWorld { get; private set; }

		public Puppeteer(Client client)
		{
			_client = client;
			_puppets = new Option<Puppet>[client.MaxPlayers];

			Root = new GameObject("Puppeteer").transform;
			GameObject.DontDestroyOnLoad(Root.gameObject);
		}

		public void HandleWorldDelta(DeltaWorldSnapshotMessage delta)
		{
			if (!delta.PlayerBodies.MatchSome(out var players))
			{
				return;
			}

			var localID = _client.TickSnapshots.LastOrNone().Unwrap().Value.PlayerID;

			for (var i = 0; i < players.Length; ++i)
			{
				if (i == localID)
				{
					continue;
				}

				ref var puppet = ref _puppets[i];

				if (players[i].IsSome)
				{
					// none -> some or some -> some
					if (puppet.IsNone)
					{
						puppet = Option.Some(new Puppet(this, i));
					}
				}
				else
				{
					// some -> none or none -> none
					if (puppet.MatchSome(out var puppetValue))
					{
						puppetValue.Dispose();
						puppet = Option.None<Puppet>();
					}
				}
			}
		}

		public void RenderUpdate()
		{
			var now = LocalTime.Now;

			// TODO: remove constant interp delay, implement dynamic interp delay in Client
			var delay = 3 * _client.TickStep;
			FittedWorld = _client.TimeSnapshotsDataFitter.Fit(_client.TimeSnapshots, now - delay);

			foreach (var puppet in _puppets.WhereSome())
			{
				puppet.RenderUpdate();
			}
		}

		public void Dispose()
		{
			GameObject.Destroy(Root.gameObject);
		}
	}
}
