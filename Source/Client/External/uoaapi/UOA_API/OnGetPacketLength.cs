using System.Runtime.InteropServices;

namespace UOA_API;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate short OnGetPacketLength(int id);
