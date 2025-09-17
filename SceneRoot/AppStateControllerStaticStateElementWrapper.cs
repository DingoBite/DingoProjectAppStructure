using System.Threading.Tasks;
using DingoProjectAppStructure.Core;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoProjectAppStructure.Core.Model;
using UnityEngine;

namespace DingoProjectAppStructure.SceneRoot
{
    public class AppStateControllerStaticStateElementWrapper : AppStaticElementBehaviour
    {
        [SerializeField] private AppStateController _appStateController;
        
        public override void PreInitialize()
        {
            _appStateController.AppViewRoot.PreInitialize();
            base.PreInitialize();
        }

        public override async Task InitializeAsync()
        {
            await _appStateController.AppViewRoot.InitializeAsync();
            await base.InitializeAsync();
        }

        public override async Task BindAsync(AppModelRoot appModel)
        {
            await _appStateController.AppViewRoot.BindAsync(appModel);
            await base.BindAsync(appModel);
        }

        public override async Task PostInitializeAsync()
        {
            await _appStateController.AppViewRoot.PostInitializeAsync();
            await base.PostInitializeAsync();
        }
    }
}