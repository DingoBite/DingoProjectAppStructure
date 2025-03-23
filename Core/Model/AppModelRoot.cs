using System;
using System.Threading.Tasks;
using AppStructure.Utils;
using UnityEngine;

namespace ProjectAppStructure.Core.Model
{
    public abstract class AppModelBase { }

    public abstract class HardLinkAppModelBase : AppModelBase
    {
        public virtual Task PostInitialize(ExternalDependencies externalDependencies)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterAppStaticDataInitializeAsync()
        {
            return Task.CompletedTask;
        }
    }

    public class AppModelRoot : RootByGenericTypes<AppModelBase>
    {
        public readonly ExternalDependencies ExternalDependencies;

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
                        await hardLinkAppModelBase.PostInitialize(ExternalDependencies);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot post initialize {type}");
                    Debug.LogException(e);
                }
            }
        }
    }
}