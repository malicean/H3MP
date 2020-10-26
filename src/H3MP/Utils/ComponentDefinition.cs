namespace H3MP.Utils
{
	public class ComponentDefinition<T>
	{
		public readonly ISerializer<T> Serializer;

		public ComponentDefinition(ISerializer<T> serializer)
		{
			Serializer = serializer;
		}
	}
}
