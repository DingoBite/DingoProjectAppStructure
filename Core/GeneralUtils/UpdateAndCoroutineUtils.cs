using DingoUnityExtensions;

namespace ProjectAppStructure.Core.GeneralUtils
{
    public static class UpdateAndCoroutineUtils
    {
        private static RuntimeDependencies _runtimeDependencies;
        private static RuntimeUpdateConnector _runtimeUpdateConnector;
        private static RuntimeDelegateConnector _runtimeDelegateConnector;
        private static RuntimeSingleCallDelegateConnector _singleCallDelegateConnector;

        public static RuntimeUpdateConnector MakeRuntimeUpdateConnector()
        {
            _runtimeUpdateConnector ??= new RuntimeUpdateConnector(
                CoroutineParent.AddUpdater, CoroutineParent.RemoveUpdater,
                CoroutineParent.AddLateUpdater, CoroutineParent.RemoveLateUpdater,
                CoroutineParent.AddFixedUpdater, CoroutineParent.RemoveFixedUpdater
            );
            return _runtimeUpdateConnector;
        }
        
        public static RuntimeDelegateConnector MakeRuntimeDelegateConnector()
        {
            _runtimeDelegateConnector ??= new RuntimeDelegateConnector(
                CoroutineParent.AddUpdater, CoroutineParent.RemoveUpdater,
                CoroutineParent.AddLateUpdater, CoroutineParent.RemoveLateUpdater,
                CoroutineParent.AddFixedUpdater, CoroutineParent.RemoveFixedUpdater
            );
            return _runtimeDelegateConnector;
        }
        
        public static RuntimeSingleCallDelegateConnector MakeRuntimeSingleCallDelegateConnector()
        {
            _singleCallDelegateConnector ??= new RuntimeSingleCallDelegateConnector(
                CoroutineParent.AddSingleUpdate,
                CoroutineParent.AddSingleLateUpdate,
                CoroutineParent.AddSingleFixedUpdate
            );
            return _singleCallDelegateConnector;
        }

        public static RuntimeDependencies MakeRuntimeDependencies()
        {
            _runtimeDependencies ??= new RuntimeDependencies(
                MakeRuntimeUpdateConnector(),
                MakeRuntimeDelegateConnector(),
                MakeRuntimeSingleCallDelegateConnector()
            );
            return _runtimeDependencies;
        }
    }
}