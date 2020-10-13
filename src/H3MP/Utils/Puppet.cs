using H3MP.Messages;
using H3MP.Models;
using H3MP.Utils;
using UnityEngine;

namespace H3MP
{
    public class Puppet : IRenderUpdatable
    {
        private readonly Transform _head;
        private readonly Transform _handLeft;
        private readonly Transform _handRight;

        private readonly ServerTime _time;
        private readonly Snapshots<PlayerTransformsMessage> _snapshots;

        public double Interp { get; set; } = 0.1;

        internal Puppet(ServerTime time)
        {
            _head = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            _handLeft = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            _handRight = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;

            _handLeft.parent = _head;
            _handRight.parent = _head;

            var killer = new TimeSnapshotKiller<PlayerTransformsMessage>(() => time.Now, 1);
            _snapshots = new Snapshots<PlayerTransformsMessage>(killer);
        }

        public void ProcessTransforms(Timestamped<PlayerTransformsMessage> message)
        {
            _snapshots.Push(message.Timestamp, message.Content);
        }

        public void RenderUpdate()
        {
            var snapshot = _snapshots[_time.Now - Interp];

            snapshot.Head.Apply(_head);
            snapshot.HandLeft.Apply(_head);
            snapshot.HandRight.Apply(_head);
        }
    }
}