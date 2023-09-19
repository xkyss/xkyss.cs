<Query Kind="Statements" />


// 十进制
{
	var input = "0xa0712d68000000000000000000000000000000000000000000000000000000000000000a";
	var m = Regex.Match(input, @"^0x([a-zA-Z0-9]{8})(0{62})([a-zA-Z0-9]{2})$");
	System.Console.WriteLine(m.Success);
	System.Console.WriteLine(m.Groups[3]);
}


// 十六进制
{
	var input = "0xa0712d68000000000000000000000000000000000000000000000000000000000000000a";
	var m = Regex.Match(input, @"^0x([a-zA-Z0-9]{8})(0{62})([a-zA-Z0-9]{2})$");
	var p = int.TryParse(m.Groups[3].Value, NumberStyles.HexNumber, null, out int amount);
}