using System;
using System.Collections.Generic;
using AppStructure.InputLocker;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core;
using DingoProjectAppStructure.Core.AppLock;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.Core.ViewModel;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoProjectAppStructure.SceneRoot
{
    public class G : ProtectedSingletonBehaviour<G>
    {
        [SerializeField] private AppInputLocker _appInputLocker;
        [SerializeField] private AppStateController _appStateController;
        [SerializeField] private AppPopupStateController _appPopupStateController;
        [SerializeField] private ExternalDependenciesRegistererBase _externalDependenciesRegisterer;
        [SerializeField] private ModelsRegistererManagerBase _modelsRegistererManager;

        public static IStateController State => GetNoCheck()?._appStateController;
        public static IAppPopupController Popup => GetNoCheck()?._appPopupStateController;
        public static IAppInputLocker<AppInputLockMessage> Lock => GetNoCheck()?._appInputLocker;
        public static AppModelRoot M => GetNoCheck()?._appModel;
        public static AppViewModelRoot VM => GetNoCheck()?._appModel?.Get<AppViewModelRootContainer>()?.Root;
        public static List<string> States => GetNoCheck()?._appStateController.States;
        public static List<string> PopupStates => GetNoCheck()?._appPopupStateController.States;

        private AppModelRoot _appModel;
        private bool _teardownStarted;

        public void PrepareOnAwake()
        {
            _externalDependenciesRegisterer.AwakePrepare();
        }

        public void PrepareController()
        {
            Debug.Log(nameof(PrepareController));
            _appInputLocker.Initialize();
            _appStateController.AppViewRoot.PreInitialize();
            _appPopupStateController.AppPopupViewRoot.PreInitialize();
        }

        public async UniTask InitializeControllerAsync(Action<bool> callback)
        {
            Debug.Log(nameof(InitializeControllerAsync));
            _appStateController.GoToBootstrap();

            var externalDependencies = new ExternalDependencies();
            await _externalDependenciesRegisterer.RegisterConfigsAsync(externalDependencies);
            await _externalDependenciesRegisterer.RegisterExternalDependenciesAsync(externalDependencies);
            _appModel = new AppModelRoot(externalDependencies);
            await _modelsRegistererManager.RegisterModelsAsync(_appModel);
            await _appModel.PostInitializeAsync();
            await _externalDependenciesRegisterer.BindToModelAsync(_appModel);

            var initializeResult = await _appStateController.AppViewRoot.InitializeAsync().AsUniTask();
            initializeResult &= await _appPopupStateController.AppPopupViewRoot.InitializeAsync().AsUniTask();
            callback?.Invoke(initializeResult);
        }

        public async UniTask BindAsync(Action<bool> callback)
        {
            Debug.Log(nameof(BindAsync));
            var result = await _appStateController.AppViewRoot.BindAsync(_appModel).AsUniTask();
            result &= await _appPopupStateController.AppPopupViewRoot.BindAsync(_appModel).AsUniTask();
            if (result)
                _appStateController.GoToLoading();
            callback?.Invoke(result);
        }

        public async UniTask FinalizeAsync(Action<bool> callback)
        {
            Debug.Log(nameof(FinalizeAsync));
            await _externalDependenciesRegisterer.PostInitializeAsync();
            var result = await _appStateController.AppViewRoot.PostInitializeAsync().AsUniTask();
            result &= await _appPopupStateController.AppPopupViewRoot.PostInitializeAsync().AsUniTask();
            
            callback?.Invoke(result);
            if (result)
                _appStateController.GoToStart();
        }

        private void OnApplicationQuit()
        {
            Teardown();
        }

        private void OnDestroy()
        {
            Teardown();
        }

        private void Teardown()
        {
            if (_teardownStarted)
                return;

            _teardownStarted = true;
            try
            {
                var viewModelRoot = _appModel?
                    .Get<AppViewModelRootContainer>()?.Root;
                if (viewModelRoot != null)
                {
                    foreach (var (p, value) in viewModelRoot.ViewModelBasesByTypes)
                    {
                        if (value is IDisposable disposable)
                            disposable.Dispose();
                    }
                }

                if (_appModel != null)
                {
                    foreach (var (p, value) in _appModel.ModelsByTypes)
                    {
                        if (value is IDisposable disposable)
                            disposable.Dispose();
                    }
                }
            }
            finally
            {
                _appModel = null;
                _externalDependenciesRegisterer?.Dispose();
            }
        }
    }
}
