using AppStructure;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoProjectAppStructure.GenericView.Popups.Elements
{
    public class PopupInfoMessageElement : PopupStateElementBehaviour
    {
        [SerializeField] private ValueContainer<string> _title;
        [SerializeField] private ValueContainer<string> _message;

        public override void OnStartStateEnable(TransferInfo<string> transferInfo)
        {
            base.OnStartStateEnable(transferInfo);
            if (ModalWindowMessage == null)
                return;
            _title.UpdateValueWithoutNotify(ModalWindowMessage?.Title);
            _message.UpdateValueWithoutNotify(ModalWindowMessage?.Message);
        }
    }
}