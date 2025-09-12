<Query Kind="Program">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>


private static readonly HttpClient httpClient = new HttpClient();

public static async Task Main()
{
	//await Sync();
	await Async();
	
	//await LoginAsync(0);
}
/// <summary>并发</summary>
public static async Task Async()
{
	var tasks = new List<Task>();

	for (var i = 0; i < 10; i++)
	{
		tasks.Add(GetAllAsync(i));
	}

	await Task.WhenAll(tasks);
}

/// <summary>顺发</summary>
public static async Task Sync()
{
	for (var i = 0; i < 10; i++)
	{
		await GetAllAsync(i);
	}
}

private static async Task GetAllAsync(int index)
{
	var request = new HttpRequestMessage(HttpMethod.Get, "http://192.168.1.144/api/app/appLabel/getAll");
	request.Headers.Accept.ParseAdd("application/json, text/plain, */*");
	request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
	request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzUxMiJ9.eyJ1c2VyX2lkIjoxLCJ1c2VyX2tleSI6ImM2ZmIwOTVhLTQ3NDItNGVkNi1hYzkyLWU4NzdkNmYzOWJjNiIsInVzZXJuYW1lIjoiYWRtaW4ifQ.tbvOYh92PmF_KyKFObIobNIc0N4EGbXs43UwRrEOpxnl9ytstksJhrwoGesoRTJZ8tGfbB-i8fQnjWf70mWZyQ");
	request.Headers.Connection.ParseAdd("keep-alive");
	request.Headers.Referrer = new Uri("http://192.168.1.144/index");
	request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36 Edg/140.0.0.0");

	using var response = await httpClient.SendAsync(request);
	response.EnsureSuccessStatusCode();
	var content = await response.Content.ReadAsStringAsync();

	Console.WriteLine($"[{index}] Response: {content}");
}


private static async Task LoginAsync(int index)
{
	var request = new HttpRequestMessage(HttpMethod.Post, "http://192.168.1.144/api/auth/login");
	request.Headers.Accept.ParseAdd("application/json, text/plain, */*");
	request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
	request.Headers.Connection.ParseAdd("keep-alive");
	request.Headers.Referrer = new Uri("http://192.168.1.144/index");
	request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36 Edg/140.0.0.0");
	request.Content = new StringContent(@"{ ""username"": ""admin"", ""password"": ""admin"", ""code"": ""1"" }", Encoding.UTF8, "application/json");

	using var response = await httpClient.SendAsync(request);
	response.EnsureSuccessStatusCode();
	var content = await response.Content.ReadAsStringAsync();

	Console.WriteLine($"[{index}] Response: {content}");
}

