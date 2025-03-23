using System;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.UnityViewProviders.Core;
using ProjectAppStructure.Core.Model;
using UnityEngine;

namespace ProjectAppStructure.GenericView.Popups
{
    public class ModalButton : ValueContainer<(Action action, ModalButtonKey key)>
    {
        [SerializeField] private EventContainer _button;
        [SerializeField] private ValueContainer<string> _title;
        [SerializeField] private ValueContainer<Color> _color;
        [SerializeField] private SerializedDictionary<ButtonMood, Color> _colorsFromMood;

        protected override void SetValueWithoutNotify((Action action, ModalButtonKey key) value)
        {
            if (_colorsFromMood.TryGetValue(value.key.Mood, out var color))
                _color.UpdateValueWithoutNotify(color);
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