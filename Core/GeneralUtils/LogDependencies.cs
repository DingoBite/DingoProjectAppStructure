using System;

namespace ProjectAppStructure.Core.GeneralUtils
{
    public record LogDependencies(
        Action<Action> UnityLogWrap
    );
}