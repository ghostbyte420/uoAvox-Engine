using System.Runtime.InteropServices;

namespace UOA_API;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool OnGetPlayerPosition(out int x, out int y, out int z);
