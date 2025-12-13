using System.Linq;
using AppStructure;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoProjectAppStructure.Core.Model;
using DingoUnityExtensions.Pools.Core;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoProjectAppStructure.GenericView.Popups.Elements
{
    public class PopupModalWindowMessageElement : PopupStateElementBehaviour
    {
        [SerializeField] private PoolBehaviour<ModalButton> _buttonsPool;
        [SerializeField] private ValueContainer<string> _title;
        [SerializeField] private ValueContainer<string> _message;

        public override void OnStartStateEnable(TransferInfo<string> transferInfo)
        {
            base.OnStartStateEnable(transferInfo);
            ModelWindowMessageChange(ModalWindowMessage);
        }

        private void ModelWindowMessageChange(ModalWindowMessage message)
        {
            if (message == null)
                return;
            
            _title.SetActiveContainer(!string.IsNullOrWhiteSpace(message.Title));
            _title.UpdateValueWithoutNotify(message.Title);
            _message.SetActiveContainer(!string.IsNullOrWhiteSpace(message.Message));
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
    }
}