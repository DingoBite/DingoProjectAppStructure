using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.Core.ViewModel;

namespace DingoProjectAppStructure.Core.GeneralUtils
{
    public static class AppModelGetExtensions
    {
        public static AppViewModelRoot ViewModel(this AppModelRoot appModelRoot) => appModelRoot.Model<AppViewModelRootContainer>().Root;
    }
}