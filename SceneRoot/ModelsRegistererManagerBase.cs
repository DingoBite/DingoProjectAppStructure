using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.Core;
using System.Threading.Tasks;
using DingoProjectAppStructure.Core.ViewModel;
using UnityEngine;

namespace DingoProjectAppStructure.SceneRoot
{
    public class ModelsRegistererManagerBase : MonoBehaviour
    {
        public virtual Task PrepareForDisposalAsync(AppModelRoot appModelRoot) => appModelRoot.PrepareForDisposalAsync();

        public async UniTask RegisterModelsAsync(AppModelRoot appModelRoot)
        {
            await AddictiveRegisterModelsAsync(appModelRoot);
            await RegisterViewModelAsync(appModelRoot);
        }

        public async UniTask RegisterViewModelAsync(AppModelRoot appModelRoot)
        {
            var container = OwnedRegistration.RegisterModel(appModelRoot, () => new AppViewModelRootContainer(new AppViewModelRoot()));
            await AddictiveRegisterViewModelsAsync(appModelRoot, container.Root);
        }

        protected virtual UniTask AddictiveRegisterModelsAsync(AppModelRoot appModelRoot) => UniTask.CompletedTask;
        protected virtual UniTask AddictiveRegisterViewModelsAsync(AppModelRoot appModelRoot, AppViewModelRoot appViewModelRoot) => UniTask.CompletedTask;
    }
}
