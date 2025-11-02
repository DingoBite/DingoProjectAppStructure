using System.Collections.Generic;
using System.Threading.Tasks;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.SceneRoot;
using DingoUnityExtensions.Extensions;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoProjectAppStructure.Core.AppRootCore
{
    public class PopupRoot : GenericAnimatableAppStateRoot<string, AppModelRoot>
    {
        [SerializeField] private List<EventContainer> _closeButtons;
        
        private AppPopupMessageModel _popupMessageModel;

        public override Task<bool> BindAsync(AppModelRoot appModel)
        {
            _popupMessageModel = appModel.Get<AppPopupMessageModel>();
            return base.BindAsync(appModel);
        }

        public override void PreInitialize()
        {
            _closeButtons.ForEach(e => e.SafeSubscribe(CloseLast));
            SetDefaultValues();
        }

        private void CloseLast()
        {
            if (_popupMessageModel.ModalWindowMessage.V.CanBeIgnored)
                G.Popup.CloseAsync();
        }
    }
}