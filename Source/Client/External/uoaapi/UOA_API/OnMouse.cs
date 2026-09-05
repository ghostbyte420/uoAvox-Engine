using System.Runtime.InteropServices;

namespace UOA_API;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OnMouse(int button, int wheel);
