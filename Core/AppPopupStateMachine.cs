using AppStructure.StateMachines;

namespace ProjectAppStructure.Core
{
    public class AppPopupStateMachine : OpenCloseStateMachine<string>
    {
        protected override string NoneState => "";
    }
}