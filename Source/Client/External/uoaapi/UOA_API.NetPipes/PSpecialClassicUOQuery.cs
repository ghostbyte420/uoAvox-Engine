using uoAvox.Network;

namespace UOA_API.NetPipes;

internal class PSpecialUOAQuery : PacketWriter
{
    public PSpecialUOAQuery(SpecialQuery query)
        : base(13)
    {
        WriteByte((byte)query);
    }
}