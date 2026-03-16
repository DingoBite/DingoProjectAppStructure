using System;
using System.Collections.Generic;
using AppStructure.Utils;
using DingoProjectAppStructure.Core.Model;
using UnityEngine;

namespace DingoProjectAppStructure.Core.ViewModel
{
    public abstract class AppViewModelBase
    {
        protected readonly AppModelRoot AppModelRoot;
        protected readonly AppViewModelRoot AppViewModelRoot;

        public AppViewModelBase(AppViewModelRoot appViewModelRoot, AppModelRoot appModelRoot)
        {
            AppViewModelRoot = appViewModelRoot;
            AppModelRoot = appModelRoot;
        }

        protected ExternalDependencies ExternalDependencies => AppModelRoot.ExternalDependencies;
    }
    
    public class AppViewModelRoot : RootByGenericTypes<AppViewModelBase>
    {
        public IReadOnlyDictionary<Type, AppViewModelBase> ViewModelBasesByTypes => ValuesByTypes;
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