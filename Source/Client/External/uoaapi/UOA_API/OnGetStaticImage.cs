using System.Runtime.InteropServices;

namespace UOA_API;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OnGetStaticImage(ushort g, ref ArtInfo art);
