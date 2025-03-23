using System;
using System.Collections;
using System.Threading.Tasks;
using DingoUnityExtensions;
using ProjectAppStructure.SceneRoot;
using UnityEngine;

namespace ProjectAppStructure.Core.AppLock
{
    [Flags]
    public enum AppInputLockConfigure
    {
        None = 0,
        ShowPreloader = 1 << 0,
    }
    
    public struct AppInputLockMessage
    {
        public readonly AppInputLockConfigure ConfigureFlags;
        
        public AppInputLockMessage(AppInputLockConfigure configureFlags)
        {
            ConfigureFlags = configureFlags;
        }
    }
    
    public static class AppLock
    {
        public static void AppSyncLockTime(float time, Action callback = null, ushort lockFlag = 0, AppInputLockMessage lockMessage = default)
        {
            if (lockMessage.ConfigureFlags == AppInputLockConfigure.None)
                lockMessage = new AppInputLockMessage(AppInputLockConfigure.ShowPreloader);
            G.Lock.Enable(lockMessage, lockFlag);
            try
            {
                CoroutineParent.InvokeAfterSecondsWithCanceling(G.Lock, time, () =>
                {
                    G.Lock.Disable(lockFlag);
                    callback?.Invoke();
                });
            }
            catch (Exception e)
            {
                G.Lock.Disable(lockFlag);
                Debug.LogException(e);
            }
        }
        
        public static async Task AppAsyncLockAction(Func<Task> awaitFunc, Action callback = null, ushort lockFlag = 0, AppInputLockMessage lockMessage = default)
        {
            if (lockMessage.ConfigureFlags == AppInputLockConfigure.None)
                lockMessage = new AppInputLockMessage(AppInputLockConfigure.ShowPreloader);
            G.Lock.Enable(lockMessage, lockFlag);
            try
            {
                await awaitFunc();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            G.Lock.Disable(lockFlag);
            callback?.Invoke();
        }
        
        public static IEnumerator AppAsyncLockActionCoroutine(Func<Task> awaitFunc, Action callback = null, ushort lockFlag = 0, AppInputLockMessage lockMessage = default)
        {
            if (lockMessage.ConfigureFlags == AppInputLockConfigure.None)
                lockMessage = new AppInputLockMessage(AppInputLockConfigure.ShowPreloader);
            G.Lock.Enable(lockMessage, lockFlag);
            Task task = null;
            try
            {
                task = awaitFunc();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            if (task != null)
            {
                while (!task.IsCompleted)
                {
                    yield return null;
                }
            }

            G.Lock.Disable(lockFlag);
            callback?.Invoke();
        }
    }
}