using System.Threading.Tasks;
using AppStructure;
using DingoProjectAppStructure.Core.AppRootCore;
using UnityEngine;

namespace DingoProjectAppStructure.StateRoots
{
    public class LogAppStateElementBehaviour : AppStateElementBehaviour
    {
        public override Task EnableElementAsync(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Enable {transferInfo}: {name}", this);
            return base.EnableElementAsync(transferInfo);
        }

        public override Task DisableElementAsync(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Disable {transferInfo}: {name}", this);
            return base.DisableElementAsync(transferInfo);
        }
    }
}