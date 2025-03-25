<Query Kind="Statements">
  <NuGetReference>CliWrap</NuGetReference>
  <Namespace>CliWrap</Namespace>
  <Namespace>CliWrap.Buffered</Namespace>
  <Namespace>CliWrap.EventStream</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>


await using var stdOut = Console.OpenStandardOutput();
await using var stdErr = Console.OpenStandardError();

var cmd = Cli.Wrap("cmd")
	.WithArguments("/c dir")
	.WithStandardOutputPipe(PipeTarget.ToStream(stdOut), true)
	.WithStandardErrorPipe(PipeTarget.ToStream(stdErr), true);
await cmd.ExecuteAsync();
