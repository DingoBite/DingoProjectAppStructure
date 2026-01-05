using System.Linq;
using AppStructure;
using AppStructure.BaseNavigation;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoProjectAppStructure.Core.Model;
using DingoUnityExtensions.Pools.Core;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Navigation;
using UnityEngine;

namespace DingoProjectAppStructure.GenericView.Popups.Elements
{
    public class PopupModalWindowMessageElement : PopupStateElementBehaviour
    {
        [SerializeField] private PoolBehaviour<ModalButton> _buttonsPool;
        [SerializeField] private ValueContainer<string> _title;
        [SerializeField] private ValueContainer<string> _message;
        [SerializeField] private DefaultFocusElement _defaultFocusElement;
        [SerializeField] private ContainerNavigationRoute _containerNavigationRoute;

        public override void OnStartStateEnable(TransferInfo<string> transferInfo)
        {
            base.OnStartStateEnable(transferInfo);
            ModelWindowMessageChange(ModalWindowMessage);
        }

        private void ModelWindowMessageChange(ModalWindowMessage message)
        {
            if (_containerNavigationRoute != null)
                _containerNavigationRoute.SetDirty();
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
            
            for (var i = 0; i < _buttonsPool.PulledElements.Count; i++)
            {
                var button = _buttonsPool.PulledElements[i];
                if (i == 0)
                    button.MakeFirst();
                else if (i == _buttonsPool.PulledElements.Count - 1)
                    button.MakeLast();
                else
                    button.ResetNavigationPosition();
            }

            if (_defaultFocusElement != null)
                _defaultFocusElement.RewriteFocusElement(_buttonsPool.PulledElements[0].NavigationGameObject);
        }
    }
}