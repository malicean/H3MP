using System;
using H3MP.IO;

namespace H3MP.Serialization
{
    public class VersionSerializer : ISerializer<Version>
    {
		private readonly ISerializer<int> _component;

		public VersionSerializer()
		{
			_component = PrimitiveSerializers.Int;
		}

        public Version Deserialize(ref BitPackReader reader)
        {
            var major = _component.Deserialize(ref reader);
			var minor = _component.Deserialize(ref reader);
			var build = _component.Deserialize(ref reader);
			var revision = _component.Deserialize(ref reader);

			if (build == -1)
			{
				return new Version(major, minor);
			}

			if (revision == -1)
			{
				return new Version(major, minor, build);
			}

			return new Version(major, minor, build, revision);
        }

        public void Serialize(ref BitPackWriter writer, Version value)
        {
            _component.Serialize(ref writer, value.Major);
			_component.Serialize(ref writer, value.Minor);
			_component.Serialize(ref writer, value.Build);
			_component.Serialize(ref writer, value.Revision);
        }
    }
}
