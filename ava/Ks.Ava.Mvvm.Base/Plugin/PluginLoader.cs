using System.Reflection;
using Microsoft.Extensions.Options;

namespace Ks.Ava.Mvvm.Base
{
    public class PluginLoader
    {
        private readonly BaseSettings _settings;

        public PluginLoader(IOptions<BaseSettings> settings)
        {
            _settings = settings.Value;
        }
        
        public void Load()
        {
            var pluginPath = _settings?.PluginPath;
            if (!Directory.Exists(pluginPath))
            {
                return;
            }
        
            var files = Directory.GetFiles(pluginPath, "*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
            }
        }

        public void Unload()
        {
            
        }
    }
}