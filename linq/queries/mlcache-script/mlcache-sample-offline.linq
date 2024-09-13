<Query Kind="Program" />

void Main()
{
    // 定义etcdctl命令和参数
    string command = "etcdctl";
    string arguments = "--endpoints=192.168.1.144:2399 get /mlcache/server/sample-managea92";
    
    // 创建ProcessStartInfo对象
    var startInfo = new System.Diagnostics.ProcessStartInfo
    {
        FileName = command,
        Arguments = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    // 创建Process对象
    using (var process = new System.Diagnostics.Process())
    {
        process.StartInfo = startInfo;
        
        // 订阅输出和错误流
        process.OutputDataReceived += (sender, e) => 
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                //Console.WriteLine($"Output: {e.Data}");
				e.Data.Dump();
            }
        };
        
        process.ErrorDataReceived += (sender, e) => 
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"Error: {e.Data}");
            }
        };

        // 启动进程并开始读取输出流
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        // 等待进程退出
        process.WaitForExit();
    }
}
