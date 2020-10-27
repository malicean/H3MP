using System;
using System.Collections.Generic;

namespace H3MP.Utils
{
	public readonly struct Entity
	{
		public readonly ushort ID;
		public readonly EntityDefinition Definition;
		public readonly Dictionary<Type, object> Components;

		public Entity(ushort id, EntityDefinition definition, Dictionary<Type, object> components)
		{
			ID = id;
			Definition = definition;
			Components = components;
		}

		public T GetComponent<T>()
		{
			if (!Components.TryGetValue(typeof(T), out var boxed))
			{
				throw new InvalidOperationException("Component not found.");
			}

			return (T) boxed;
		}
	}
}
