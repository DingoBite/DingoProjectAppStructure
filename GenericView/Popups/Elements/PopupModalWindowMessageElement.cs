using System.Linq;
using System.Threading.Tasks;
using Bind;
using DingoUnityExtensions.Pools;
using DingoUnityExtensions.UnityViewProviders.Core;
using ProjectAppStructure.Core.AppRootCore;
using ProjectAppStructure.Core.Model;
using UnityEngine;

namespace ProjectAppStructure.GenericView.Popups.Elements
{
    public class PopupModalWindowMessageElement : PopupStateElementBehaviour
    {
        [SerializeField] private PoolBehaviour<ModalButton> _buttonsPool;
        [SerializeField] private ValueContainer<string> _title;
        [SerializeField] private ValueContainer<string> _message;
        
        private AppPopupMessageModel _popupMessageModel;

        public override Task BindAsync(AppModelRoot appModel)
        {
            _popupMessageModel = appModel.Model<AppPopupMessageModel>();
            return base.BindAsync(appModel);
        }

        private void ModelWindowMessageChange(ModalWindowMessage message)
        {
            _title.UpdateValueWithoutNotify(message.Title);
            _message.UpdateValueWithoutNotify(message.Message);
            _buttonsPool.Clear();
            if (message.AddictiveActions == null)
                return;
            
            foreach (var (key, action) in message.AddictiveActions.OrderBy(e => e.Key.order).ThenByDescending(e => e.Key.Mood))
            {
                var button = _buttonsPool.PullElement();
                button.UpdateValueWithoutNotify((action, key));
            }
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