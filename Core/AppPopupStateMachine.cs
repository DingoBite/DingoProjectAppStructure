using AppStructure.StateMachines;

namespace DingoProjectAppStructure.Core
{
    public class AppPopupStateMachine : OpenCloseStateMachine<string>
    {
        protected override string NoneState => "";
    }
}