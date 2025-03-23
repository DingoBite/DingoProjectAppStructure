using System;
using System.Collections.Generic;
using Bind;

namespace ProjectAppStructure.Core.Model
{
    public enum ButtonMood
    {
        Danger,
        Warning,
        Neutral,
        Good,
    }

    public record ModalButtonKey(string Key, ButtonMood Mood, int order = 0);
    
    public struct ModalWindowMessage
    {
        public readonly string Title;
        public readonly string Message;
        public readonly IReadOnlyDictionary<ModalButtonKey, Action> AddictiveActions;
        
        public ModalWindowMessage(string title, string message, IReadOnlyDictionary<ModalButtonKey, Action> addictiveActions = null)
        {
            Title = title;
            Message = message;
            AddictiveActions = addictiveActions;
        }
    }
    
    public class AppPopupMessageModel : AppModelBase
    {
        public readonly Bind<ModalWindowMessage> ModalWindowMessage = new();
    }
}