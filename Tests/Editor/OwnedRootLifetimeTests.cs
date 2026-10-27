#if UNITY_EDITOR && UNITY_INCLUDE_TESTS

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DingoProjectAppStructure.Core;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.Core.ViewModel;
using NUnit.Framework;

namespace DingoProjectAppStructure.Tests.Editor
{
    public class OwnedRootLifetimeTests
    {
        [Test]
        public void ModelRootDispose_PendingPreparationRetainsOwnersUntilAwaited()
        {
            var models = new AppModelRoot(new ExternalDependencies());
            var gate = new TaskCompletionSource<bool>();
            var native = OwnedRegistration.RegisterModel(models, () => new OwnedLifetimeModel { Preparation = () => gate.Task });
            var views = new AppViewModelRoot();
            var view = OwnedRegistration.RegisterViewModel(views, () => new OwnedLifetimeViewModel(views, models));
            models.Register(new AppViewModelRootContainer(views));

            Assert.Throws<InvalidOperationException>(models.Dispose);
            var pending = models.PrepareForDisposalAsync();
            Assert.That(pending.IsCompleted, Is.False);
            Assert.That(native.PreparationCount, Is.EqualTo(1));
            Assert.That(models.IsDisposed, Is.False);
            Assert.That(native.DisposeCount, Is.Zero);
            Assert.That(view.DisposeCount, Is.Zero);

            gate.SetResult(true);
            Assert.That(pending.IsCompletedSuccessfully, Is.True);
            models.Dispose();
            models.Dispose();
            views.Dispose();
            Assert.That(models.IsDisposed, Is.True);
            Assert.That(views.IsDisposed, Is.True);
            Assert.That(native.DisposeCount, Is.EqualTo(1));
            Assert.That(view.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void ModelRootDispose_FailedPreparationCanRetryBeforeReleasingOwners()
        {
            var models = new AppModelRoot(new ExternalDependencies());
            var failure = new InvalidOperationException("Native cleanup still owns its resource.");
            var native = OwnedRegistration.RegisterModel(models, () => new OwnedLifetimeModel { Preparation = () => Task.FromException(failure) });
            var views = new AppViewModelRoot();
            var view = OwnedRegistration.RegisterViewModel(views, () => new OwnedLifetimeViewModel(views, models));
            models.Register(new AppViewModelRootContainer(views));

            var error = Assert.Throws<AggregateException>(models.Dispose);
            Assert.That(error.InnerExceptions, Does.Contain(failure));
            Assert.That(models.IsDisposed, Is.False);
            Assert.That(native.DisposeCount, Is.Zero);
            Assert.That(view.DisposeCount, Is.Zero);

            native.Preparation = () => Task.CompletedTask;
            models.Dispose();
            Assert.That(native.PreparationCount, Is.EqualTo(2));
            Assert.That(native.DisposeCount, Is.EqualTo(1));
            Assert.That(view.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void ModelRootDispose_ReleasesViewModelsThenModelsOnceDespiteCleanupFailure()
        {
            var trace = new List<string>();
            var models = new AppModelRoot(new ExternalDependencies());
            var first = OwnedRegistration.RegisterModel(models, () => new OwnedLifetimeModel { Disposal = () => trace.Add("first model") });
            var failure = new InvalidOperationException("Second model cleanup diagnostic.");
            var second = OwnedRegistration.RegisterModel(models, () => new SecondOwnedLifetimeModel { Disposal = () => { trace.Add("second model"); throw failure; } });
            var views = new AppViewModelRoot();
            var view = OwnedRegistration.RegisterViewModel(views, () => new OwnedLifetimeViewModel(views, models) { Disposal = () => trace.Add("view") });
            models.Register(new AppViewModelRootContainer(views));

            Assert.That(Assert.Throws<InvalidOperationException>(models.Dispose), Is.SameAs(failure));
            models.Dispose();
            views.Dispose();
            Assert.That(trace, Is.EqualTo(new[] { "view", "second model", "first model" }));
            Assert.That(first.DisposeCount, Is.EqualTo(1));
            Assert.That(second.DisposeCount, Is.EqualTo(1));
            Assert.That(view.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void OwnedRegistration_BindFailureReleasesOnlyNewOwnerAndPreservesCleanupError()
        {
            var models = new AppModelRoot(new ExternalDependencies());
            var existing = OwnedRegistration.RegisterModel(models, () => new OwnedLifetimeModel());
            var bindingFailure = new InvalidOperationException("Binding failed.");
            var cleanupFailure = new InvalidOperationException("New owner cleanup failed.");
            var candidate = new SecondOwnedLifetimeModel { Disposal = () => throw cleanupFailure };

            var failure = Assert.Throws<AggregateException>(() => OwnedRegistration.RegisterModel(models, () => candidate, _ => throw bindingFailure));
            Assert.That(failure.InnerExceptions, Is.EqualTo(new[] { bindingFailure, cleanupFailure }));
            Assert.That(candidate.DisposeCount, Is.EqualTo(1));
            Assert.That(existing.DisposeCount, Is.Zero);
            Assert.That(models.Get<SecondOwnedLifetimeModel>(), Is.Null);
            models.Dispose();
            Assert.That(existing.DisposeCount, Is.EqualTo(1));
        }
    }

    public class OwnedLifetimeModel : AppModelBase, IDisposable
    {
        public Func<Task> Preparation;
        public Action Disposal;
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

    public class SecondOwnedLifetimeModel : OwnedLifetimeModel { }

    public class OwnedLifetimeViewModel : AppViewModelBase, IDisposable
    {
        public Action Disposal;
        public int DisposeCount;

        public OwnedLifetimeViewModel(AppViewModelRoot views, AppModelRoot models) : base(views, models) { }

        public void Dispose()
        {
            DisposeCount++;
            Disposal?.Invoke();
        }
    }
}

#endif
