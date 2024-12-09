namespace System.IO
{
    public static class PathExtensions
    {
        public static string Profile(string project)
        {
            return Profile(".xky", project);
        }

        public static string Profile(string xky, string project)
        {
            // 获取当前用户的用户文件夹路径
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var specificPath = Path.Combine(path, xky, project);
            // 如果子文件夹不存在，则创建它
            if (!Directory.Exists(specificPath))
            {
                Directory.CreateDirectory(specificPath);
            }
            return specificPath;
        }
    }
}