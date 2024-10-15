<Query Kind="Statements">
  <NuGetReference>RazorEngineCore</NuGetReference>
  <Namespace>RazorEngineCore</Namespace>
</Query>

IRazorEngine razorEngine = new RazorEngine();
IRazorEngineCompiledTemplate template = razorEngine.Compile("Hello @Model.Name");

string result = template.Run(new
{
	Name = "Alexander<gfd/>"
});

result.Dump();