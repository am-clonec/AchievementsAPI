using AchievementsAPI;
using AchievementsAPI.API;
using BepInEx;
using BepInEx.Unity.IL2CPP;
#if !VENT_FRAMEWORK
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
#endif

namespace AchievementsAPI.Developer;

/// Marks the local player as a developer. Build one of these per developer (change DeveloperName) and
/// only give the DLL to that developer; nobody else needs it (ModFlags.None).
[BepInAutoPlugin("achievementsapi.developer", "Achievements API Developer")]
[BepInProcess("Among Us.exe")]
#if VENT_FRAMEWORK
[BepInDependency(VentLib.Vents.Id)]
#else
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.None)]
#endif
[BepInDependency(AchievementsAPIPlugin.Id)]
public partial class DeveloperPlugin : BasePlugin
{
    private const string DeveloperName = "satire";

    public override void Load() => DeveloperPresence.RegisterLocalDeveloper(DeveloperName);
}
