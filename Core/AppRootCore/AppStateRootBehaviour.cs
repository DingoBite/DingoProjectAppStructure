using AppStructure;
using AppStructure.BaseElements;
using ProjectAppStructure.Core.Model;

namespace ProjectAppStructure.Core.AppRootCore
{
    public abstract class AppStateRootBehaviour : ImmediateAppStateRoot<string, AppModelRoot> { }
    public abstract class AnimatableAppStateBehaviour : GenericAnimatableAppStateRoot<string, AppModelRoot> { }
    public abstract class AppStateElementBehaviour : StateViewElement<string, AppModelRoot> { }
    public abstract class AppStaticElementBehaviour : StaticViewElement<AppModelRoot> { }
    public abstract class AppStateStaticElementBehaviour : StaticStateViewElement<string, AppModelRoot> { }
}