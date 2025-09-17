using System.Collections.Generic;
using DingoProjectAppStructure.Core;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoProjectAppStructure.SceneRoot
{
    public abstract class SubGameController<TThis> : ProtectedSingletonBehaviour<TThis> where TThis : SubGameController<TThis>
    {
        [SerializeField] private AppStateController _appStateController;
        
        public static IStateController State => Instance._appStateController;
        public static List<string> States => Instance._appStateController.States;
    }
}