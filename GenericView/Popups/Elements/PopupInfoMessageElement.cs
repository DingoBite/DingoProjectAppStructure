using System.Threading.Tasks;
using Bind;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoProjectAppStructure.Core.Model;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoProjectAppStructure.GenericView.Popups.Elements
{
    public class PopupInfoMessageElement : PopupStateElementBehaviour
    {
        [SerializeField] private ValueContainer<string> _title;
        [SerializeField] private ValueContainer<string> _message;

        private AppPopupMessageModel _popupMessageModel;

        public override Task BindAsync(AppModelRoot appModel)
        {
            _popupMessageModel = appModel.Get<AppPopupMessageModel>();
            return base.BindAsync(appModel);
        }

        private void ModelWindowMessageChange(ModalWindowMessage message)
        {
            _title.UpdateValueWithoutNotify(message.Title);
            _message.UpdateValueWithoutNotify(message.Message);
        }
        
        protected override void SubscribeOnly()
        {
            _popupMessageModel.ModalWindowMessage.SafeSubscribeAndSet(ModelWindowMessageChange);
        }

        protected override void UnsubscribeOnly()
        {
            _popupMessageModel.ModalWindowMessage.UnSubscribe(ModelWindowMessageChange);
        }
    }
}