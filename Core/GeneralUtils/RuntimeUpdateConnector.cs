using System;
using DingoUnityExtensions;

namespace ProjectAppStructure.Core.GeneralUtils
{
    public record RuntimeDependencies(
        RuntimeUpdateConnector RuntimeUpdateConnector,
        RuntimeDelegateConnector RuntimeDelegateConnector,
        RuntimeSingleCallDelegateConnector RuntimeSingleCallDelegateConnector
    );
    
    public record RuntimeUpdateConnector(
        Action<IUpdater, int> AddUpdate,
        Action<IUpdater> RemoveUpdate,
        Action<ILateUpdater, int> AddLateUpdate,
        Action<ILateUpdater> RemoveLateUpdate,
        Action<IFixedUpdater, int> AddFixedUpdate,
        Action<IFixedUpdater> RemoveFixedUpdate);
    
    public record RuntimeDelegateConnector(
        Action<object, Action, int> AddUpdate,
        Action<object> RemoveUpdate,
        Action<object, Action, int> AddLateUpdate,
        Action<object> RemoveLateUpdate,
        Action<object, Action, int> AddFixedUpdate,
        Action<object> RemoveFixedUpdate);

    public record RuntimeSingleCallDelegateConnector(
        Func<Action, Action> AddUpdate,
        Func<Action, Action> AddLateUpdate,
        Func<Action, Action> AddFixedUpdate);
}