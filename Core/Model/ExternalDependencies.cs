using AppStructure.Utils;
using DingoProjectAppStructure.Core.GeneralUtils;

namespace DingoProjectAppStructure.Core.Model
{
    public class ExternalDependencies : RootByGenericTypes<object>
    {
        public readonly RuntimeDependencies RuntimeDependencies;
        public readonly LogDependencies LogDependencies;
        
        public ExternalDependencies(RuntimeDependencies runtimeDependencies, LogDependencies logDependencies)
        {
            RuntimeDependencies = runtimeDependencies;
            LogDependencies = logDependencies;
        }
    }
}