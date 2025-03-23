using System;
using AppStructure.Utils;

namespace ProjectAppStructure.Core.Config
{
    [Serializable]
    public abstract class ConfigBase
    {
        
    }
    
    public class AppConfigRoot : RootByGenericTypes<ConfigBase>
    {
    }
}