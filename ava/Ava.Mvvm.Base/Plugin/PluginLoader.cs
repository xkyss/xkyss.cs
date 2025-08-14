using Microsoft.Extensions.Options;

namespace Ava.Mvvm.Base
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
            
        }

        public void Unload()
        {
            
        }
    }
}