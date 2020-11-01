using System;
using H3MP.Messages;
using H3MP.Timing;
using UnityEngine;

namespace H3MP.Puppetting
{
	public class Puppet : IRenderUpdatable, IDisposable
	{
		private readonly Puppeteer _puppeteer;
		private readonly int _id;

		private readonly Transform _root;
		private readonly Transform _head;
		private readonly Transform _handLeft;
		private readonly Transform _handRight;

		public Puppet(Puppeteer puppeteer, int id)
		{
			_puppeteer = puppeteer;
			_id = id;

			_root = new GameObject("Puppet " + _id).transform;
			_head = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
			_handLeft = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
			_handRight = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;

			_head.parent = _root;
			_handLeft.parent = _root;
			_handRight.parent = _root;
		}

		public void RenderUpdate()
		{
			var player = _puppeteer.FittedWorld.PlayerBodies[_id].Unwrap();

			void SetTransform(Transform transform, TransformMessage data)
			{
				transform.localPosition = data.Position;
				transform.localRotation = data.Rotation;
			}

			_root.position = player.Root.Position;
			_root.rotation = player.Root.Rotation;
			SetTransform(_head, player.Head);
			SetTransform(_handLeft, player.HandLeft);
			SetTransform(_handRight, player.HandRight);
		}

		public void Dispose()
		{
			GameObject.Destroy(_root.gameObject);
		}
	}
}
