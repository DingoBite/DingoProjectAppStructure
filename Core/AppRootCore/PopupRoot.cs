using System.Collections.Generic;
using System.Threading.Tasks;
using AppStructure;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.SceneRoot;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Navigation;
using UnityEngine;

namespace DingoProjectAppStructure.Core.AppRootCore
{
    public class PopupRoot : GenericAnimatableAppStateRoot<string, AppModelRoot>
    {
        [SerializeField] private List<EventContainer> _closeButtons;

        private object _parameters;

        private ModalWindowMessage ModalWindowMessage => _parameters as ModalWindowMessage;
        
        public override void PreInitialize()
        {
            _closeButtons.ForEach(e => e.SafeSubscribe(CloseLast));
            SetDefaultValues();
        }

        public override Task EnableOnTransferAsync(TransferInfo<string> transferInfo)
        {
            _parameters = transferInfo.Parameters;
            EscapeRouter.AddAction(Close);
            return base.EnableOnTransferAsync(transferInfo);
        }

        protected override void StartDisable(TransferInfo<string> transferInfo)
        {
            EscapeRouter.RemoveAction(Close);
            base.StartDisable(transferInfo);
        }

        private EscapeResult Close()
        {
            if (ModalWindowMessage == null || ModalWindowMessage.CanBeIgnored)
            {
                G.Popup.CloseAsync();
                return EscapeResult.Closed;
            }

            return EscapeResult.Block;
        }

        private void CloseLast()
        {
            if (ModalWindowMessage == null || ModalWindowMessage.CanBeIgnored)
                G.Popup.CloseAsync();
        }
    }
}