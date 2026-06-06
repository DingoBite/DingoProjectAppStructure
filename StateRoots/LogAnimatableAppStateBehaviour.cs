using System.Threading.Tasks;
using AppStructure;
using DingoProjectAppStructure.Core.AppRootCore;
using UnityEngine;

namespace DingoProjectAppStructure.StateRoots
{
    public class LogAnimatableAppStateBehaviour : AnimatableAppStateBehaviour
    {
        public override void EnableOnTransfer(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Enable {transferInfo}", this);
            base.EnableOnTransfer(transferInfo);
        }

        public override void DisableOnTransfer(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Disable {transferInfo}", this);
            base.DisableOnTransfer(transferInfo);
        }
    }
}