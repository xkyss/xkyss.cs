using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MewPad.Core.Services;
using QuickLaunch.Plugin.Models;

namespace QuickLaunch.Plugin.Services
{
    /// <summary>
    /// QuickLaunch 服务 - 处理配置加载、保存、启动执行
    /// </summary>
    public class LaunchService
    {
        private const string ConfigFileName = "quicklaunch-config.json";
        private const string ConfigDirName = "MewPad";

        private readonly ILogger _logger;
        private readonly string _configPath;
        private LaunchConfig? _currentConfig;

        public LaunchService(ILogger? logger = null)
        {
            _logger = logger ?? new DebugLogger(nameof(LaunchService));
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configDir = Path.Combine(appDataPath, ConfigDirName);
            _configPath = Path.Combine(configDir, ConfigFileName);

            // 确保目录存在
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
        }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        public string ConfigPath => _configPath;

        /// <summary>
        /// 获取当前配置（如果未加载则从文件加载）
        /// </summary>
        public LaunchConfig GetCurrentConfig()
        {
            return _currentConfig ?? LoadConfig();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public LaunchConfig LoadConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    var json = File.ReadAllText(_configPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };
                    var config = JsonSerializer.Deserialize<LaunchConfig>(json, options);
                    _currentConfig = config ?? new LaunchConfig();
                    return _currentConfig;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to load config: {ex.Message}", ex);
                    _currentConfig = new LaunchConfig();
                    return _currentConfig;
                }
            }

                // 如果配置文件不存在，加载默认配置
                _currentConfig = LoadDefaultConfig();
                return _currentConfig;
            }

            /// <summary>
            /// 加载默认配置（嵌入资源或硬编码）
            /// </summary>
            private LaunchConfig LoadDefaultConfig()
            {
                // 创建一个基本的默认配置
                var config = new LaunchConfig
                {
                    Version = "1.0.0"
                };

                // 添加默认分类
                config.Categories.AddRange(new[]
                {
                    new LaunchCategory { Id = "work", Name = "工作", Icon = "💼", Order = 10 },
                    new LaunchCategory { Id = "work-websites", Name = "常用网站", Icon = "🌐", ParentId = "work", Order = 20 },
                    new LaunchCategory { Id = "work-tools", Name = "本地工具", Icon = "🔧", ParentId = "work", Order = 30 },
                    new LaunchCategory { Id = "development", Name = "开发", Icon = "👨‍💻", Order = 40 },
                    new LaunchCategory { Id = "dev-docs", Name = "文档", Icon = "📚", ParentId = "development", Order = 50 },
                    new LaunchCategory { Id = "dev-tools", Name = "工具", Icon = "⚙️", ParentId = "development", Order = 60 },
                });

                // 添加默认启动项
                config.Items.AddRange(new[]
                {
                    new LaunchItem { Id = "github", Name = "GitHub", Type = LaunchItemType.Web, Target = "https://github.com", Category = "work-websites", Icon = "🐙", Description = "版本控制平台", Order = 10 },
                    new LaunchItem { Id = "gmail", Name = "Gmail", Type = LaunchItemType.Web, Target = "https://mail.google.com", Category = "work-websites", Icon = "📧", Description = "邮件系统", Order = 20 },
                    new LaunchItem { Id = "notion", Name = "Notion", Type = LaunchItemType.Web, Target = "https://notion.so", Category = "work-websites", Icon = "🗂", Description = "文档协作", Order = 30 },
                    new LaunchItem { Id = "mdn", Name = "MDN Web Docs", Type = LaunchItemType.Web, Target = "https://developer.mozilla.org", Category = "dev-docs", Icon = "📖", Description = "Web标准文档", Order = 10 },
                    new LaunchItem { Id = "stack-overflow", Name = "Stack Overflow", Type = LaunchItemType.Web, Target = "https://stackoverflow.com", Category = "dev-docs", Icon = "❓", Description = "问题解答平台", Order = 20 },
                });

                return config;
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig(LaunchConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configPath, json);
                _currentConfig = config;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save config: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 执行启动项
        /// </summary>
        public bool ExecuteItem(LaunchItem item)
        {
            if (!item.Enabled)
            {
                return false;
            }

            try
            {
                return item.Type switch
                {
                    LaunchItemType.Web => ExecuteWeb(item.Target),
                    LaunchItemType.Application => ExecuteApplication(item.Target),
                    LaunchItemType.Script => ExecuteScript(item.Target),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to execute item '{item.Name}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 执行网页启动（使用默认浏览器打开URL）
        /// </summary>
        private bool ExecuteWeb(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            try
            {
                // 确保URL有协议前缀
                var url = target;
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 执行应用程序启动
        /// </summary>
        private bool ExecuteApplication(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 执行脚本/命令
        /// </summary>
        private bool ExecuteScript(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            try
            {
                // 在Windows上使用cmd.exe执行命令
                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {target}",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process.Start(processInfo);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
