using System.Collections.Generic;
using AppStructure;
using AppStructure.StateMachines;

namespace DingoProjectAppStructure.Core
{
    public class AppStateMachine : GoBackSupportStateMachine<string>
    {
        protected override string NoneState => "";

        protected override bool IsValidBack(IReadOnlyList<TransferInfo<string>> history, out TransferInfo<string> firstBack)
        {
            firstBack = default;
            return false;
        }
    }
}