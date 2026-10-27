using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppStructure.Utils;
using DingoProjectAppStructure.Core.ViewModel;
using UnityEngine;

namespace DingoProjectAppStructure.Core.Model
{
    public abstract class AppModelBase
    {
        public virtual Task PrepareForDisposalAsync() => Task.CompletedTask;
    }

    public abstract class HardLinkAppModelBase : AppModelBase
    {
        public virtual Task PostInitialize(ExternalDependencies externalDependencies)
        {
            return Task.CompletedTask;
        }
    }

    public class AppModelRoot : RootByGenericTypes<AppModelBase>, IDisposable
    {
        public readonly ExternalDependencies ExternalDependencies;
        // An ended Play lifetime forbids stale global publication, not exact owned cleanup.
        public readonly CancellationToken LifetimeCancellation;

        public IReadOnlyDictionary<Type, AppModelBase> ModelsByTypes => ValuesByTypes;
        public bool IsDisposed => _disposed;
        private Task _disposalPreparation;
        private bool _disposed;

        public AppModelRoot(ExternalDependencies externalDependencies, CancellationToken lifetimeCancellation = default)
        {
            ExternalDependencies = externalDependencies;
            LifetimeCancellation = lifetimeCancellation;
        }

        public async Task PostInitializeAsync(CancellationToken cancellationToken = default)
        {
            foreach (var (type, appModelBase) in ValuesByTypes)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                try
                {
                    if (appModelBase is HardLinkAppModelBase hardLinkAppModelBase)
                        await hardLinkAppModelBase.PostInitialize(ExternalDependencies);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot post initialize {type}");
                    Debug.LogException(e);
                }
            }
        }

        // A failed safety barrier retains dependencies; an explicit later call may retry it.
        public Task PrepareForDisposalAsync()
        {
            if (_disposalPreparation != null && !_disposalPreparation.IsFaulted && !_disposalPreparation.IsCanceled)
                return _disposalPreparation;
            var completion = new TaskCompletionSource<bool>();
            _disposalPreparation = completion.Task;
            _ = CompleteDisposalPreparationAsync(ValuesByTypes.Values.ToArray(), completion);
            return _disposalPreparation;
        }

        private async Task CompleteDisposalPreparationAsync(AppModelBase[] models, TaskCompletionSource<bool> completion)
        {
            var errors = new List<Exception>();
            foreach (var model in models)
            {
                try
                {
                    await model.PrepareForDisposalAsync();
                }
                catch (Exception error)
                {
                    errors.Add(error);
                }
            }
            if (errors.Count == 0)
                completion.TrySetResult(true);
            else
                completion.TrySetException(new AggregateException("Application Model disposal preparation failed.", errors));
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            var preparation = PrepareForDisposalAsync();
            if (!preparation.IsCompleted)
                throw new InvalidOperationException("Await PrepareForDisposalAsync before disposing an application Model root with pending native cleanup.");
            preparation.GetAwaiter().GetResult();
            _disposed = true;

            var models = new List<AppModelBase>(ValuesByTypes.Values);
            var failures = new List<Exception>();
            AppRootDisposal.Dispose(Get<AppViewModelRootContainer>()?.Root, failures);
            AppRootDisposal.DisposeReverse(models, failures);
            AppRootDisposal.ThrowFailures(failures);
        }
    }
}
