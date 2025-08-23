using System;
using System.Collections.Generic;
using AppStructure.InputLocker;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core;
using DingoProjectAppStructure.Core.AppLock;
using DingoProjectAppStructure.Core.Model;
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

        public static IStateController State => Instance._appStateController;
        public static IAppPopupController Popup => Instance._appPopupStateController;
        public static IAppInputLocker<AppInputLockMessage> Lock => Instance._appInputLocker;
        public static AppModelRoot M => Instance._appModel;
        public static List<string> States => Instance._appStateController.States;
        
        private AppModelRoot _appModel;

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
            await _appStateController.GoToBootstrap();

            var externalDependencies = ExternalDependenciesRegistererBase.ConstructExternalDependencies();
            await _externalDependenciesRegisterer.RegisterConfigsAsync(externalDependencies);
            await _externalDependenciesRegisterer.RegisterExternalDependenciesAsync(externalDependencies);
            _appModel = new AppModelRoot(externalDependencies);
            await _modelsRegistererManager.RegisterModelsAsync(_appModel);
            await _appModel.PostInitializeAsync();
            _externalDependenciesRegisterer.BindToModelAsync(_appModel);

            var initializeResult = await _appStateController.AppViewRoot.InitializeAsync().AsUniTask();
            initializeResult |= await _appPopupStateController.AppPopupViewRoot.InitializeAsync().AsUniTask();
            callback?.Invoke(initializeResult);
        }

        public async UniTask BindAsync(Action<bool> callback)
        {
            Debug.Log(nameof(BindAsync));
            var result = await _appStateController.AppViewRoot.BindAsync(_appModel).AsUniTask();
            result |= await _appPopupStateController.AppPopupViewRoot.BindAsync(_appModel).AsUniTask();
            await _appStateController.GoToLoading();
            callback?.Invoke(result);
        }

        public async UniTask FinalizeAsync(Action<bool> callback)
        {
            Debug.Log(nameof(FinalizeAsync));
            var result = await _appStateController.AppViewRoot.PostInitializeAsync().AsUniTask();
            result |= await _appPopupStateController.AppPopupViewRoot.PostInitializeAsync().AsUniTask();
            await _externalDependenciesRegisterer.PostInitializeAsync();
            
            callback?.Invoke(result);
            await _appStateController.GoToStart();
        }

        private void OnDestroy()
        {
            _externalDependenciesRegisterer.Dispose();
        }
    }
}