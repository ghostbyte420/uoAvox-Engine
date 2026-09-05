using uoAvox.Network;

namespace UOA_API.NetPipes;

internal class PCastSpell : PacketWriter
{
	public PCastSpell(uint idx)
		: base(11)
	{
		WriteUInt(idx);
	}
}
