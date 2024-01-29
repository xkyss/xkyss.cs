<Query Kind="Program">
  <Namespace>Xunit</Namespace>
</Query>

#load "xunit"

void Main()
{
	RunTests();
}

public class Foo(int x)
{
	public int GetXx() => x + 100;
}

[Fact] public void Test01() 
{
	var foo = new Foo(1);
	System.Console.WriteLine(foo.GetXx());
	Assert.Equal(101, foo.GetXx());
}
