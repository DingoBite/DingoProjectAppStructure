using System.Collections;
using System.Threading.Tasks;
using AppStructure;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.GeneralUtils;
using DingoProjectAppStructure.Core.Model;

namespace DingoProjectAppStructure.Core.AppRootCore.UnityLifeTime
{
    public abstract class AppStateStaticElementSceneBehaviour : AppStateStaticElementBehaviour
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
        
        private void OnEnable() => Enable(TransferInfo<string>.None);
        private void OnDisable() => Disable(TransferInfo<string>.None);
    }
}