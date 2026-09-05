using System.Runtime.InteropServices;

namespace UOA_API;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OnCastSpell(int idx);
