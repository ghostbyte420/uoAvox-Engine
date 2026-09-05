using System.Runtime.InteropServices;

namespace UOA_API;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ArtInfo
{
	public long Address;

	public long Size;

	public long CompressedSize;
}
