using System;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.Model;
using UnityEngine;

namespace DingoProjectAppStructure.SceneRoot
{
    public class ExternalDependenciesRegistererBase : MonoBehaviour, IDisposable
    {
        public async UniTask RegisterConfigsAsync(ExternalDependencies externalDependencies)
        {
            await UniTask.CompletedTask;
        }

        public async UniTask RegisterExternalDependenciesAsync(ExternalDependencies externalDependencies)
        {
            await AddictiveRegisterExternalDependenciesAsync(externalDependencies);
        }
        
        public virtual UniTask BindToModelAsync(AppModelRoot appModelRoot) => UniTask.CompletedTask;
        public virtual UniTask PostInitializeAsync() => UniTask.CompletedTask;
        
        protected virtual UniTask AddictiveRegisterExternalDependenciesAsync(ExternalDependencies externalDependencies) => UniTask.CompletedTask;
        protected virtual void AwakePreInitialize() {}
        
        public virtual void Dispose() { }
        public void AwakePrepare() => AwakePreInitialize();
    }
}