using AppStructure.StateMachines;

namespace DingoProjectAppStructure.Core
{
    public class AppLockStateMachine : OpenCloseStateMachine<string>
    {
        protected override string NoneState => "";
    }
}