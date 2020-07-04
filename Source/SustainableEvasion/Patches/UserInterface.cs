using BattleTech;
using BattleTech.UI;
using Harmony;
using SustainableEvasion.Extensions;
using System;

namespace SustainableEvasion.Patches
{
    class UserInterface
    {
        [HarmonyPatch(typeof(CombatSelectionHandler), "addNewState", new Type[] { typeof(SelectionState) })]
        public static class CombatSelectionHandler_addNewState_Patch
        {
            // NOTE: This is not called for AI
            static void Postfix(CombatSelectionHandler __instance, SelectionState newState)
            {
                try
                {
                    Logger.Info("[CombatSelectionHandler_addNewState_POSTFIX] newState.SelectionType: " + newState.SelectionType);
                    Fields.IsJumpPreview = newState.SelectionType == SelectionType.Jump ? true : false;
                    Logger.Debug("[CombatSelectionHandler_addNewState_POSTFIX] Fields.IsJumpPreview: " + Fields.IsJumpPreview);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        // Needed for right click during jump preview which falls back to a normal move...
        [HarmonyPatch(typeof(CombatSelectionHandler), "RemoveTopState", new Type[] { })]
        public static class CombatSelectionHandler_RemoveTopState_Patch
        {
            // NOTE: This is not called for AI
            static void Prefix(CombatSelectionHandler __instance)
            {
                try
                {
                    Logger.Info("[CombatSelectionHandler_RemoveTopState_PREFIX] __instance.ActiveState.SelectionType: " + __instance.ActiveState.SelectionType);
                    if (__instance.ActiveState.SelectionType == SelectionType.Jump)
                    {
                        Fields.IsJumpPreview = false;
                    }
                    Logger.Debug("[CombatSelectionHandler_RemoveTopState_PREFIX] Fields.IsJumpPreview: " + Fields.IsJumpPreview);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }



        [HarmonyPatch(typeof(CombatHUDActorInfo), "RefreshEvasiveDisplay")]
        public static class CombatHUDActorInfo_RefreshEvasiveDisplay_Patch
        {
            static void Postfix(CombatHUDActorInfo __instance, ICombatant ___displayedCombatant, AbstractActor ___displayedActor, Mech ___displayedMech)
            {
                try
                {
                    if (__instance.EvasiveDisplay == null || ___displayedActor == null)
                    {
                        return;
                    }

                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] ___displayedActor: { ___displayedActor.DisplayName}");
                    int sustainableEvasion = ___displayedActor.GetSustainableEvasion();
                    bool jumpRelated = ___displayedActor.JumpedLastRound || Fields.IsJumpPreview;
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] sustainableEvasion: {sustainableEvasion}");
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] jumpRelated: {jumpRelated}");

                    Utilities.ColorEvasivePips(__instance.EvasiveDisplay, jumpRelated, sustainableEvasion);

                    // Update SidePanel too
                    Utilities.UpdateSidePanel(__instance.EvasiveDisplay, jumpRelated, sustainableEvasion);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowMoveIndicators", new Type[] { typeof(AbstractActor), typeof(float) })]
        public static class CombatHUDStatusPanel_ShowMoveIndicators_Patch
        {
            static void Postfix(CombatHUDStatusPanel __instance, AbstractActor target)
            {
                try
                {
                    if (target == null)
                    {
                        return;
                    }

                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] ___displayedActor: { target.DisplayName}");
                    int sustainableEvasion = target.GetSustainableEvasion();
                    bool jumpRelated = target.JumpedLastRound || Fields.IsJumpPreview;
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] sustainableEvasion: {sustainableEvasion}");
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] jumpRelated: {jumpRelated}");

                    Utilities.ColorEvasivePips(__instance.evasiveDisplay, jumpRelated, sustainableEvasion);


                    // Update SidePanel too
                    Utilities.UpdateSidePanel(__instance.evasiveDisplay, jumpRelated, sustainableEvasion);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }
}
