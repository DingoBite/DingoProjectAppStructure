using DingoProjectAppStructure.Core.Config;
using DingoProjectAppStructure.Core.Model;
using DingoProjectAppStructure.Core.ViewModel;

namespace DingoProjectAppStructure.Core.GeneralUtils
{
    public static class AppModelGetExtensions
    {
        public static AppViewModelRoot ViewModel(this AppModelRoot appModelRoot) => appModelRoot.Get<AppViewModelRootContainer>().Root;
        public static AppConfigRoot Configs(this AppModelRoot appModelRoot) => appModelRoot.ExternalDependencies.Get<AppConfigRoot>();
    }
}