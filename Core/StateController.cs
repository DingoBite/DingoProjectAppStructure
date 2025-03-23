using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using NaughtyAttributes;
using ProjectAppStructure.Core.AppRootCore;
using ProjectAppStructure.Core.Model;
using UnityEngine;

namespace ProjectAppStructure.Core
{
    public class StateController : MonoBehaviour, IStateController
    {
        [SerializeField] private AppStateMachine _appCoreStateMachine;
        [SerializeField] private AppStateElementsRoot _appStateElementsRoot;

        [SerializeField, Dropdown(nameof(States))] private string _bootstrapState;
        [SerializeField, Dropdown(nameof(States))] private string _startState;
        
        public List<string> States
        {
            get
            {
                var states = _appStateElementsRoot.States;
                var list = states.ToList();
                if (list.Count == 0)
                    list.Add("__EMPTY__");
                return list;
            }
        }

        public IAppStructurePart<AppModelRoot> AppViewRoot => _appStateElementsRoot;
        
        public async Task GoToBootstrap()
        {
            var t = _appCoreStateMachine.GoToState(_bootstrapState);
            await _appStateElementsRoot.ApplyTransferAsync(t);
        }

        public async Task GoToStart()
        {
            var t = _appCoreStateMachine.GoToState(_startState);
            await _appStateElementsRoot.ApplyTransferAsync(t);
        }

        public async Task GoToAsync(string appState)
        {
            var t = _appCoreStateMachine.GoToState(appState);
            await _appStateElementsRoot.ApplyTransferAsync(t);
        }
    }
}