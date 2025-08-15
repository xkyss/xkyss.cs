namespace Ks.Ava.Mvvm.Base
{
    public class BaseSettings
    {
        public static readonly string Tag = "BASE";
        
        /// <summary>
        /// 全局输出目录
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;
        /// <summary>
        /// 全局插件目录
        /// </summary>
        public string PluginPath { get; set; } = string.Empty;
    }
}