using ProjectAppStructure.Core.Model;
using ProjectAppStructure.Core.ViewModel;

namespace ProjectAppStructure.Core.GeneralUtils
{
    public static class AppModelGetExtensions
    {
        public static AppViewModelRoot ViewModel(this AppModelRoot appModelRoot) => appModelRoot.Model<AppViewModelRootContainer>().Root;
    }
}