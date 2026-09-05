using System.Runtime.InteropServices;

namespace UOA_API;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool OnPacketSendRecv(ref byte[] data, ref int length);
