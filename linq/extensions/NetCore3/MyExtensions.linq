<Query Kind="Program" />

void Main()
{
	// Write code to test your extensions here. Press F5 to compile and run.
}

public static class MyExtensions
{
	// Write custom extension methods here. They will be available to all queries.
	public static string ToHexString(this byte[] byteList)
	{
		return string.Concat(byteList.Select(temp => temp.ToString("x2")));
	}

}

// You can also define namespaces, non-static classes, enums, etc.

#region Advanced - How to multi-target

// The NETx symbol is active when a query runs under .NET x or later.
// (LINQPad also recognizes NETx_0_OR_GREATER in case you enjoy typing.)

#if NET8
// Code that requires .NET 8 or later
#endif

#if NET7
// Code that requires .NET 7 or later
#endif

#if NET6
// Code that requires .NET 6 or later
#endif

#if NETCORE
// Code that requires .NET Core or later
#else
// Code that runs under .NET Framework (LINQPad 5)
#endif

#endregion