<Query Kind="Program">
  <Namespace>Xunit</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

// 使用 System.Text.Json 序列化派生类的属性
// https://learn.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0

#load "xunit"


void Main()
{
	RunTests();
}

public class Tester
{
	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
	    WriteIndented = true
	};

	/// <summary>
	/// 没有指定子类类型,只会反序列化为父类
	/// </summary>
	[Fact]
	public void Test01()
	{
		var value = JsonSerializer.Deserialize<WeatherForecastBase>("""
	    {
	      "City": "Milwaukee",
	      "Date": "2022-09-26T00:00:00-05:00",
	      "TemperatureCelsius": 15,
	      "Summary": "Cool"
	    }
	    """);
		Assert.False(value is WeatherForecastWithCity);
	}
	
	[Fact]
	public void Test02()
	{
		var value = JsonSerializer.Deserialize<WeatherForecastBase>("""
	    {
	      "$type" : "withCity",
	      "City": "Milwaukee",
	      "Date": "2022-09-26T00:00:00-05:00",
	      "TemperatureCelsius": 15,
	      "Summary": "Cool"
	    }
	    """);

		Assert.True(value is WeatherForecastWithCity);
	}
}

#region Models
[JsonDerivedType(typeof(WeatherForecastBase), typeDiscriminator: "base")]
[JsonDerivedType(typeof(WeatherForecastWithCity), typeDiscriminator: "withCity")]
public class WeatherForecastBase
{
	public DateTimeOffset Date { get; set; }
	public int TemperatureCelsius { get; set; }
	public string? Summary { get; set; }
}
public class WeatherForecastWithCity : WeatherForecastBase
{
	public string? City { get; set; }
}

#endregion