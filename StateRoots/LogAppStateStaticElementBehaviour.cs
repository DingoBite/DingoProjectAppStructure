using AppStructure;
using ProjectAppStructure.Core.AppRootCore;
using UnityEngine;

namespace ProjectAppStructure.StateRoots
{
    public class LogAppStateStaticElementBehaviour : AppStateStaticElementBehaviour
    {
        public override void Transfer(TransferInfo<string> transferInfo)
        {
            Debug.Log($"Transfer {transferInfo}", this);
            base.Transfer(transferInfo);
        }
    }
}