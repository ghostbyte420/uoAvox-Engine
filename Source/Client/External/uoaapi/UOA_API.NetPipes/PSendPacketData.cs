using uoAvox.Network;

namespace UOA_API.NetPipes;

internal class PSendPacketData : PacketWriter
{
	public PSendPacketData(bool toUOA, in byte[] data, int length, bool isdynamic)
		: base(10)
	{
		WriteBool(toUOA);
		WriteBool(isdynamic);
		WriteUShort((ushort)length);
		WriteBytes(data, 0, length);
	}
}
