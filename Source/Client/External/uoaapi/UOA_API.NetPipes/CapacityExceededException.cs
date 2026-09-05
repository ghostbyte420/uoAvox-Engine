using System;

namespace UOA_API.NetPipes;

[Serializable]
public sealed class CapacityExceededException : Exception
{
	public CapacityExceededException()
		: base("Too much data pending.")
	{
	}
}
