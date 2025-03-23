using System.Collections.Generic;
using AppStructure.BaseElements;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.UnityViewProviders.Core;
using ProjectAppStructure.Core.Model;
using ProjectAppStructure.SceneRoot;
using UnityEngine;

namespace ProjectAppStructure.Core.AppRootCore
{
    public abstract class PopupRoot : GenericAnimatableAppStateRoot<string, AppModelRoot>
    {
        [SerializeField] private List<EventContainer> _closeButtons;

        public override void PreInitialize()
        {
            _closeButtons.ForEach(e => e.SafeSubscribe(CloseLast));
            SetDefaultValues();
        }

        private static void CloseLast() => G.Popup.CloseAsync();
    }

    public abstract class PopupStateElementBehaviour : StateViewElement<string, AppModelRoot>
    {
    }
}