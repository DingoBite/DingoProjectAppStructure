using System;
using System.Collections.Generic;
using AppStructure.Utils;
using DingoProjectAppStructure.Core.Model;

namespace DingoProjectAppStructure.Core.ViewModel
{
    public abstract class AppViewModelBase
    {
        protected readonly AppModelRoot AppModelRoot;
        protected readonly AppViewModelRoot AppViewModelRoot;

        protected ExternalDependencies ExternalDependencies => AppModelRoot.ExternalDependencies;

        public AppViewModelBase(AppViewModelRoot appViewModelRoot, AppModelRoot appModelRoot)
        {
            AppViewModelRoot = appViewModelRoot;
            AppModelRoot = appModelRoot;
        }
    }

    public class AppViewModelRoot : RootByGenericTypes<AppViewModelBase>, IDisposable
    {
        private bool _disposed;

        public IReadOnlyDictionary<Type, AppViewModelBase> ViewModelBasesByTypes => ValuesByTypes;
        public bool IsDisposed => _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            var viewModels = new List<AppViewModelBase>(ValuesByTypes.Values);
            var failures = new List<Exception>();
            AppRootDisposal.DisposeReverse(viewModels, failures);
            AppRootDisposal.ThrowFailures(failures);
        }
    }

    public class AppViewModelRootContainer : AppModelBase
    {
        public readonly AppViewModelRoot Root;

        public AppViewModelRootContainer(AppViewModelRoot root)
        {
            Root = root;
        }
    }
}
