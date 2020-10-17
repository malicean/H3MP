using H3MP.Messages;

namespace H3MP
{
	public class Husk
	{
		private bool _moved;
		public Timestamped<PlayerTransformsMessage>? Delta 
		{
			get
			{
				if (!_moved) 
				{
					return null;
				}

				_moved = false;
				return _latest;
			}
		}

		private Timestamped<PlayerTransformsMessage> _latest;
		public Timestamped<PlayerTransformsMessage> Latest 
		{
			get => _latest;
			set
			{
				_moved = true;
				_latest = value;
			}
		}
	}
}