using uoAvox.Network;

namespace UOA_API.NetPipes;

internal class PAttack : PacketWriter
{
	public PAttack(uint serial)
		: base(12)
	{
		WriteUInt(serial);
	}
}
