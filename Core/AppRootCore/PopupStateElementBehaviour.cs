using AppStructure;
using AppStructure.BaseElements;
using DingoProjectAppStructure.Core.Model;

namespace DingoProjectAppStructure.Core.AppRootCore
{
    public abstract class PopupStateElementBehaviour : StateViewElement<string, AppModelRoot>
    {
        protected object Parameters { get; private set; }
        
        protected ModalWindowMessage ModalWindowMessage => Parameters as ModalWindowMessage;
        
        public override void OnStartStateEnable(TransferInfo<string> transferInfo)
        {
            Parameters = transferInfo.Parameters;
            base.OnStartStateEnable(transferInfo);
        }
    }
}