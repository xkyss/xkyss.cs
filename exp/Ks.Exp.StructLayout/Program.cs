

using System.Runtime.InteropServices;
using Ks.Exp.StructLayout;


var size1 = Marshal.SizeOf(typeof(BaseStruct));
Console.WriteLine($"BaseStruct Size={size1}");

var size2 = Marshal.SizeOf(typeof(SubStruct));
Console.WriteLine($"SubStruct Size={size2}");

var ret = new BaseStruct();
unsafe
{
	var p1 = &ret;
	var p2 = &(ret.Field1);
}