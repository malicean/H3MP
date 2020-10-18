using System;
using BepInEx.Logging;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Utils;
using UnityEngine;

namespace H3MP
{
	public class Puppet : IRenderUpdatable, IDisposable
	{
		// Should slowly shrink to provide better reliability.
		private const double INTERP_DELAY_EMA_ALPHA = 1d / 10;

		private readonly ManualLogSource _log;

		private readonly GameObject _head;
		private readonly GameObject _handLeft;
		private readonly GameObject _handRight;

		private readonly Func<ServerTime> _timeGetter;
		private readonly ExponentialMovingAverage _interpDelay;
		private readonly Snapshots<PlayerTransformsMessage> _snapshots;

		private ServerTime Time => _timeGetter();

		internal Puppet(ManualLogSource log, Func<ServerTime> timeGetter)
		{
			_log = log;

			// Unity objects
			_head = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_handLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_handRight = GameObject.CreatePrimitive(PrimitiveType.Cube);

			// Parents
			_handLeft.transform.parent = _head.transform;
			_handRight.transform.parent = _head.transform;

			// Trans-scene
			GameObject.DontDestroyOnLoad(_head);

			// Remove colliders
			GameObject.Destroy(_head.GetComponent<Collider>());
			GameObject.Destroy(_handLeft.GetComponent<Collider>());
			GameObject.Destroy(_handRight.GetComponent<Collider>());

			// Visuals
			_head.transform.localScale = Vector3.one * 0.1f;

			var baseMat = _head.GetComponent<MeshRenderer>().material;
			var matLeft = new Material(baseMat);
			var matRight = new Material(baseMat);
			
			matLeft.color = Color.red;
			matRight.color = Color.blue;
			
			_handLeft.GetComponent<MeshRenderer>().material = matLeft;
			_handRight.GetComponent<MeshRenderer>().material = matRight;

			// .NET objects
			_timeGetter = timeGetter;
			_interpDelay = new ExponentialMovingAverage(0.1, INTERP_DELAY_EMA_ALPHA);
			var killer = new TimeSnapshotKiller<PlayerTransformsMessage>(() => Time.Now, 5);
			_snapshots = new Snapshots<PlayerTransformsMessage>(killer);
		}

		public void ProcessTransforms(Timestamped<PlayerTransformsMessage> message)
		{
			var messageDelay = Time.Now - message.Timestamp;
			var interpDelay = _interpDelay.Value;

			if (messageDelay > interpDelay)
			{
				_interpDelay.Reset(messageDelay);
				_log.LogDebug($"A puppet's interpolation delay jumped ({interpDelay} -> {messageDelay})");
			}
			else
			{
				_interpDelay.Push(messageDelay);
			}

			_snapshots.Push(message.Timestamp, message.Content);
		}

		public void RenderUpdate()
		{
			var time = Time;
			if (time is null)
			{
				return;
			}

			var snapshot = _snapshots[time.Now - _interpDelay.Value];

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