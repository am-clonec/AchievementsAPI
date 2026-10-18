namespace AchievementsAPI.Backend;

internal static class Strings
{
    public static StringNames Register(string text) =>
        Reactor.Localization.Utilities.CustomStringName.CreateAndRegister(text);
}
