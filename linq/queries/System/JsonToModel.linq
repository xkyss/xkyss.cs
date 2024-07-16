<Query Kind="Statements">
  <Namespace>System.Text.Json.Nodes</Namespace>
</Query>

// Json 转换为 Model

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

var node = JsonNode.Parse(jsonString);
node.Dump();
//node.ToJsonString().Dump();

//System.Console.WriteLine(node["Username"]);

