<Query Kind="Statements">
  <NuGetReference>RazorEngineCore</NuGetReference>
  <Namespace>RazorEngineCore</Namespace>
</Query>


//var templatePath = @"D:\Code\thzt\mlcache-doc\deploy\generating\download.ps1";
var templatePath = @"D:\Code\thzt\mlcache-doc\deploy\generating\upload.ps1";
//templatePath.Dump();

var templateContent = File.ReadAllText(templatePath);
//templateContent.Dump();


var razorEngine = new RazorEngine();
var template = razorEngine.Compile(templateContent);

var result = template.Run(new
{
	Username = "appstore",
	HostIp = "192.168.1.144",
	WorkDir = "/home/Ml-Cache",
});

result.Dump();