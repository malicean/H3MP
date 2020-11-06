using System;
using System.Collections.Generic;
using System.Linq;
using H3MP.Extensions;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Peers;
using H3MP.Timing;
using H3MP.Utils;
using UnityEngine;

namespace H3MP.Puppetting
{
	public class Puppeteer : IDisposable
	{
		private readonly Client _client;
		private readonly IFrameClock _renderFrame;
		private readonly Option<Puppet>[] _puppets;

		public readonly Transform Root;

		public WorldSnapshotMessage FittedWorld { get; private set; }

		public Puppeteer(Client client, IFrameClock renderFrame)
		{
			_client = client;
			_renderFrame = renderFrame;
			_puppets = new Option<Puppet>[client.MaxPlayers];

			Root = new GameObject("Puppeteer").transform;
			GameObject.DontDestroyOnLoad(Root.gameObject);

			renderFrame.Elapsed += Update;
		}

		private void Update()
		{
			// TODO: remove constant interp delay, implement dynamic interp delay in Client
			var delay = 1 * _client.TickStep;
			var delayed = _renderFrame.Time - delay;
			var timeSnapshots = ((IEnumerable<KeyValuePair<LocalTickstamp, WorldSnapshotMessage>>) _client.Snapshots).Reverse().Select(x => new KeyValuePair<double, WorldSnapshotMessage>(x.Key.Time, x.Value));
			FittedWorld = _client.TimeSnapshotsDataFitter.Fit(timeSnapshots, delayed);

			var players = FittedWorld.PlayerBodies;
			for (var i = 0; i < players.Length; ++i)
			{
				if (i == FittedWorld.PlayerID)
				{
					//continue;
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

			foreach (var puppet in _puppets.WhereSome())
			{
				puppet.RenderUpdate();
			}
		}

		public void Dispose()
		{
			_renderFrame.Elapsed -= Update;

			foreach (var puppet in _puppets.WhereSome())
			{
				puppet.Dispose();
			}

			GameObject.Destroy(Root.gameObject);
		}
	}
}
