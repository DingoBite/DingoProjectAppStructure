using Cysharp.Threading.Tasks;
using ProjectAppStructure.Core.Model;
using ProjectAppStructure.Core.ViewModel;
using UnityEngine;

namespace ProjectAppStructure.SceneRoot
{
    public class ModelsRegistererManager : MonoBehaviour
    {
        public async UniTask RegisterModelsAsync(AppModelRoot appModelRoot)
        {
            appModelRoot.RegisterModel(new AppPopupMessageModel());
            await AddictiveRegisterModelsAsync(appModelRoot);
            await RegisterViewModelAsync(appModelRoot);
        }

        public async UniTask RegisterViewModelAsync(AppModelRoot appModelRoot)
        {
            var appViewModelRoot = new AppViewModelRoot();
            appModelRoot.RegisterModel(new AppViewModelRootContainer(appViewModelRoot));
            await AddictiveRegisterViewModelsAsync(appViewModelRoot);
        }

        protected virtual UniTask AddictiveRegisterModelsAsync(AppModelRoot appModelRoot) => UniTask.CompletedTask;
        protected virtual UniTask AddictiveRegisterViewModelsAsync(AppViewModelRoot appViewModelRoot) => UniTask.CompletedTask;
    }
}