using System.Threading.Tasks;
using AppStructure;
using DingoProjectAppStructure.Core.AppRootCore;
using UnityEngine;

namespace DingoProjectAppStructure.StateRoots
{
    public class LogAppStateRootBehaviour : AppStateRootBehaviour
    {
        public override Task EnableOnTransferAsync(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Enable {transferInfo}", this);
            return base.EnableOnTransferAsync(transferInfo);
        }

        protected override void DisableCompletely(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Disable {transferInfo}", this);
            base.DisableCompletely(transferInfo);
        }
    }
}