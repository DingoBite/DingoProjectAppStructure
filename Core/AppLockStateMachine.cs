using AppStructure.StateMachines;

namespace ProjectAppStructure.Core
{
    public class AppLockStateMachine : OpenCloseStateMachine<string>
    {
        protected override string NoneState => "";
    }
}