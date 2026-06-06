using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoProjectAppStructure.Core.Model;
using UnityEngine;

namespace DingoProjectAppStructure.Core
{
    public interface IAppPopupController
    {
        public void Open(string popup, object parameters);
        public void Close();
    }
    
    public class AppPopupStateController : MonoBehaviour, IAppPopupController
    {
        [SerializeField] private AppPopupStateMachine _appPopupStateMachine;
        [SerializeField] private AppPopupViewRoot _appPopupViewRoot;

        public IAppStructurePart<AppModelRoot> AppPopupViewRoot => _appPopupViewRoot;
            
        public List<string> States
        {
            get
            {
                var states = _appPopupViewRoot?.States;
                var list = states?.ToList();
                list ??= new List<string>();
                if (list.Count == 0)
                    list.Add("__EMPTY__");
                return list;
            }
        }
        
        public void Open(string popup, object parameters)
        {
            var transferInfo = _appPopupStateMachine.OpenState(popup);
            transferInfo.Parameters = parameters;
            _appPopupViewRoot.ApplyTransfer(transferInfo);
        }

        public void Close()
        {
            var transferInfo = _appPopupStateMachine.CloseLastState();
            _appPopupViewRoot.ApplyTransfer(transferInfo);
        }
    }
}