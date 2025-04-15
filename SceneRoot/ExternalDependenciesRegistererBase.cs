using System;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.Config;
using DingoProjectAppStructure.Core.GeneralUtils;
using DingoProjectAppStructure.Core.Model;
using UnityEngine;

namespace DingoProjectAppStructure.SceneRoot
{
    public class ExternalDependenciesRegistererBase : MonoBehaviour
    {
        public virtual ExternalDependencies CreateExternalDependencies()
        {
            return new ExternalDependencies(UpdateAndCoroutineUtils.MakeRuntimeDependencies(), new LogDependencies(UnityLogInserted));
        }

        public async UniTask RegisterConfigsAsync(ExternalDependencies externalDependencies)
        {
            var configRoot = new AppConfigRoot();
            externalDependencies.Register(configRoot);
            await AddictiveRegisterConfigsAsync(externalDependencies);
        }

        public async UniTask RegisterExternalDependenciesAsync(ExternalDependencies externalDependencies)
        {
            await AddictiveRegisterExternalDependenciesAsync(externalDependencies);
        }

        public virtual UniTask BindToModelAsync(AppModelRoot appModelRoot) => UniTask.CompletedTask;
        public virtual UniTask PostInitializeAsync() => UniTask.CompletedTask;
        
        protected virtual UniTask AddictiveRegisterConfigsAsync(ExternalDependencies externalDependencies) => UniTask.CompletedTask;
        protected virtual UniTask AddictiveRegisterExternalDependenciesAsync(ExternalDependencies externalDependencies) => UniTask.CompletedTask;
        
        private void UnityLogInserted(Action logAction)
        {
            try
            {
                logAction();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}