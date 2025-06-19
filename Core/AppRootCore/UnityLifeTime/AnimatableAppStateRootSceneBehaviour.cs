using System.Collections;
using System.Threading.Tasks;
using AppStructure;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.GeneralUtils;
using DingoProjectAppStructure.Core.Model;
using DingoUnityExtensions;

namespace DingoProjectAppStructure.Core.AppRootCore.UnityLifeTime
{
    public abstract class AnimatableAppStateRootSceneBehaviour : AnimatableAppStateBehaviour
    {
        private async Task<AppModelRoot> ModelRootFactoryAsync()
        {
            var dependencies = new ExternalDependencies(
                UpdateAndCoroutineUtils.MakeRuntimeDependencies(),
                new LogDependencies(LogDependenciesUtils.UnityLogInserted
                ));
            var appModelRoot = new AppModelRoot(dependencies);
            await ConstructModelRootAsync(appModelRoot);
            return appModelRoot;
        }

        protected virtual Task ConstructModelRootAsync(AppModelRoot appModelRoot) => Task.CompletedTask;

        private void Awake() => PreInitialize();

        private IEnumerator Start()
        {
            yield return InitializeAsync().AsUniTask().ToCoroutine();
            var task = ModelRootFactoryAsync();
            yield return task.AsUniTask().ToCoroutine();
            yield return BindAsync(task.Result).AsUniTask().ToCoroutine();
            yield return PostInitializeAsync().AsUniTask().ToCoroutine();
        }
        
        private void OnEnable() => CoroutineParent.StartCoroutineWithCanceling((this, nameof(AppStateRootSceneBehaviour)), EnableCoroutine);
        private void OnDisable() => CoroutineParent.StartCoroutineWithCanceling((this, nameof(AppStateRootSceneBehaviour)), DisableCoroutine);

        private IEnumerator EnableCoroutine() => EnableOnTransferAsync(TransferInfo<string>.None).AsUniTask().ToCoroutine();
        private IEnumerator DisableCoroutine() => DisableOnTransferAsync(TransferInfo<string>.None).AsUniTask().ToCoroutine();
    }
}