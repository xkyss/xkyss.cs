﻿using System.Runtime.InteropServices;

namespace Ks.Exp.StructLayout;

// 32
// => 4 + 4 + 8 + 8 *2
[StructLayout(LayoutKind.Explicit)]
public unsafe struct BaseStruct
{
	[FieldOffset(0x00)]
	public int Field1;

	[FieldOffset(0x04)]
	public int Field2;

	[FieldOffset(0x08)]
	public fixed byte OemName[8];

	[FieldOffset(0x10)]
	[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 2)]
	public SubStruct[] Subs;
}

// 8
// => 4 * 2
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SubStruct
{
	public int Sub1;
	public int Sub2;
}