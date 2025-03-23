using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectAppStructure.Core
{
    public interface IStateController
    {
        public Task GoToAsync(string appState);
        public List<string> States { get; }
    }
}