using System.Threading.Tasks;
using AppStructure;
using DingoProjectAppStructure.Core.AppRootCore;
using UnityEngine;

namespace DingoProjectAppStructure.StateRoots
{
    public class LogAppStateElementBehaviour : AppStateElementBehaviour
    {
        [SerializeField] private bool _onEnableLogging = true;
        [SerializeField] private bool _onDisableLogging = true;
        
        public override Task EnableElementAsync(TransferInfo<string> transferInfo)
        {
            if (_onEnableLogging)
                Debug.Log($"Enable {transferInfo}: {name}", this);
            return base.EnableElementAsync(transferInfo);
        }

        public override Task DisableElementAsync(TransferInfo<string> transferInfo)
        {
            if (_onDisableLogging)
                Debug.Log($"Disable {transferInfo}: {name}", this);
            return base.DisableElementAsync(transferInfo);
        }
    }
}