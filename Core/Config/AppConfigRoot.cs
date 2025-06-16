using System;
using AppStructure.Utils;

namespace DingoProjectAppStructure.Core.Config
{
    [Serializable]
    public abstract class ConfigBase
    {
    }
    
    public class AppConfigRoot : RootByGenericTypes<ConfigBase>
    {
    }
}