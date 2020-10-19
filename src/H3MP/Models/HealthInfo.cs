using System;
using H3MP.Utils;

namespace H3MP.Models
{
	internal struct HealthInfo
	{
        public readonly LoopTimer DisplayTimer;

        public readonly SimpleMovingAverage OffsetAbsoluteDeviation;
        public readonly SimpleMovingAverage RttAbsoluteDeviation;

        public uint Sent;
        public uint Received;

        public HealthInfo(double displayInterval, int sampleCount)
        {
            DisplayTimer = new LoopTimer(displayInterval);
            
            OffsetAbsoluteDeviation = new SimpleMovingAverage(0, sampleCount);
            RttAbsoluteDeviation = new SimpleMovingAverage(0, sampleCount);

            Sent = 0;
            Received = 0;
        }
    }
}
