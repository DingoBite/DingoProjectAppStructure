using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppStructure.BaseElements;
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

        public static IStateController State => Current?._appStateController;
        public static IAppPopupController Popup => Current?._appPopupStateController;
        public static IAppInputLocker<AppInputLockMessage> Lock => Current?._appInputLocker;
        public static AppModelRoot M => Current?._appModel;
        public static AppViewModelRoot VM => Current?._appModel?.Get<AppViewModelRootContainer>()?.Root;
        public static List<string> States => Current?._appStateController.States;
        public static List<string> PopupStates => Current?._appPopupStateController.States;
        public Exception DisposalError { get; private set; }
        public Exception StartupError { get; private set; }

        private static G _latestOwner;
        private static G _activeOwner;
        private static CancellationTokenSource _playLifetime = new();
        private static G Current
        {
            get
            {
                var candidate = !ReferenceEquals(_activeOwner, null) ? _activeOwner : GetNoCheck();
                if (ReferenceEquals(candidate, null))
                    return null;
                return candidate._claimed && !ReferenceEquals(candidate._ownedPlayLifetime, _playLifetime) ? null : candidate;
            }
        }

        private CancellationTokenSource _ownedPlayLifetime;
        private readonly CancellationTokenSource _startupCancellation = new();
        private readonly List<Exception> _disposalErrors = new();
        private AppModelRoot _appModel;
        private G _previousOwner;
        private Task<bool> _initializationTask;
        private Task<bool> _bindingTask;
        private Task<bool> _finalizationTask;
        private Task _teardownTask;
        private bool _claimed;
        private bool _awakeRequested;
        private bool _controllersRequested;
        private bool _awakePrepared;
        private bool _controllersPrepared;
        private bool _teardownStarted;
        private bool _dependenciesRetired;
        private bool _startPublished;
        private int _callbackDepth;

        private bool IsCurrent => !_teardownStarted && ReferenceEquals(_ownedPlayLifetime, _playLifetime) && ReferenceEquals(_latestOwner, this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void BeginPlayLifetime()
        {
            var previous = _playLifetime;
            _playLifetime = new CancellationTokenSource();
            _latestOwner = null;
            _activeOwner = null;
            previous.Cancel();
            previous.Dispose();
        }

        public void PrepareOnAwake()
        {
            ClaimLifetime();
            _awakeRequested = true;
            PrepareWhenPreviousReleased();
        }

        public void PrepareController()
        {
            ClaimLifetime();
            _controllersRequested = true;
            PrepareWhenPreviousReleased();
        }

        public async UniTask InitializeControllerAsync(Action<bool> callback)
        {
            ClaimLifetime();
            _initializationTask ??= StartStage(InitializeAsync);
            await NotifyStageAsync(_initializationTask, callback);
        }

        public async UniTask BindAsync(Action<bool> callback)
        {
            _bindingTask ??= StartStage(BindCurrentAsync);
            await NotifyStageAsync(_bindingTask, callback);
        }

        public async UniTask FinalizeAsync(Action<bool> callback)
        {
            _finalizationTask ??= StartStage(FinalizeCurrentAsync);
            var result = await NotifyStageAsync(_finalizationTask, callback);
            if (!result || !IsCurrent || _startPublished)
                return;
            _startPublished = true;
            InvokeCallback(_appStateController.GoToStart);
        }

        private void ClaimLifetime()
        {
            if (_claimed || _teardownStarted)
                return;
            // Authored components can be constructed before SubsystemRegistration.
            _ownedPlayLifetime = _playLifetime;
            _claimed = true;
            _previousOwner = _latestOwner;
            _latestOwner = this;
            if (!ReferenceEquals(_previousOwner, null))
                _ = _previousOwner.TeardownAsync();
        }

        private void PrepareWhenPreviousReleased()
        {
            if (!IsCurrent)
                return;
            if (!ReferenceEquals(_previousOwner, null) && !_previousOwner._dependenciesRetired)
                return;
            _activeOwner = this;
            if (_awakeRequested && !_awakePrepared)
            {
                _awakePrepared = true;
                InvokeCallback(() => _externalDependenciesRegisterer.AwakePrepare(_ownedPlayLifetime.Token));
            }
            if (!IsCurrent || !_controllersRequested || _controllersPrepared)
                return;
            _controllersPrepared = true;
            InvokeCallback(() =>
            {
                _appInputLocker.Initialize();
                if (!IsCurrent)
                    return;
                _appStateController.AppViewRoot.PreInitialize();
                if (IsCurrent)
                    _appPopupStateController.AppPopupViewRoot.PreInitialize();
            });
        }

        // Publish the stage before any callback can synchronously request teardown.
        private Task<bool> StartStage(Func<Task<bool>> stage)
        {
            var completion = new TaskCompletionSource<bool>();
            _ = CompleteStageAsync(stage, completion);
            return completion.Task;
        }

        private async Task CompleteStageAsync(Func<Task<bool>> stage, TaskCompletionSource<bool> completion)
        {
            await Task.Yield();
            try
            {
                completion.TrySetResult(IsCurrent && await stage());
            }
            catch (Exception error)
            {
                StartupError = error;
                Debug.LogException(error);
                completion.TrySetResult(false);
            }
        }

        private async UniTask<bool> NotifyStageAsync(Task<bool> stage, Action<bool> callback)
        {
            var result = await stage;
            if (IsCurrent)
                InvokeCallback(() => callback?.Invoke(result));
            return result;
        }

        private async Task<bool> InitializeAsync()
        {
            if (!ReferenceEquals(_previousOwner, null))
                await _previousOwner.TeardownAsync();
            if (!IsCurrent)
                return false;
            _previousOwner = null;
            PrepareWhenPreviousReleased();
            if (!IsCurrent)
                return false;
            InvokeCallback(_appStateController.GoToBootstrap);
            if (!IsCurrent)
                return false;
            var registerer = _externalDependenciesRegisterer;
            var manager = _modelsRegistererManager;
            var views = _appStateController.AppViewRoot;
            var popups = _appPopupStateController.AppPopupViewRoot;
            var dependencies = new ExternalDependencies();
            await registerer.RegisterConfigsAsync(dependencies);
            if (!IsCurrent)
                return false;
            await registerer.RegisterExternalDependenciesAsync(dependencies);
            if (!IsCurrent)
                return false;
            var models = new AppModelRoot(dependencies, _ownedPlayLifetime.Token);
            _appModel = models;
            await manager.RegisterModelsAsync(models);
            if (!IsCurrent || !ReferenceEquals(_appModel, models))
                return false;
            await models.PostInitializeAsync(_startupCancellation.Token);
            if (!IsCurrent || !ReferenceEquals(_appModel, models))
                return false;
            await registerer.BindToModelAsync(models);
            if (!IsCurrent || !ReferenceEquals(_appModel, models))
                return false;
            var result = await views.InitializeAsync();
            if (!IsCurrent || !ReferenceEquals(_appModel, models))
                return false;
            result &= await popups.InitializeAsync();
            return IsCurrent && ReferenceEquals(_appModel, models) && result;
        }

        private async Task<bool> BindCurrentAsync()
        {
            if (_initializationTask == null || !await _initializationTask || !IsCurrent)
                return false;
            var models = _appModel;
            var views = _appStateController.AppViewRoot;
            var popups = _appPopupStateController.AppPopupViewRoot;
            var result = await views.BindAsync(models);
            if (!IsCurrent || !ReferenceEquals(_appModel, models))
                return false;
            result &= await popups.BindAsync(models);
            if (!IsCurrent || !ReferenceEquals(_appModel, models))
                return false;
            if (result)
                InvokeCallback(_appStateController.GoToLoading);
            return IsCurrent && result;
        }

        private async Task<bool> FinalizeCurrentAsync()
        {
            if (_bindingTask == null || !await _bindingTask || !IsCurrent)
                return false;
            var models = _appModel;
            var registerer = _externalDependenciesRegisterer;
            var views = _appStateController.AppViewRoot;
            var popups = _appPopupStateController.AppPopupViewRoot;
            await registerer.PostInitializeAsync();
            if (!IsCurrent || !ReferenceEquals(_appModel, models))
                return false;
            var result = await views.PostInitializeAsync();
            if (!IsCurrent || !ReferenceEquals(_appModel, models))
                return false;
            result &= await popups.PostInitializeAsync();
            return IsCurrent && ReferenceEquals(_appModel, models) && result;
        }

        private void InvokeCallback(Action callback)
        {
            _callbackDepth++;
            try
            {
                callback();
            }
            finally
            {
                _callbackDepth--;
            }
        }

        public Task TeardownAsync()
        {
            if (_teardownTask != null && !_teardownTask.IsFaulted && !_teardownTask.IsCanceled)
                return _teardownTask;
            var completion = new TaskCompletionSource<bool>();
            _teardownTask = completion.Task;
            _teardownStarted = true;
            _ = CompleteTeardownAsync(completion);
            return _teardownTask;
        }

        private async Task CompleteTeardownAsync(TaskCompletionSource<bool> completion)
        {
            try
            {
                _startupCancellation.Cancel();
                var views = _controllersPrepared ? (AppPartRoot<AppModelRoot>)_appStateController.AppViewRoot : null;
                var popups = _controllersPrepared ? (AppPartRoot<AppModelRoot>)_appPopupStateController.AppPopupViewRoot : null;
                CancelViews(views);
                CancelViews(popups);
                // Shutdown requested inside startup must unwind that callback before draining it.
                if (_callbackDepth != 0 || _initializationTask is { IsCompleted: false } || _bindingTask is { IsCompleted: false } || _finalizationTask is { IsCompleted: false })
                    await Task.Yield();
                if (!ReferenceEquals(_previousOwner, null))
                    await _previousOwner.TeardownAsync();
                if (_initializationTask != null)
                    await _initializationTask;
                if (_bindingTask != null)
                    await _bindingTask;
                if (_finalizationTask != null)
                    await _finalizationTask;
                await DeinitializeViewsAsync(views);
                await DeinitializeViewsAsync(popups);
                var models = _appModel;
                if (models != null)
                {
                    // Stop consumers before the mandatory Model dependency barrier.
                    var barriers = new List<Exception>();
                    try
                    {
                        await _modelsRegistererManager.PrepareForDisposalAsync(models);
                    }
                    catch (Exception error)
                    {
                        barriers.Add(error);
                    }
                    try
                    {
                        await models.PrepareForDisposalAsync();
                    }
                    catch (Exception error)
                    {
                        barriers.Add(error);
                    }
                    if (barriers.Count != 0)
                        throw new AggregateException("Application consumers still own dependencies; teardown can be retried.", barriers);
                    DisposeOwner(models);
                }
                DisposeOwner(_externalDependenciesRegisterer);
                _appModel = null;
                _previousOwner = null;
                _dependenciesRetired = true;
                _startupCancellation.Dispose();
                if (ReferenceEquals(_activeOwner, this))
                    _activeOwner = null;
                completion.TrySetResult(true);
            }
            catch (OperationCanceledException)
            {
                completion.TrySetCanceled();
            }
            catch (Exception error)
            {
                completion.TrySetException(error);
            }
        }

        private void CancelViews(AppPartRoot<AppModelRoot> views)
        {
            try
            {
                views?.CancelPendingOperations();
            }
            catch (Exception error)
            {
                RecordDisposalError(error);
            }
        }

        private async Task DeinitializeViewsAsync(AppPartRoot<AppModelRoot> views)
        {
            if (views == null)
                return;
            try
            {
                if (!await views.DeinitializeAsync())
                    RecordDisposalError(new InvalidOperationException("An application view completed deinitialization with errors."));
            }
            catch (Exception error)
            {
                RecordDisposalError(error);
            }
        }

        private void DisposeOwner(IDisposable owner)
        {
            try
            {
                owner?.Dispose();
            }
            catch (Exception error)
            {
                RecordDisposalError(error);
            }
        }

        private void RecordDisposalError(Exception error)
        {
            _disposalErrors.Add(error);
            DisposalError = new AggregateException("Application cleanup completed with errors.", _disposalErrors);
            Debug.LogException(error);
        }

        private void OnApplicationQuit() => ObserveTeardownAsync().Forget();
        private void OnDestroy() => ObserveTeardownAsync().Forget();

        private async UniTask ObserveTeardownAsync()
        {
            try
            {
                await TeardownAsync();
            }
            catch (OperationCanceledException) when (!ReferenceEquals(_ownedPlayLifetime, _playLifetime))
            {
                // A new Play session is a separate process lifetime, not a successful teardown.
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
        }
    }
}
