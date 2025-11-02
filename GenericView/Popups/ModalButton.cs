using System;
using AYellowpaper.SerializedCollections;
using DingoProjectAppStructure.Core.Model;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
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
        
        private ToggleSwapInfoBase _swap;

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