using System.Collections.Generic;
using System.Threading.Tasks;
using AppStructure;
using AppStructure.StateMachines;

namespace ProjectAppStructure.Core
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