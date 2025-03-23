using DingoProjectAppStructure.Core.Config;
using DingoProjectAppStructure.Core.GeneralUtils;

namespace DingoProjectAppStructure.Core.Model
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