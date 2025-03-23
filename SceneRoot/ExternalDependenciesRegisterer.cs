using System;
using ProjectAppStructure.Core.Config;
using ProjectAppStructure.Core.GeneralUtils;
using UnityEngine;

namespace ProjectAppStructure.SceneRoot
{
    public class ExternalDependenciesRegisterer : MonoBehaviour
    {
        public Core.Model.ExternalDependencies CollectDependencies()
        {
            var configRoot = new AppConfigRoot();
            RegisterConfigs(configRoot);
            return new Core.Model.ExternalDependencies(configRoot, UpdateAndCoroutineUtils.MakeRuntimeDependencies(), new LogDependencies(UnityLogInserted));
        }

        protected virtual void RegisterConfigs(AppConfigRoot appConfigRoot) { }
        
        private void UnityLogInserted(Action logAction)
        {
            try
            {
                logAction();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}