using System.Threading.Tasks;
using AppStructure.BaseElements;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoProjectAppStructure.Core.Model;
using UnityEngine;

namespace DingoProjectAppStructure.Core
{
    public interface IAppPopupController
    {
        public Task OpenAsync(string popup);
        public Task CloseAsync();
    }
    
    public class AppPopupStateController : MonoBehaviour, IAppPopupController
    {
        [SerializeField] private AppPopupStateMachine _appPopupStateMachine;
        [SerializeField] private AppPopupViewRoot _appPopupViewRoot;

        public IAppStructurePart<AppModelRoot> AppPopupViewRoot => _appPopupViewRoot;
            
        public async Task OpenAsync(string popup)
        {
            var transferInfo = _appPopupStateMachine.OpenState(popup);
            await _appPopupViewRoot.ApplyTransferAsync(transferInfo);
        }

        public async Task CloseAsync()
        {
            var transferInfo = _appPopupStateMachine.CloseLastState();
            await _appPopupViewRoot.ApplyTransferAsync(transferInfo);
        }
    }
}