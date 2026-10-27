#if UNITY_EDITOR && UNITY_INCLUDE_TESTS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppStructure;
using AppStructure.BaseElements;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core;
using DingoProjectAppStructure.Core.AppLock;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.Core.ViewModel;
using DingoProjectAppStructure.SceneRoot;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DingoProjectAppStructure.Tests.PlayMode
{
    [Category("Runtime")]
    public class GAsyncLifetimePlayModeTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var errors = new List<Exception>();
            foreach (var fixture in AppLifetimeTestFixture.Active.ToArray())
            {
                var teardown = fixture.Root.TeardownAsync();
                for (var frame = 0; frame < 120 && !teardown.IsCompleted; frame++)
                {
                    yield return null;
                }
                if (!teardown.IsCompleted)
                    errors.Add(new TimeoutException("Test application teardown did not finish before exiting Play."));
                else if (teardown.IsFaulted)
                    errors.Add(teardown.Exception);
                fixture.Dispose();
            }
            if (Application.isPlaying)
                yield return new ExitPlayMode();
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void ModelBarrier_PublishesPendingTask_AndRetriesNativeFailure()
        {
            var models = new AppModelRoot(new ExternalDependencies());
            var gate = new TaskCompletionSource<bool>();
            var native = new SafetyTestModel { Preparation = () => gate.Task };
            var independent = new IndependentSafetyTestModel();
            models.Register(native);
            models.Register(independent);

            var first = models.PrepareForDisposalAsync();
            Assert.That(models.PrepareForDisposalAsync(), Is.SameAs(first));
            Assert.That(first.IsCompleted, Is.False);
            gate.SetException(new InvalidOperationException("native ownership remains"));
            Assert.That(first.IsFaulted, Is.True);
            Assert.That(independent.PreparationCount, Is.EqualTo(1));
            _ = first.Exception;

            native.Preparation = () => Task.CompletedTask;
            var retry = models.PrepareForDisposalAsync();
            Assert.That(retry, Is.Not.SameAs(first));
            Assert.That(retry.IsCompletedSuccessfully, Is.True);
            Assert.That(native.PreparationCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator Teardown_DrainsViewsThenStopsManagerAndModels_BeforeAnyDependencyDispose()
        {
            using var fixture = new AppLifetimeTestFixture();
            var gate = new TaskCompletionSource<bool>();
            var stopped = false;
            var trace = fixture.Trace;
            fixture.Manager.Preparation = _ =>
            {
                stopped = true;
                trace.Add("stop");
                return Task.CompletedTask;
            };
            var model = new SafetyTestModel
            {
                Preparation = () =>
                {
                    Assert.That(stopped, Is.True);
                    trace.Add("drain");
                    return gate.Task;
                },
                Disposal = () => trace.Add("model")
            };
            var models = fixture.InstallModels(model, () => trace.Add("vm"));
            fixture.Root.PrepareOnAwake();
            fixture.Root.PrepareController();
            fixture.Views.Deinitialization = () =>
            {
                Assert.That(stopped, Is.False);
                Assert.That(model.DisposeCount, Is.Zero);
                trace.Add("views");
            };

            var teardown = fixture.Root.TeardownAsync();
            Assert.That(teardown.IsCompleted, Is.False);
            Assert.That(G.M, Is.SameAs(models), "Same-Play consumers retain their captured Model owner until the dependency barrier finishes.");
            Assert.That(trace, Is.EqualTo(new[] { "awake", "views", "stop", "drain" }));
            Assert.That(fixture.Registerer.DisposeCount, Is.Zero);
            Assert.That(model.DisposeCount, Is.Zero);

            gate.SetResult(true);
            yield return AppLifetimeTestFixture.Wait(teardown);
            Assert.That(teardown.IsCompletedSuccessfully, Is.True);
            Assert.That(trace, Is.EqualTo(new[] { "awake", "views", "stop", "drain", "vm", "model", "external" }));
            Assert.That(models.IsDisposed, Is.True);
            Assert.That(models.Get<AppViewModelRootContainer>().Root.IsDisposed, Is.True);
            Assert.That(fixture.Root.TeardownAsync(), Is.SameAs(teardown));
        }

        [UnityTest]
        public IEnumerator NativeFailure_HoldsDependencies_ExplicitRetryReleasesThem()
        {
            using var fixture = new AppLifetimeTestFixture();
            var owned = true;
            var model = new SafetyTestModel { Preparation = () => owned ? Task.FromException(new InvalidOperationException("scene still owned")) : Task.CompletedTask };
            fixture.InstallModels(model);
            fixture.Root.PrepareOnAwake();

            var failed = fixture.Root.TeardownAsync();
            yield return AppLifetimeTestFixture.Wait(failed);
            Assert.That(failed.IsFaulted, Is.True);
            _ = failed.Exception;
            Assert.That(model.DisposeCount, Is.Zero);
            Assert.That(fixture.Registerer.DisposeCount, Is.Zero);

            owned = false;
            var retry = fixture.Root.TeardownAsync();
            yield return AppLifetimeTestFixture.Wait(retry);
            Assert.That(retry.IsCompletedSuccessfully, Is.True);
            Assert.That(retry, Is.Not.SameAs(failed));
            Assert.That(model.DisposeCount, Is.EqualTo(1));
            Assert.That(fixture.Registerer.DisposeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CompletedWorkerError_PreservesDiagnostics_AndFinishesIndependentCleanup()
        {
            using var fixture = new AppLifetimeTestFixture();
            var workerError = new InvalidOperationException("completed worker cleanup");
            var model = new SafetyTestModel();
            model.Preparation = () =>
            {
                model.CleanupError = workerError;
                return Task.CompletedTask;
            };
            fixture.InstallModels(model, () => throw new InvalidOperationException("view model dispose"));
            fixture.Root.PrepareOnAwake();
            LogAssert.Expect(LogType.Exception, new Regex("view model dispose"));

            var teardown = fixture.Root.TeardownAsync();
            yield return AppLifetimeTestFixture.Wait(teardown);
            Assert.That(teardown.IsCompletedSuccessfully, Is.True);
            Assert.That(model.CleanupError, Is.SameAs(workerError));
            Assert.That(fixture.Root.DisposalError?.ToString(), Does.Contain("view model dispose"));
            Assert.That(model.DisposeCount, Is.EqualTo(1));
            Assert.That(fixture.Registerer.DisposeCount, Is.EqualTo(1));

            using var replacement = new AppLifetimeTestFixture();
            replacement.Root.PrepareOnAwake();
            Assert.That(replacement.Registerer.AwakeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Replacement_DoesNotRunAwakeHooks_UntilPreviousNativeOwnerRetires()
        {
            using var previous = new AppLifetimeTestFixture();
            var gate = new TaskCompletionSource<bool>();
            var model = new SafetyTestModel { Preparation = () => gate.Task };
            previous.InstallModels(model);
            previous.Root.PrepareOnAwake();
            using var replacement = new AppLifetimeTestFixture();

            replacement.Root.PrepareOnAwake();
            Assert.That(replacement.Registerer.AwakeCount, Is.Zero);
            Assert.That(previous.Registerer.DisposeCount, Is.Zero);
            gate.SetResult(true);
            yield return AppLifetimeTestFixture.Wait(previous.Root.TeardownAsync());
            replacement.Root.PrepareOnAwake();
            Assert.That(replacement.Registerer.AwakeCount, Is.EqualTo(1));
            Assert.That(previous.Registerer.DisposeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ReplacementAfterFailedTeardown_RetriesPreviousOwner_WithoutDroppingSafetyFailure()
        {
            using var previous = new AppLifetimeTestFixture();
            var owned = true;
            previous.InstallModels(new SafetyTestModel { Preparation = () => owned ? Task.FromException(new InvalidOperationException("native still owned")) : Task.CompletedTask });
            previous.Root.PrepareOnAwake();
            var failed = previous.Root.TeardownAsync();
            _ = failed.Exception;
            using var blocked = new AppLifetimeTestFixture();
            blocked.Root.PrepareOnAwake();
            blocked.Root.PrepareController();
            LogAssert.Expect(LogType.Exception, new Regex("native still owned"));
            var reported = true;
            var initialization = blocked.Root.InitializeControllerAsync(result => reported = result).AsTask();
            yield return AppLifetimeTestFixture.Wait(initialization);
            Assert.That(reported, Is.False);
            Assert.That(blocked.Root.StartupError?.ToString(), Does.Contain("native still owned"));
            Assert.That(blocked.Registerer.AwakeCount, Is.Zero);
            Assert.That(previous.Registerer.DisposeCount, Is.Zero);

            owned = false;
            using var replacement = new AppLifetimeTestFixture();
            replacement.Root.PrepareOnAwake();
            yield return AppLifetimeTestFixture.Wait(blocked.Root.TeardownAsync());
            replacement.Root.PrepareOnAwake();
            Assert.That(previous.Registerer.DisposeCount, Is.EqualTo(1));
            Assert.That(blocked.Registerer.DisposeCount, Is.EqualTo(1));
            Assert.That(replacement.Registerer.AwakeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ShutdownDuringExternalRegistration_DrainsIt_AndSuppressesLateBootstrap()
        {
            using var fixture = new AppLifetimeTestFixture();
            var gate = new TaskCompletionSource<bool>();
            fixture.Registerer.RegistrationGate = gate.Task;
            fixture.Root.PrepareOnAwake();
            fixture.Root.PrepareController();
            var callbacks = 0;
            var initialization = fixture.Root.InitializeControllerAsync(_ => callbacks++).AsTask();
            yield return AppLifetimeTestFixture.WaitFor(() => fixture.Registerer.RegistrationCount == 1);

            var teardown = fixture.Root.TeardownAsync();
            Assert.That(teardown.IsCompleted, Is.False);
            Assert.That(fixture.Registerer.DisposeCount, Is.Zero);
            gate.SetResult(true);
            yield return AppLifetimeTestFixture.Wait(teardown);
            yield return AppLifetimeTestFixture.Wait(initialization);
            Assert.That(callbacks, Is.Zero);
            Assert.That(fixture.Manager.RegistrationCount, Is.Zero);
            Assert.That(fixture.Views.InitializeCount, Is.Zero);
            Assert.That(fixture.Registerer.DisposeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SynchronousShutdownInsideInitialization_DoesNotAwaitItsOwnCallback()
        {
            using var fixture = new AppLifetimeTestFixture();
            Task teardown = null;
            fixture.Registerer.OnRegistration = () => teardown = fixture.Root.TeardownAsync();
            fixture.Root.PrepareOnAwake();
            fixture.Root.PrepareController();
            var callbacks = 0;
            var initialization = fixture.Root.InitializeControllerAsync(_ => callbacks++).AsTask();
            yield return AppLifetimeTestFixture.Wait(initialization);
            Assert.That(teardown, Is.Not.Null);
            yield return AppLifetimeTestFixture.Wait(teardown);
            Assert.That(teardown.IsCompletedSuccessfully, Is.True);
            Assert.That(callbacks, Is.Zero);
            Assert.That(fixture.Manager.RegistrationCount, Is.Zero);
            Assert.That(fixture.Registerer.DisposeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ShutdownDuringViewBinding_DrainsIt_WithoutEnteringLoading()
        {
            using var fixture = new AppLifetimeTestFixture();
            var gate = new TaskCompletionSource<bool>();
            fixture.Views.BindGate = gate.Task;
            fixture.Root.PrepareOnAwake();
            fixture.Root.PrepareController();
            yield return AppLifetimeTestFixture.Wait(fixture.Root.InitializeControllerAsync(null).AsTask());
            var callbacks = 0;
            var binding = fixture.Root.BindAsync(_ => callbacks++).AsTask();
            yield return AppLifetimeTestFixture.WaitFor(() => fixture.Views.BindCount == 1);

            var teardown = fixture.Root.TeardownAsync();
            Assert.That(teardown.IsCompleted, Is.False);
            Assert.That(fixture.Registerer.DisposeCount, Is.Zero);
            gate.SetResult(true);
            yield return AppLifetimeTestFixture.Wait(teardown);
            yield return AppLifetimeTestFixture.Wait(binding);
            Assert.That(fixture.State.CurrentState, Is.EqualTo("bootstrap"));
            Assert.That(callbacks, Is.Zero);
            Assert.That(fixture.Registerer.DisposeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ShutdownDuringFinalization_DrainsIt_WithoutEnteringStart()
        {
            using var fixture = new AppLifetimeTestFixture();
            var gate = new TaskCompletionSource<bool>();
            fixture.Registerer.PostInitializeGate = gate.Task;
            fixture.Root.PrepareOnAwake();
            fixture.Root.PrepareController();
            yield return AppLifetimeTestFixture.Wait(fixture.Root.InitializeControllerAsync(null).AsTask());
            yield return AppLifetimeTestFixture.Wait(fixture.Root.BindAsync(null).AsTask());
            var callbacks = 0;
            var finalization = fixture.Root.FinalizeAsync(_ => callbacks++).AsTask();
            yield return AppLifetimeTestFixture.WaitFor(() => fixture.Registerer.PostInitializeCount == 1);

            var teardown = fixture.Root.TeardownAsync();
            Assert.That(teardown.IsCompleted, Is.False);
            Assert.That(fixture.Registerer.DisposeCount, Is.Zero);
            gate.SetResult(true);
            yield return AppLifetimeTestFixture.Wait(teardown);
            yield return AppLifetimeTestFixture.Wait(finalization);
            Assert.That(fixture.State.CurrentState, Is.EqualTo("loading"));
            Assert.That(callbacks, Is.Zero);
            Assert.That(fixture.Registerer.DisposeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FinalizeCallbackShutdown_PreventsStartState_AndReleasesOnce()
        {
            using var fixture = new AppLifetimeTestFixture();
            fixture.Root.PrepareOnAwake();
            fixture.Root.PrepareController();
            yield return AppLifetimeTestFixture.Wait(fixture.Root.InitializeControllerAsync(null).AsTask());
            yield return AppLifetimeTestFixture.Wait(fixture.Root.BindAsync(null).AsTask());
            Task teardown = null;
            var finalization = fixture.Root.FinalizeAsync(_ => teardown = fixture.Root.TeardownAsync()).AsTask();
            yield return AppLifetimeTestFixture.Wait(finalization);
            yield return AppLifetimeTestFixture.Wait(teardown);
            Assert.That(fixture.State.CurrentState, Is.EqualTo("loading"));
            Assert.That(fixture.Registerer.DisposeCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator AuthoredOwnerConstructedBeforePlayEpoch_CapturesLifetimeAtItsFirstBootstrap()
        {
            using var fixture = new AppLifetimeTestFixture();
            AppLifetimeTestFixture.BeginPlayLifetime();

            fixture.Root.PrepareOnAwake();
            fixture.Root.PrepareController();
            Assert.That(fixture.Registerer.AwakeCount, Is.EqualTo(1));
            Assert.That(fixture.Registerer.AwakeLifetime.IsCancellationRequested, Is.False);
            var initialized = false;
            var bound = false;
            var finalized = false;
            yield return AppLifetimeTestFixture.Wait(fixture.Root.InitializeControllerAsync(result => initialized = result).AsTask());
            yield return AppLifetimeTestFixture.Wait(fixture.Root.BindAsync(result => bound = result).AsTask());
            yield return AppLifetimeTestFixture.Wait(fixture.Root.FinalizeAsync(result => finalized = result).AsTask());

            Assert.That(initialized && bound && finalized, Is.True);
            Assert.That(fixture.State.CurrentState, Is.EqualTo("start"));
            Assert.That(fixture.Manager.RegistrationCount, Is.EqualTo(1));
            Assert.That(G.M.LifetimeCancellation, Is.EqualTo(fixture.Registerer.AwakeLifetime));
            Assert.That(fixture.Root.StartupError, Is.Null);

            var capturedLifetime = AppLifetimeTestFixture.GetField(fixture.Root, "_ownedPlayLifetime");
            AppLifetimeTestFixture.BeginPlayLifetime();
            fixture.Root.PrepareOnAwake();
            fixture.Root.PrepareController();
            Assert.That(AppLifetimeTestFixture.GetField(fixture.Root, "_ownedPlayLifetime"), Is.SameAs(capturedLifetime));
            Assert.That(fixture.Registerer.AwakeLifetime.IsCancellationRequested, Is.True);
            Assert.That(fixture.Registerer.AwakeCount, Is.EqualTo(1));
            Assert.That(G.M, Is.Null, "An already claimed owner must not re-enter a later Play lifetime.");
        }

        [UnityTest]
        public IEnumerator NewPlayEpoch_ReleasesExactOldOwners_WithoutResettingReplacementState()
        {
            using var previous = new AppLifetimeTestFixture();
            var gate = new TaskCompletionSource<bool>();
            var globalValue = 1;
            var model = new SafetyTestModel();
            var models = previous.InstallModels(model);
            model.Preparation = async () =>
            {
                await gate.Task;
                if (!models.LifetimeCancellation.IsCancellationRequested)
                    globalValue = 0;
            };
            previous.Root.PrepareOnAwake();
            Assert.That(previous.Registerer.AwakeLifetime, Is.EqualTo(models.LifetimeCancellation));
            var capturedLifetime = AppLifetimeTestFixture.GetField(previous.Root, "_ownedPlayLifetime");
            var teardown = previous.Root.TeardownAsync();
            AppLifetimeTestFixture.BeginPlayLifetime();
            Assert.That(models.LifetimeCancellation.IsCancellationRequested, Is.True);
            Assert.That(previous.Registerer.AwakeLifetime.IsCancellationRequested, Is.True);
            previous.Root.PrepareOnAwake();
            previous.Root.PrepareController();
            Assert.That(AppLifetimeTestFixture.GetField(previous.Root, "_ownedPlayLifetime"), Is.SameAs(capturedLifetime));
            Assert.That(previous.Registerer.AwakeCount, Is.EqualTo(1));
            Assert.That(previous.Registerer.AwakeLifetime, Is.EqualTo(models.LifetimeCancellation));
            Assert.That(previous.Registerer.AwakeLifetime.IsCancellationRequested, Is.True);
            using var replacement = new AppLifetimeTestFixture();
            replacement.Root.PrepareOnAwake();
            globalValue = 2;
            gate.SetResult(true);
            yield return AppLifetimeTestFixture.Wait(teardown);
            Assert.That(teardown.IsCompletedSuccessfully, Is.True);
            Assert.That(model.DisposeCount, Is.EqualTo(1));
            Assert.That(previous.Registerer.DisposeCount, Is.EqualTo(1));
            Assert.That(replacement.Registerer.AwakeCount, Is.EqualTo(1));
            Assert.That(globalValue, Is.EqualTo(2));
        }
    }

    public class AppLifetimeTestFixture : IDisposable
    {
        public static readonly List<AppLifetimeTestFixture> Active = new();
        public readonly GameObject Owner = new(nameof(AppLifetimeTestFixture));
        public readonly List<string> Trace = new();
        public readonly G Root;
        public readonly AsyncLifetimeTestRegisterer Registerer;
        public readonly AsyncLifetimeTestManager Manager;
        public readonly AsyncLifetimeTestViews Views;
        public readonly AppStateMachine State;

        public AppLifetimeTestFixture()
        {
            Active.Add(this);
            Registerer = Owner.AddComponent<AsyncLifetimeTestRegisterer>();
            Registerer.Trace = Trace;
            Manager = Owner.AddComponent<AsyncLifetimeTestManager>();
            Views = Owner.AddComponent<AsyncLifetimeTestViews>();
            var popups = Owner.AddComponent<AsyncLifetimeTestPopups>();
            SetField(Views, "_stateViews", new SerializedDictionary<string, AppStateRoot<string, AppModelRoot>>());
            SetField(Views, "_staticStateViewElements", new List<StaticStateViewElement<string, AppModelRoot>>());
            SetField(Views, "_staticViewElements", new List<StaticViewElement<AppModelRoot>>());
            SetField(popups, "_stateViews", new SerializedDictionary<string, AppStateRoot<string, AppModelRoot>>());
            SetField(popups, "_staticStateViewElements", new List<StaticStateViewElement<string, AppModelRoot>>());
            SetField(popups, "_staticViewElements", new List<StaticViewElement<AppModelRoot>>());
            State = Owner.AddComponent<AppStateMachine>();
            SetField(State, "<CurrentState>k__BackingField", "initial");
            var states = Owner.AddComponent<AppStateController>();
            SetField(states, "_appCoreStateMachine", State);
            SetField(states, "_appStateElementsRoot", Views);
            SetField(states, "_bootstrapState", "bootstrap");
            SetField(states, "_loadingState", "loading");
            SetField(states, "_startState", "start");
            var popupStates = Owner.AddComponent<AppPopupStateController>();
            SetField(popupStates, "_appPopupViewRoot", popups);
            Root = Owner.AddComponent<G>();
            SetField(Root, "_appInputLocker", Owner.AddComponent<AppInputLocker>());
            SetField(Root, "_appStateController", states);
            SetField(Root, "_appPopupStateController", popupStates);
            SetField(Root, "_externalDependenciesRegisterer", Registerer);
            SetField(Root, "_modelsRegistererManager", Manager);
        }

        public AppModelRoot InstallModels(SafetyTestModel model, Action viewModelDisposal = null)
        {
            Root.PrepareOnAwake();
            var models = new AppModelRoot(new ExternalDependencies(), Registerer.AwakeLifetime);
            models.Register(model);
            var viewModels = new AppViewModelRoot();
            viewModels.Register(new LifetimeTestViewModel(viewModels, models, viewModelDisposal));
            models.Register(new AppViewModelRootContainer(viewModels));
            SetField(Root, "_appModel", models);
            return models;
        }

        public void Dispose()
        {
            var teardown = Root.TeardownAsync();
            if (!teardown.IsCompletedSuccessfully)
                return;
            Active.Remove(this);
            Object.DestroyImmediate(Owner);
        }

        public static IEnumerator Wait(Task task)
        {
            Assert.That(task, Is.Not.Null);
            for (var frame = 0; frame < 120 && !task.IsCompleted; frame++)
            {
                yield return null;
            }
            Assert.That(task.IsCompleted, Is.True, "Application lifetime task did not complete.");
        }

        public static IEnumerator WaitFor(Func<bool> condition)
        {
            for (var frame = 0; frame < 120 && !condition(); frame++)
            {
                yield return null;
            }
            Assert.That(condition(), Is.True, "Application lifetime callback did not occur.");
        }

        public static void BeginPlayLifetime() => typeof(G).GetMethod("BeginPlayLifetime", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);

        public static void SetField(object owner, string name, object value) => FindField(owner.GetType(), name).SetValue(owner, value);
        public static object GetField(object owner, string name) => FindField(owner.GetType(), name).GetValue(owner);

        private static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    return field;
                type = type.BaseType;
            }
            throw new MissingFieldException(name);
        }
    }

    public class SafetyTestModel : AppModelBase, IDisposable
    {
        public Func<Task> Preparation;
        public Action Disposal;
        public Exception CleanupError;
        public int PreparationCount;
        public int DisposeCount;

        public override Task PrepareForDisposalAsync()
        {
            PreparationCount++;
            return Preparation?.Invoke() ?? Task.CompletedTask;
        }

        public void Dispose()
        {
            DisposeCount++;
            Disposal?.Invoke();
        }
    }

    public class IndependentSafetyTestModel : AppModelBase
    {
        public int PreparationCount;
        public override Task PrepareForDisposalAsync()
        {
            PreparationCount++;
            return Task.CompletedTask;
        }
    }

    public class LifetimeTestViewModel : AppViewModelBase, IDisposable
    {
        private readonly Action _disposal;
        public LifetimeTestViewModel(AppViewModelRoot viewModels, AppModelRoot models, Action disposal) : base(viewModels, models) => _disposal = disposal;
        public void Dispose() => _disposal?.Invoke();
    }

    public class AsyncLifetimeTestManager : ModelsRegistererManagerBase
    {
        public Func<AppModelRoot, Task> Preparation;
        public int RegistrationCount;
        public override Task PrepareForDisposalAsync(AppModelRoot root) => Preparation?.Invoke(root) ?? Task.CompletedTask;
        protected override UniTask AddictiveRegisterModelsAsync(AppModelRoot root)
        {
            RegistrationCount++;
            return UniTask.CompletedTask;
        }
    }

    public class AsyncLifetimeTestRegisterer : ExternalDependenciesRegistererBase
    {
        public List<string> Trace;
        public Task RegistrationGate;
        public Task PostInitializeGate;
        public Action OnRegistration;
        public CancellationToken AwakeLifetime;
        public int AwakeCount;
        public int RegistrationCount;
        public int PostInitializeCount;
        public int DisposeCount;

        protected override void AwakePreInitialize()
        {
            AwakeLifetime = LifetimeCancellation;
            AwakeCount++;
            Trace.Add("awake");
        }

        protected override async UniTask AddictiveRegisterExternalDependenciesAsync(ExternalDependencies dependencies)
        {
            RegistrationCount++;
            OnRegistration?.Invoke();
            if (RegistrationGate != null)
                await RegistrationGate;
        }

        public override async UniTask PostInitializeAsync()
        {
            PostInitializeCount++;
            if (PostInitializeGate != null)
                await PostInitializeGate;
        }

        public override void Dispose()
        {
            DisposeCount++;
            Trace.Add("external");
        }
    }

    public class AsyncLifetimeTestViews : AppStateElementsRoot
    {
        // The fixture configures collections after AddComponent, before any tested lifecycle call.
        private void OnValidate() { }

        public int InitializeCount;
        public int BindCount;
        public Task BindGate;
        public Action Deinitialization;
        public override void PreInitialize() { }
        public override Task<bool> InitializeAsync()
        {
            InitializeCount++;
            return Task.FromResult(true);
        }
        public override async Task<bool> BindAsync(AppModelRoot model)
        {
            BindCount++;
            if (BindGate != null)
                await BindGate;
            return true;
        }
        public override Task<bool> PostInitializeAsync() => Task.FromResult(true);
        public override void CancelPendingOperations() { }
        public override Task<bool> DeinitializeAsync()
        {
            Deinitialization?.Invoke();
            return Task.FromResult(true);
        }
    }

    public class AsyncLifetimeTestPopups : AppPopupViewRoot
    {
        private void OnValidate() { }

        public override void PreInitialize() { }
        public override Task<bool> InitializeAsync() => Task.FromResult(true);
        public override Task<bool> BindAsync(AppModelRoot model) => Task.FromResult(true);
        public override Task<bool> PostInitializeAsync() => Task.FromResult(true);
        public override void CancelPendingOperations() { }
        public override Task<bool> DeinitializeAsync() => Task.FromResult(true);
    }
}

#endif
