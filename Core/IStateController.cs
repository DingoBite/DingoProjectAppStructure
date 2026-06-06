using System.Collections.Generic;
using System.Threading.Tasks;

namespace DingoProjectAppStructure.Core
{
    public interface IStateController
    {
        public void GoTo(string appState);
        public List<string> States { get; }
    }
}