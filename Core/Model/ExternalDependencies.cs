using ProjectAppStructure.Core.Config;
using ProjectAppStructure.Core.GeneralUtils;

namespace ProjectAppStructure.Core.Model
{
    public class ExternalDependencies
    {
        public readonly AppConfigRoot AppConfigRoot;
        public readonly RuntimeDependencies RuntimeDependencies;
        public readonly LogDependencies LogDependencies;
        
        public ExternalDependencies(AppConfigRoot appConfigRoot, RuntimeDependencies runtimeDependencies, LogDependencies logDependencies)
        {
            AppConfigRoot = appConfigRoot;
            RuntimeDependencies = runtimeDependencies;
            LogDependencies = logDependencies;
        }
    }
}