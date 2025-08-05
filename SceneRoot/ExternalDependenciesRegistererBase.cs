using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.Config;
using DingoProjectAppStructure.Core.GeneralUtils;
using DingoProjectAppStructure.Core.Model;
using UnityEngine;

namespace DingoProjectAppStructure.SceneRoot
{
    public class ConfigRegistererBase : MonoBehaviour
    {
        [SerializeField] private List<ScriptableConfigBase> _configs;
        
        public virtual async UniTask RegisterConfigsAsync(AppConfigRoot appConfigRoot)
        {
            foreach (var config in _configs)
            {
                await config.RegisterToAsync(appConfigRoot);
            }
        }
    }
    
    public class ExternalDependenciesRegistererBase : MonoBehaviour, IDisposable
    {
        [SerializeField] private ConfigRegistererBase _configRegisterer;
        
        public async UniTask RegisterConfigsAsync(ExternalDependencies externalDependencies)
        {
            var configRoot = new AppConfigRoot();
            externalDependencies.Register(configRoot);
            await _configRegisterer.RegisterConfigsAsync(configRoot);
        }

        public async UniTask RegisterExternalDependenciesAsync(ExternalDependencies externalDependencies)
        {
            await AddictiveRegisterExternalDependenciesAsync(externalDependencies);
        }

        public virtual UniTask BindToModelAsync(AppModelRoot appModelRoot) => UniTask.CompletedTask;
        public virtual UniTask PostInitializeAsync() => UniTask.CompletedTask;
        
        protected virtual UniTask AddictiveRegisterExternalDependenciesAsync(ExternalDependencies externalDependencies) => UniTask.CompletedTask;
        
        public static ExternalDependencies ConstructExternalDependencies() => new(UpdateAndCoroutineUtils.MakeRuntimeDependencies(), new LogDependencies(LogDependenciesUtils.UnityLogInserted));
        public virtual void Dispose() { }
    }
}