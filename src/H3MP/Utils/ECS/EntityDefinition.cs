using System;
using System.Collections.Generic;
using System.Linq;

namespace H3MP.Utils
{
	public class EntityDefinition : ISerializer<Entity>
	{
		public readonly ushort ID;
		public readonly Dictionary<Type, object> Components;
		public readonly IComponentSerializer[] Serializers;

		public EntityDefinition(ushort id, Dictionary<Type, object> components, IComponentSerializer[] serializers)
		{
			ID = id;
			Components = components;
			Serializers = serializers;
		}

		public Option<ComponentDefinition<T>> GetComponent<T>()
		{
			if (!Components.TryGetValue(typeof(T), out var boxed))
			{
				return Option.None<ComponentDefinition<T>>();
			}

			return Option.Some((ComponentDefinition<T>) boxed);
		}

		public Entity Deserialize(ref BitPackReader reader)
		{
			var components = new Dictionary<Type, object>();

			var id = PrimitiveSerializers.UShort.Deserialize(ref reader);
			foreach (var serializer in Serializers)
			{
				var type = serializer.Deserialize(ref reader, out var boxed);
				components.Add(type, boxed);
			}

			return new Entity(id, this, components);
		}

		public void Serialize(ref BitPackWriter writer, Entity value)
		{
			PrimitiveSerializers.UShort.Serialize(ref writer, value.ID);
			foreach (var serializer in Serializers)
			{
				serializer.Serialize(ref writer, value.Components);
			}
		}
	}

	public interface IComponentSerializer
	{
		Type Deserialize(ref BitPackReader reader, out object boxed);

		void Serialize(ref BitPackWriter writer, Dictionary<Type, object> components);
	}

	public class EntityDefinitionBuilder
	{
		private readonly Dictionary<Type, object> _components;
		private readonly List<IComponentSerializer> _serializers;

		public readonly ushort ID;

		public EntityDefinitionBuilder(ushort id)
		{
			_components = new Dictionary<Type, object>();

			ID = id;
		}

		public bool AddComponent<TComponent>(ComponentDefinition<TComponent> component)
		{
			if (_components.ContainsKey(typeof(TComponent)))
			{
				return false;
			}

			_components.Add(typeof(TComponent), component);
			_serializers.Add(new PackedSerializableWrapper<TComponent>(component.Serializer));

			return true;
		}

		public EntityDefinition ToDefinition()
		{
			return new EntityDefinition(ID, _components, _serializers.ToArray());
		}

		private class PackedSerializableWrapper<T> : IComponentSerializer
		{
			private readonly ISerializer<T> _serializer;

			public PackedSerializableWrapper(ISerializer<T> serializer)
			{
				_serializer = serializer;
			}

			public Type Deserialize(ref BitPackReader reader, out object boxed)
			{
				boxed = _serializer.Deserialize(ref reader);
				return typeof(T);
			}

			public void Serialize(ref BitPackWriter writer, Dictionary<Type, object> components)
			{
				_serializer.Serialize(ref writer, (T) components[typeof(T)]);
			}
		}
	}
}
