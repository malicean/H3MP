using H3MP.Messages;
using H3MP.Utils;

namespace H3MP.Models
{
	public class InputState
	{
		public SetterDirty<string> Level { get; }

		public SetterDirty<MoveMessage?> MoveDelta { get; }
	}
}
