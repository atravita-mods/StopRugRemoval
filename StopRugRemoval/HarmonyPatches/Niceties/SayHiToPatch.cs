using AtraShared.Utils.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace StopRugRemoval.HarmonyPatches.Niceties;

/// <summary>
/// Patches for SayHiTo...
/// </summary>
[HarmonyPatch(typeof(NPC))]
internal static class SayHiToPatch
{
#if DEBUG
    [HarmonyPatch(nameof(NPC.sayHiTo))]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony Convention")]
    private static void Postfix(NPC __instance, Character c)
    {
        ModEntry.ModMonitor.DebugOnlyLog($"{__instance.Name} trying to say hi to {c.Name}", LogLevel.Alert);
    }
#endif

    // Short circuit this if there's no player on the map. It literally only handles saying hi to people.
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)] // run after every other prefix.
    [HarmonyPatch(nameof(NPC.performTenMinuteUpdate))]
    private static bool PrefixTenMinuteUpdate(GameLocation l)
        => l == Game1.player.currentLocation;

    // Get the NPCs to pretty frequently greet each other in the saloon?
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(nameof(NPC.performTenMinuteUpdate))]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony convention.")]
    private static void PostfixTenMinuteUpdate(NPC __instance, GameLocation l, int ___textAboveHeadTimer)
    {
        if (Game1.player.currentLocation == l && ___textAboveHeadTimer < 0 && !l.Name.Equals("Saloon", StringComparison.OrdinalIgnoreCase)
            && __instance.isVillager() && __instance.isMoving() && Game1.random.NextDouble() < 0.5)
        {
            // Invert the check here to favor the farmer. :(
            // Goddamnit greet me more often plz.
            Character? c = Utility.isThereAFarmerWithinDistance(__instance.getTileLocation(), 4, l);
            if (c is null)
            {
                Vector2 loc = __instance.getTileLocation();
                foreach (NPC npc in l.characters)
                {
                    if ((npc.getTileLocation() - loc).LengthSquared() <= 16 && __instance.isFacingToward(npc.getTileLocation()))
                    {
                        c = npc;
                        break;
                    }
                }
            }

            if (c is not null)
            {
                __instance.sayHiTo(c);
            }
        }
    }
}
