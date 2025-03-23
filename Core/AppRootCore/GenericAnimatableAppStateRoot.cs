using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStructure;
using DingoUnityExtensions.Tweens;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectAppStructure.Core.AppRootCore
{
    public class GenericAnimatableAppStateRoot<TState, TAppModel> : AppStateRoot<TState, TAppModel>
    {
        [Foldout("Events")]
        [SerializeField] private UnityEvent _startEnable;
        [Foldout("Events")]
        [SerializeField] private UnityEvent _fullEnable;
        [Foldout("Events")]
        [SerializeField] private UnityEvent _startDisable;
        [Foldout("Events")]
        [SerializeField] private UnityEvent _fullDisable;
        
        [SerializeField] private List<AnimatableBehaviour> _animatableBehaviours;
            
        private Canvas _canvas;

        protected Canvas Canvas => _canvas ??= GetComponent<Canvas>();

        private List<AnimatableBehaviour> AnimatableBehaviours => _animatableBehaviours;

        protected override void StartEnable(TransferInfo<TState> transferInfo)
        {
            _startEnable?.Invoke();
            if (Canvas != null)
                Canvas.enabled = true;
            base.StartEnable(transferInfo);
        }

        protected override void StartDisable(TransferInfo<TState> transferInfo)
        {
            _startDisable?.Invoke();
            base.StartDisable(transferInfo);
        }

        public override Task DisableOnTransferAsync(TransferInfo<TState> transferInfo)
        {
            StartDisable(transferInfo);
            if (AnimatableBehaviours.Count == 0)
                DisableCompletely(transferInfo);
            else
                AnimatableBehaviours.ForEach(b => b.Disable(onComplete: () => DisableAllHandle(transferInfo)));
            return base.DisableOnTransferAsync(transferInfo);
        }

        protected override void DisableCompletely(TransferInfo<TState> transferInfo)
        {
            base.DisableCompletely(transferInfo);
            if (Canvas != null)
                Canvas.enabled = false;
            SetDefaultValues();
            _fullDisable?.Invoke();
        }

        protected virtual void EnableCompletely()
        {
            _fullEnable?.Invoke();
        }
        
        public override Task EnableOnTransferAsync(TransferInfo<TState> transferInfo)
        {
            StartEnable(transferInfo);
            AnimatableBehaviours.ForEach(b => b.Enable(onComplete:EnableFirstHandle));
            return base.EnableOnTransferAsync(transferInfo);
        }

        private void DisableAllHandle(TransferInfo<TState> transferInfo)
        {
            if (AnimatableBehaviours.Any(animatableBehaviour => !animatableBehaviour.IsFullyDisabled))
                return;

            DisableCompletely(transferInfo);
        }

        private void EnableFirstHandle()
        {
            if (AnimatableBehaviours.Any(animatableBehaviour => !animatableBehaviour.IsFullyEnabled))
                return;
            EnableCompletely();
        }
        
        protected override void SetDefaultValues()
        {
            IsActive = false;
            
            if (Canvas != null)
                Canvas.enabled = false;

            gameObject.SetActive(false);
            AnimatableBehaviours.ForEach(b => b.DisableImmediately());
        }
    }
}