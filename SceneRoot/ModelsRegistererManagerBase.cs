using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.Core.ViewModel;
using UnityEngine;

namespace DingoProjectAppStructure.SceneRoot
{
    public class ModelsRegistererManagerBase : MonoBehaviour
    {
        public async UniTask RegisterModelsAsync(AppModelRoot appModelRoot)
        {
            appModelRoot.Register(new AppPopupMessageModel());
            await AddictiveRegisterModelsAsync(appModelRoot);
            await RegisterViewModelAsync(appModelRoot);
        }

        public async UniTask RegisterViewModelAsync(AppModelRoot appModelRoot)
        {
            var appViewModelRoot = new AppViewModelRoot();
            appModelRoot.Register(new AppViewModelRootContainer(appViewModelRoot));
            await AddictiveRegisterViewModelsAsync(appViewModelRoot);
        }

        protected virtual UniTask AddictiveRegisterModelsAsync(AppModelRoot appModelRoot) => UniTask.CompletedTask;
        protected virtual UniTask AddictiveRegisterViewModelsAsync(AppViewModelRoot appViewModelRoot) => UniTask.CompletedTask;
    }
}