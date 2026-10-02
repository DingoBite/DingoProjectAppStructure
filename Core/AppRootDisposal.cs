using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace DingoProjectAppStructure.Core
{
    // Dispose snapshots of the existing registry; this helper retains no owners or graph.
    public static class AppRootDisposal
    {
        public static void Dispose(object owner, List<Exception> failures)
        {
            if (owner is IDisposable disposable)
            {
                Attempt(disposable.Dispose, failures);
            }
        }

        public static void Attempt(Action cleanup, List<Exception> failures)
        {
            try
            {
                cleanup();
            }
            catch (Exception failure)
            {
                failures.Add(failure);
            }
        }

        public static void DisposeReverse<T>(IReadOnlyList<T> owners, List<Exception> failures)
        {
            for (var index = owners.Count - 1; index >= 0; index--)
            {
                Dispose(owners[index], failures);
            }
        }

        public static void ThrowFailures(IReadOnlyList<Exception> failures)
        {
            if (failures.Count == 1)
            {
                ExceptionDispatchInfo.Capture(failures[0]).Throw();
            }
            if (failures.Count > 1)
            {
                throw new AggregateException("Application root cleanup failed.", failures);
            }
        }
    }
}
