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
            static void Postfix(CombatHUDActorInfo __instance, bool? isSelected, ICombatant ___displayedCombatant, AbstractActor ___displayedActor, Mech ___displayedMech)
            {
                try
                {
                    if (__instance.EvasiveDisplay == null || ___displayedActor == null)
                    {
                        return;
                    }

                    Logger.Debug($"---");
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] ___displayedActor: { ___displayedActor.DisplayName}");
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] ___displayedActor.JumpedLastRound: {___displayedActor.JumpedLastRound}");
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] ___displayedActor.HasMovedThisRound: {___displayedActor.HasMovedThisRound}");

                    int sustainableEvasion = ___displayedActor.GetSustainableEvasion();
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] sustainableEvasion: {sustainableEvasion}");

                    bool willJumpOrHasJumped = ___displayedActor.JumpedLastRound;
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] willJumpOrHasJumped: {willJumpOrHasJumped}");

                    bool isCurrentlySelected = (isSelected == null) ? (__instance.HUD.SelectedActor == ___displayedActor) : isSelected.Value;
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] isCurrentlySelected: {isCurrentlySelected}");

                    bool suppressCoilPips = isCurrentlySelected && !___displayedActor.HasMovedThisRound;
                    Logger.Debug($"[CombatHUDActorInfo_RefreshEvasiveDisplay_POSTFIX] suppressCoilPips: {suppressCoilPips}");

                    Utilities.ColorEvasivePips(__instance.EvasiveDisplay, willJumpOrHasJumped, sustainableEvasion, suppressCoilPips);
                    //Utilities.UpdateSidePanel(__instance.EvasiveDisplay, willJumpOrHasJumped, sustainableEvasion);
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

                    Logger.Debug($"---");
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] target: {target.DisplayName}");
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] target.HasJumpedThisRound: {target.HasJumpedThisRound}");
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] target.JumpedLastRound: {target.JumpedLastRound}");
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] target.HasMovedThisRound: {target.HasMovedThisRound}");

                    bool isMoveStatusPreview = __instance.GetComponentInParent(typeof(MoveStatusPreview)) != null;
                    bool isCombatHUDMechTray = __instance.GetComponentInParent(typeof(CombatHUDMechTray)) != null;
                    bool isCombatHUDTargetingComputer = __instance.GetComponentInParent(typeof(CombatHUDTargetingComputer)) != null;
                    Logger.Info($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] isMoveStatusPreview: {isMoveStatusPreview}");
                    Logger.Info($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] isCombatHUDMechTray: {isCombatHUDMechTray}");
                    Logger.Info($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] isCombatHUDTargetingComputer: {isCombatHUDTargetingComputer}");

                    //CombatHUD ___HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDStatusPanel), "HUD").GetValue(__instance, null);
                    //bool isJumpPreview = ___HUD.SelectionHandler.ActiveState.SelectionType == SelectionType.Jump;
                    //Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] isJumpPreview: {isJumpPreview}");
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] Fields.IsJumpPreview: {Fields.IsJumpPreview}");

                    int sustainableEvasion = target.GetSustainableEvasion();
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] sustainableEvasion: {sustainableEvasion}");

                    bool willJumpOrHasJumped = isMoveStatusPreview ? Fields.IsJumpPreview : (target.HasJumpedThisRound || target.JumpedLastRound);
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] willJumpOrHasJumped: {willJumpOrHasJumped}");

                    bool suppressCoilPips = isCombatHUDTargetingComputer && !target.HasMovedThisRound;
                    Logger.Debug($"[CombatHUDStatusPanel_ShowMoveIndicators_POSTFIX] suppressCoilPips: {suppressCoilPips}");

                    Utilities.ColorEvasivePips(__instance.evasiveDisplay, willJumpOrHasJumped, sustainableEvasion, suppressCoilPips);
                    //Utilities.UpdateSidePanel(__instance.evasiveDisplay, willJumpOrHasJumped, sustainableEvasion);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }


        
        // Overrides the former for previews
        /*
        [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowPreviewMoveIndicators", new Type[] { typeof(AbstractActor), typeof(MoveType) })]
        public static class CombatHUDStatusPanel_ShowPreviewMoveIndicators_Patch
        {
            static void Postfix(CombatHUDStatusPanel __instance, AbstractActor actor, MoveType moveType)
            {
                try
                {
                    if (actor == null)
                    {
                        return;
                    }

                    Logger.Debug($"[CombatHUDStatusPanel_ShowPreviewMoveIndicators_POSTFIX] ___displayedActor: { actor.DisplayName}");
                    int sustainableEvasion = actor.GetSustainableEvasion();
                    bool jumpRelated = moveType == MoveType.Jumping;
                    Logger.Debug($"[CombatHUDStatusPanel_ShowPreviewMoveIndicators_POSTFIX] sustainableEvasion: {sustainableEvasion}");
                    Logger.Debug($"[CombatHUDStatusPanel_ShowPreviewMoveIndicators_POSTFIX] jumpRelated: {jumpRelated}");

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
        */


        /*
        [HarmonyPatch(typeof(CombatHUDEvasiveBarPips), "UpdateEvasive")]
        public static class CombatHUDEvasiveBarPips_UpdateEvasive_Patch
        {
            static void Postfix(CombatHUDEvasiveBarPips __instance, float current)
            {
                try
                {
                    Logger.Debug("[CombatHUDEvasiveBarPips_UpdateEvasive_POSTFIX] Called");


                    // ToDo: Still confuses coil stuff
                    CombatHUD ___HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "HUD").GetValue(__instance, null);
                    if (___HUD.TargetingComputer != null && ___HUD.TargetingComputer.ActorInfo != null)
                    {
                        if (___HUD.TargetingComputer.ActorInfo.DisplayedCombatant is AbstractActor displayedActor)
                        {
                            int sustainableEvasion = displayedActor.GetSustainableEvasion();
                            Utilities.ColorEvasivePips(___HUD.TargetingComputer.ActorInfo.EvasiveDisplay, displayedActor.JumpedLastRound, sustainableEvasion);

                            // Update SidePanel too
                            Utilities.UpdateSidePanel(___HUD.TargetingComputer.ActorInfo.EvasiveDisplay, displayedActor.JumpedLastRound, sustainableEvasion);
                        }
                    }

                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
        */
    }
}
