using System;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Utils;
using UnityEngine;

namespace H3MP
{
	public class Puppet : IRenderUpdatable, IDisposable
	{
		private readonly GameObject _head;
		private readonly GameObject _handLeft;
		private readonly GameObject _handRight;

		private readonly ServerTime _time;
		private readonly Snapshots<PlayerTransformsMessage> _snapshots;

		public double Interp { get; set; } = 0.1;

		internal Puppet(ServerTime time)
		{
			_head = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_handLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_handRight = GameObject.CreatePrimitive(PrimitiveType.Cube);

			_handLeft.transform.parent = _head.transform;
			_handRight.transform.parent = _head.transform;

			_time = time;
			var killer = new TimeSnapshotKiller<PlayerTransformsMessage>(() => _time.Now, 1);
			_snapshots = new Snapshots<PlayerTransformsMessage>(killer);
		}

		public void ProcessTransforms(Timestamped<PlayerTransformsMessage> message)
		{
			_snapshots.Push(message.Timestamp, message.Content);
		}

		public void RenderUpdate()
		{
			var snapshot = _snapshots[_time.Now - Interp];

			snapshot.Head.Apply(_head.transform);
			snapshot.HandLeft.Apply(_handLeft.transform);
			snapshot.HandRight.Apply(_handRight.transform);
		}

		public void Dispose()
		{
			GameObject.Destroy(_head);
		}
	}
}