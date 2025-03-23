using System.Threading.Tasks;
using AppStructure;
using ProjectAppStructure.Core.AppRootCore;
using UnityEngine;

namespace ProjectAppStructure.StateRoots
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