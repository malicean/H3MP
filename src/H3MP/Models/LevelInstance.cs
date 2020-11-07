using System;

namespace H3MP.Models
{
	public readonly struct LevelInstance : IEquatable<LevelInstance>
	{
		public readonly ushort ID;
		public readonly string Name;

		public LevelInstance(ushort id, string name)
		{
			ID = id;
			Name = name;
		}

		public LevelInstance Next(string name)
		{
			return new LevelInstance((ushort) (ID + 1), name);
		}

		public bool Equals(LevelInstance other)
		{
			return ID == other.ID && Name == other.Name;
		}
	}
}
