namespace H3MP.Models
{
	public enum ConnectionError : byte
	{
		InternalError,
		MalformedVersion,
		MismatchedVersion,
		UserDefined
	}
}
