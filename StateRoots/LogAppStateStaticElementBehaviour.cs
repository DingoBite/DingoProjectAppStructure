using AppStructure;
using DingoProjectAppStructure.Core.AppRootCore;
using UnityEngine;

namespace DingoProjectAppStructure.StateRoots
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