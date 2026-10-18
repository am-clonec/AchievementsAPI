using System.Collections;

namespace AchievementsAPI.Backend;

internal static class Coroutines
{
    public static IEnumerator Start(IEnumerator coroutine) => Reactor.Utilities.Coroutines.Start(coroutine);

    public static void Stop(IEnumerator coroutine) => Reactor.Utilities.Coroutines.Stop(coroutine);
}
