using System;
using System.Collections.Generic;
using AppStructure.InputLocker;
using Cysharp.Threading.Tasks;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using ProjectAppStructure.Core;
using ProjectAppStructure.Core.AppLock;
using ProjectAppStructure.Core.Model;
using UnityEngine;

namespace ProjectAppStructure.SceneRoot
{
    public class G : SingletonProtectedBehaviour<G>
    {
        [SerializeField] private AppInputLocker _appInputLocker;
        [SerializeField] private StateController _coreStateController;
        [SerializeField] private AppPopupStateController _appPopupStateController;
        [SerializeField] private ExternalDependenciesRegisterer _externalDependenciesRegisterer;
        [SerializeField] private ModelsRegistererManager _modelsRegistererManager;

        public static IStateController State => Instance._coreStateController;
        public static IAppPopupController Popup => Instance._appPopupStateController;
        public static IAppInputLocker<AppInputLockMessage> Lock => Instance._appInputLocker;
        public static AppModelRoot M => Instance._appModel;
        public static List<string> States => Instance._coreStateController.States;
        
        private AppModelRoot _appModel;

        public void PrepareController()
        {
            Debug.Log(nameof(PrepareController));
            _appInputLocker.Initialize();
            _coreStateController.AppViewRoot.PreInitialize();
            _appPopupStateController.AppPopupViewRoot.PreInitialize();
        }
        
        public async UniTask InitializeControllerAsync(Action<bool> callback)
        {
            Debug.Log(nameof(InitializeControllerAsync));
            await _coreStateController.GoToBootstrap();
            
            _appModel = new AppModelRoot(_externalDependenciesRegisterer.CollectDependencies());
            await _modelsRegistererManager.RegisterModelsAsync(_appModel);
            await _appModel.PostInitializeAsync();
            
            var initializeResult = await _coreStateController.AppViewRoot.InitializeAsync().AsUniTask();
            initializeResult |= await _appPopupStateController.AppPopupViewRoot.InitializeAsync().AsUniTask();
            callback?.Invoke(initializeResult);
        }

        public async UniTask BindAsync(Action<bool> callback)
        {
            Debug.Log(nameof(BindAsync));
            var result = await _coreStateController.AppViewRoot.BindAsync(_appModel).AsUniTask();
            result |= await _appPopupStateController.AppPopupViewRoot.BindAsync(_appModel).AsUniTask();
            callback?.Invoke(result);
        }

        public async UniTask FinalizeAsync(Action<bool> callback)
        {
            Debug.Log(nameof(FinalizeAsync));
            var result = await _coreStateController.AppViewRoot.PostInitializeAsync().AsUniTask();
            result |= await _appPopupStateController.AppPopupViewRoot.PostInitializeAsync().AsUniTask();
 
            callback?.Invoke(result);
            await _coreStateController.GoToStart();
        }
    }
}