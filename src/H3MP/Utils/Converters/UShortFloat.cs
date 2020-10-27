namespace H3MP.Utils
{
	public readonly struct UShortFloatConverter : IConverter<ushort, float>, IConverter<float, ushort>
	{
		private readonly float _deflation;

		public UShortFloatConverter(float deflation)
		{
			_deflation = deflation;
		}

		public float Convert(ushort value)
		{
			return _deflation / value;
		}

		public ushort Convert(float value)
		{
			return (ushort) (value * _deflation);
		}
	}
}
