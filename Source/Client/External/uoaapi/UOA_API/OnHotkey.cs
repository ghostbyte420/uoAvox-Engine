using System.Runtime.InteropServices;

namespace UOA_API;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool OnHotkey(int key, int mod, bool pressed);
