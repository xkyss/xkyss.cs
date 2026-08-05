namespace Mew.Launcher;

/// <summary>
/// 启动管理器管理的单个可启动条目(程序、脚本或 URL),含名称、命令与可选的参数、工作目录、分类、图标与每项热键。
/// CategoryId 是新数据模型中的分类引用(可空 = 未分类);Category 是旧模型的分类文本,
/// 供现有界面兼容显示,由数据层从分类树解析填充,票据 03 移除。
/// </summary>
public sealed record LauncherItem(
    string Id,
    string Name,
    string Command,
    string? Args = null,
    string? WorkingDirectory = null,
    string Category = "默认",
    string? CategoryId = null,
    string? Icon = null,
    string? Hotkey = null);
