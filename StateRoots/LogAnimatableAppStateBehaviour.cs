using System.Threading.Tasks;
using AppStructure;
using ProjectAppStructure.Core.AppRootCore;
using UnityEngine;

namespace ProjectAppStructure.StateRoots
{
    public class LogAnimatableAppStateBehaviour : AnimatableAppStateBehaviour
    {
        public override Task EnableOnTransferAsync(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Enable {transferInfo}", this);
            return base.EnableOnTransferAsync(transferInfo);
        }

        public override Task DisableOnTransferAsync(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Disable {transferInfo}", this);
            return base.DisableOnTransferAsync(transferInfo);
        }
    }
}