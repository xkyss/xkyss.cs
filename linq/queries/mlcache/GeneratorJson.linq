<Query Kind="Statements">
  <NuGetReference>RazorEngineCore</NuGetReference>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>RazorEngineCore</Namespace>
</Query>

// 读取Json为Model

var jsonString = """
{
	"Username": "appstore",
	"HostIp": "192.168.1.144",
	"BaseIp": "192.168.1.144",
	"HostShort": "144",
	"WorkDir": "/home/Ml-Cache",
	"Version": "1.1.7",
	"Platform": "amd64"
}
""";

var templateContent = """
@{
    string message = "foreignObject example with Scalable Vector Graphics (SVG)";
}
Hello @Model["Username"]! and -@Model["Platform"] @message
""";


var node = JsonNode.Parse(jsonString);


var razorEngine = new RazorEngine();
var template = razorEngine.Compile(templateContent);

var result = template.Run(node);

result.Dump();



var datesNode = JsonNode.Parse(@"[""2019-08-01T00:00:00"",""2019-08-02T00:00:00""]");
datesNode[0].GetValue<DateTime>();

var template2 = razorEngine.Compile(@"Hello @Model[0].GetValue<DateTime>()", builder =>
{
	builder.AddAssemblyReferenceByName("System.Text.Json");
});

var result2 = template2.Run(datesNode);

result2.Dump();