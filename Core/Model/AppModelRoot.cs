using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppStructure.Utils;
using DingoProjectAppStructure.Core.ViewModel;
using UnityEngine;

namespace DingoProjectAppStructure.Core.Model
{
    public abstract class AppModelBase { }

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

        private bool _disposed;

        public IReadOnlyDictionary<Type, AppModelBase> ModelsByTypes => ValuesByTypes;
        public bool IsDisposed => _disposed;

        public AppModelRoot(ExternalDependencies externalDependencies)
        {
            ExternalDependencies = externalDependencies;
        }

        public async Task PostInitializeAsync()
        {
            foreach (var (type, appModelBase) in ValuesByTypes)
            {
                try
                {
                    if (appModelBase is HardLinkAppModelBase hardLinkAppModelBase)
                    {
                        await hardLinkAppModelBase.PostInitialize(ExternalDependencies);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot post initialize {type}");
                    Debug.LogException(e);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            var models = new List<AppModelBase>(ValuesByTypes.Values);
            var failures = new List<Exception>();
            AppRootDisposal.Dispose(Get<AppViewModelRootContainer>()?.Root, failures);
            AppRootDisposal.DisposeReverse(models, failures);
            AppRootDisposal.ThrowFailures(failures);
        }
    }
}
