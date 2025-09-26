using System;
using System.Threading.Tasks;
using AppStructure.Utils;
using UnityEngine;
using UnityEngine.Scripting;

namespace DingoProjectAppStructure.Core.Config
{
    public abstract class ScriptableConfigBase : ScriptableObject
    {
        public const string CREATE_MENU_PREFIX = "GameConfigs/";
        
        public abstract void RegisterTo(AppConfigRoot appConfigRoot);
        public abstract Task RegisterToAsync(AppConfigRoot appConfigRoot);
    }

    public abstract class ScriptableConfig<T> : ScriptableConfigBase where T : ConfigBase
    {
        [SerializeField] private T _config;

        public T Config => _config;
        
        public override void RegisterTo(AppConfigRoot appConfigRoot) => appConfigRoot.Register(_config);
        
        public override Task RegisterToAsync(AppConfigRoot appConfigRoot)
        {
            appConfigRoot.Register(_config);
            return Task.CompletedTask;
        }
    }
    
    [Serializable, Preserve]
    public abstract class ConfigBase
    {
    }
    
    public class AppConfigRoot : RootByGenericTypes<ConfigBase>
    {
    }
}