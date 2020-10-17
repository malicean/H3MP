namespace H3MP.Networking
{
	public enum ConnectionError : byte
	{
		InternalError,
		MalformedVersion,
		MismatchedVersion,
		UserDefined
	}
}
