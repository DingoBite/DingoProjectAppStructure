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
        
        public override void EnableElement(TransferInfo<string> transferInfo)
        {
            if (_onEnableLogging)
                Debug.Log($"Enable {transferInfo}: {name}", this);
            base.EnableElement(transferInfo);
        }

        public override void DisableElement(TransferInfo<string> transferInfo)
        {
            if (_onDisableLogging)
                Debug.Log($"Disable {transferInfo}: {name}", this);
            base.DisableElement(transferInfo);
        }
    }
}