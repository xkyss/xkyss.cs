namespace Mew.Launcher;

/// <summary>
/// 启动管理器管理的单个可启动条目(程序、脚本或 URL),含名称、命令与可选的参数、工作目录、分类、图标与每项热键。
/// </summary>
public sealed record LauncherItem(
    string Id,
    string Name,
    string Command,
    string? Args = null,
    string? WorkingDirectory = null,
    string Category = "默认",
    string? Icon = null,
    string? Hotkey = null);
