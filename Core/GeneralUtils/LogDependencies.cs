using System;
using UnityEngine;

namespace DingoProjectAppStructure.Core.GeneralUtils
{
    public record LogDependencies(
        Action<Action> UnityLogWrap
    );
    
    public static class LogDependenciesUtils 
    {
        public static void UnityLogInserted(Action logAction)
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