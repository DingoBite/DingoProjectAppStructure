using System;

namespace DingoProjectAppStructure.Core.GeneralUtils
{
    public record LogDependencies(
        Action<Action> UnityLogWrap
    );
}