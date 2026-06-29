using System.Collections;
using System.Threading.Tasks;
using AppStructure;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.Model;

namespace DingoProjectAppStructure.Core.AppRootCore.UnityLifeTime
{
    public abstract class AppStateElementSceneBehaviour : AppStateElementBehaviour
    {
        private async Task<AppModelRoot> ModelRootFactoryAsync()
        {
            var dependencies = new ExternalDependencies();
            var appModelRoot = new AppModelRoot(dependencies);
            await ConstructModelRootAsync(dependencies, appModelRoot);
            return appModelRoot;
        }
        
        protected bool Initialized { get; private set; }

        protected virtual Task ConstructModelRootAsync(ExternalDependencies dependencies, AppModelRoot appModelRoot) => Task.CompletedTask;

        private void Awake() => PreInitialize();

        private IEnumerator Start()
        {
            yield return InitializeAsync().AsUniTask().ToCoroutine();
            var task = ModelRootFactoryAsync();
            yield return task.AsUniTask().ToCoroutine();
            yield return task.Result.PostInitializeAsync().AsUniTask().ToCoroutine();
            yield return BindAsync(task.Result).AsUniTask().ToCoroutine();
            yield return PostInitializeAsync().AsUniTask().ToCoroutine();
            Initialized = true;
        }

        private void OnEnable() => EnableElement(TransferInfo<string>.None);
        private void OnDisable() => DisableElement(TransferInfo<string>.None);
    }
}