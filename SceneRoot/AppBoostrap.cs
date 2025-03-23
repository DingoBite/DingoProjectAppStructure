using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectAppStructure.SceneRoot
{
    public class AppBoostrap : AppStructure.AppBootstrap
    {
        [SerializeField] private G _g;

        protected override IEnumerator PrepareBootstrapProcess(Action<bool> callback)
        {
            _g.PrepareController();
            yield return _g.InitializeControllerAsync(callback).ToCoroutine();
        }

        protected override IEnumerator StartBootstrapProcess(Action<bool> callback)
        {
            yield return _g.BindAsync(callback).ToCoroutine();
        }

        protected override IEnumerator FinalizeBootstrapProcess(Action<bool> callback)
        {
            yield return _g.FinalizeAsync(callback).ToCoroutine();
        }
    }
}