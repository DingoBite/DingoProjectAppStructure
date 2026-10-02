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
            ThrowIfTeardownStarted();
            try
            {
                _externalDependenciesRegisterer.AwakePrepare();
            }
            catch (Exception initializationFailure)
            {
                CleanupAfterBootstrapFailure(initializationFailure);
                throw;
            }
        }

        public void PrepareController()
        {
            ThrowIfTeardownStarted();
            Debug.Log(nameof(PrepareController));
            try
            {
                _appInputLocker.Initialize();
                _appStateController.AppViewRoot.PreInitialize();
                _appPopupStateController.AppPopupViewRoot.PreInitialize();
            }
            catch (Exception initializationFailure)
            {
                CleanupAfterBootstrapFailure(initializationFailure);
                throw;
            }
        }

        public async UniTask InitializeControllerAsync(Action<bool> callback)
        {
            ThrowIfTeardownStarted();
            Debug.Log(nameof(InitializeControllerAsync));
            try
            {
                _appStateController.GoToBootstrap();

                var externalDependencies = new ExternalDependencies();
                await _externalDependenciesRegisterer.RegisterConfigsAsync(externalDependencies);
                ThrowIfTeardownStarted();
                await _externalDependenciesRegisterer.RegisterExternalDependenciesAsync(externalDependencies);
                ThrowIfTeardownStarted();
                _appModel = new AppModelRoot(externalDependencies);
                await _modelsRegistererManager.RegisterModelsAsync(_appModel);
                ThrowIfTeardownStarted();
                await _appModel.PostInitializeAsync();
                ThrowIfTeardownStarted();
                await _externalDependenciesRegisterer.BindToModelAsync(_appModel);
                ThrowIfTeardownStarted();

                var initializeResult = await _appStateController.AppViewRoot.InitializeAsync().AsUniTask();
                ThrowIfTeardownStarted();
                initializeResult |= await _appPopupStateController.AppPopupViewRoot.InitializeAsync().AsUniTask();
                ThrowIfTeardownStarted();
                callback?.Invoke(initializeResult);
            }
            catch (Exception initializationFailure)
            {
                CleanupAfterBootstrapFailure(initializationFailure);
                throw;
            }
        }

        public async UniTask BindAsync(Action<bool> callback)
        {
            ThrowIfTeardownStarted();
            Debug.Log(nameof(BindAsync));
            try
            {
                var result = await _appStateController.AppViewRoot.BindAsync(_appModel).AsUniTask();
                ThrowIfTeardownStarted();
                result |= await _appPopupStateController.AppPopupViewRoot.BindAsync(_appModel).AsUniTask();
                ThrowIfTeardownStarted();
                _appStateController.GoToLoading();
                callback?.Invoke(result);
            }
            catch (Exception initializationFailure)
            {
                CleanupAfterBootstrapFailure(initializationFailure);
                throw;
            }
        }

        public async UniTask FinalizeAsync(Action<bool> callback)
        {
            ThrowIfTeardownStarted();
            Debug.Log(nameof(FinalizeAsync));
            try
            {
                await _externalDependenciesRegisterer.PostInitializeAsync();
                ThrowIfTeardownStarted();
                var result = await _appStateController.AppViewRoot.PostInitializeAsync().AsUniTask();
                ThrowIfTeardownStarted();
                result |= await _appPopupStateController.AppPopupViewRoot.PostInitializeAsync().AsUniTask();
                ThrowIfTeardownStarted();

                callback?.Invoke(result);
                ThrowIfTeardownStarted();
                _appStateController.GoToStart();
            }
            catch (Exception initializationFailure)
            {
                CleanupAfterBootstrapFailure(initializationFailure);
                throw;
            }
        }

        private void OnApplicationQuit()
        {
            Teardown();
        }

        private void OnDestroy()
        {
            Teardown();
        }

        private void ThrowIfTeardownStarted()
        {
            if (_teardownStarted)
            {
                throw new ObjectDisposedException(nameof(G));
            }
        }

        private void CleanupAfterBootstrapFailure(Exception initializationFailure)
        {
            try
            {
                Teardown();
            }
            catch (Exception cleanupFailure)
            {
                throw new AggregateException("Application initialization and cleanup failed.", initializationFailure, cleanupFailure);
            }
        }

        private void Teardown()
        {
            if (_teardownStarted)
            {
                return;
            }

            _teardownStarted = true;
            var failures = new List<Exception>();
            try
            {
                AppRootDisposal.Dispose(_appModel, failures);
            }
            finally
            {
                _appModel = null;
                AppRootDisposal.Dispose(_externalDependenciesRegisterer, failures);
            }
            AppRootDisposal.ThrowFailures(failures);
        }
    }
}
