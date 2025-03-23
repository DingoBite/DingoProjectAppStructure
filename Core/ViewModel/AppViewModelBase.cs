using System;
using AppStructure.Utils;
using ProjectAppStructure.Core.Model;
using UnityEngine;

namespace ProjectAppStructure.Core.ViewModel
{
    public abstract class AppViewModelBase
    {
        protected readonly AppModelRoot AppModelRoot;

        public AppViewModelBase(AppModelRoot appModelRoot)
        {
            AppModelRoot = appModelRoot;
        }

        protected ExternalDependencies ExternalDependencies => AppModelRoot.ExternalDependencies;

        protected void Log(string message) => ExternalDependencies.LogDependencies.UnityLogWrap(() => Debug.Log(message));
        protected void LogError(string message) => ExternalDependencies.LogDependencies.UnityLogWrap(() => Debug.LogError(message));
        protected void LogException(Exception exception) => ExternalDependencies.LogDependencies.UnityLogWrap(() => Debug.LogException(exception));
    }
    
    public class AppViewModelRoot : RootByGenericTypes<AppViewModelBase>
    {
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