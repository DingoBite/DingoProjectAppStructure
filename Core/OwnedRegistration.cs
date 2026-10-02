using System;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.Core.ViewModel;

namespace DingoProjectAppStructure.Core
{
    // The existing typed roots receive ownership only after construction and binding succeed.
    public static class OwnedRegistration
    {
        public static T RegisterModel<T>(AppModelRoot root, Func<T> create, Action<T> bind = null) where T : AppModelBase
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            if (root.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(AppModelRoot));
            }
            if (create == null)
            {
                throw new ArgumentNullException(nameof(create));
            }
            if (root.ModelsByTypes.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException($"{typeof(T).Name} is already registered.");
            }

            return RegisterOwned(create, bind, owner =>
            {
                if (root.IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(AppModelRoot));
                }
                root.Register(owner);
            }, root.Get<T>);
        }

        public static T RegisterViewModel<T>(AppViewModelRoot root, Func<T> create, Action<T> bind = null) where T : AppViewModelBase
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            if (root.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(AppViewModelRoot));
            }
            if (create == null)
            {
                throw new ArgumentNullException(nameof(create));
            }
            if (root.ViewModelBasesByTypes.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException($"{typeof(T).Name} is already registered.");
            }

            return RegisterOwned(create, bind, owner =>
            {
                if (root.IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(AppViewModelRoot));
                }
                root.Register(owner);
            }, root.Get<T>);
        }

        private static T RegisterOwned<T>(Func<T> create, Action<T> bind, Action<T> register, Func<T> takeRegistered) where T : class
        {
            var owner = create() ?? throw new InvalidOperationException($"{typeof(T).Name} construction returned no owner.");
            try
            {
                bind?.Invoke(owner);
                register(owner);
                if (!ReferenceEquals(takeRegistered(), owner))
                {
                    throw new InvalidOperationException($"{typeof(T).Name} registration did not retain the created owner.");
                }
                return owner;
            }
            catch (Exception registrationFailure)
            {
                try
                {
                    if (owner is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                catch (Exception cleanupFailure)
                {
                    throw new AggregateException($"{typeof(T).Name} initialization and cleanup failed.", registrationFailure, cleanupFailure);
                }
                throw;
            }
        }
    }
}
