using System;
using AYellowpaper.SerializedCollections;
using DingoProjectAppStructure.Core.Model;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
using DingoUnityExtensions.UnityViewProviders.Navigation;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using UnityEngine;

namespace DingoProjectAppStructure.GenericView.Popups
{
    public class ModalButton : ValueContainer<(Action action, ModalButtonKey key)>
    {
        [SerializeField] private EventContainer _button;
        [SerializeField] private ValueContainer<string> _title;
        [SerializeField] private SerializedDictionary<ButtonMood, ToggleSwapInfoBase> _toggleFromMood;
        [SerializeField] private bool _swapImmediately;
        [SerializeField] private ContainerNavigationNode _navigationNode;
        
        private ToggleSwapInfoBase _swap;

        public GameObject NavigationGameObject => _navigationNode != null ? _navigationNode.gameObject : null;

        public void MakeFirst()
        {
            if (_navigationNode == null)
                return;
            
            _navigationNode.TagEdgeNavigationCase(EdgeNavigationCase.First);
            _navigationNode.LoopFindEdgeNavigationCase(EdgeNavigationCase.First, true, true);
        }

        public void MakeLast()
        {
            if (_navigationNode == null)
                return;
            
            _navigationNode.TagEdgeNavigationCase(EdgeNavigationCase.Last);
            _navigationNode.LoopFindEdgeNavigationCase(EdgeNavigationCase.Last, true, true);
        }

        public void ResetNavigationPosition()
        {
            if (_navigationNode == null)
                return;

            _navigationNode.TagEdgeNavigationCase(EdgeNavigationCase.None);
            _navigationNode.LoopFindEdgeNavigationCase(EdgeNavigationCase.None);
        }
        
        protected override void OnSetInteractable(bool value)
        {
            _button.Interactable = value;
            base.OnSetInteractable(value);
        }

        protected override void SetValueWithoutNotify((Action action, ModalButtonKey key) value)
        {
            if (_swap != null)
                _swap.SetViewActive(false.TimeContext(_swapImmediately));
            if (_toggleFromMood.TryGetValue(value.key.Mood, out _swap))
                _swap.SetViewActive(true.TimeContext(_swapImmediately));
            _title.UpdateValueWithoutNotify(value.key.Key);
        }

        protected override void OnSelected(bool value) => _button.Selected = value;

        private void Invoke() => Value.action?.Invoke();
        protected override void SubscribeOnly()
        {
            _button.SafeSubscribe(Invoke);
        }

        protected override void UnsubscribeOnly()
        {
            _button.UnSubscribe(Invoke);
        }
    }
}