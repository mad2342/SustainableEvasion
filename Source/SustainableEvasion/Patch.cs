using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;

namespace SustainableEvasion
{
    [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowPreviewMoveIndicators")]
    public static class CombatHUDStatusPanel_ShowPreviewMoveIndicators_Patch
    {
        static void Postfix(CombatHUDStatusPanel __instance, AbstractActor actor, MoveType moveType)
        {
            try
            {
                // NOTE: This is not called for AI
                AbstractActor a = actor;
                if (a != null)
                {
                    //Mech m = a as Mech;
                    Pilot p = a.GetPilot();
                    if (p != null)
                    {
                        // Need to set all fields as one can select other actors during preview leading to wrong results
                        Logger.LogLine("[CombatHUDStatusPanel_ShowPreviewMoveIndicators_POSTFIX] SETTING fields at PREVIEW MOVE for pilot: " + p.Name);
                        Fields.SustainablePips = Utilities.GetSustainableEvasion(a);
                        Fields.WillJumpOrHasJumped = moveType == MoveType.Jumping;
                        Fields.LastActorInfo = a.LogDisplayName;
                        Logger.LogLine("[CombatHUDStatusPanel_ShowPreviewMoveIndicators_POSTFIX] SET Fields.SustainablePips: " + Fields.SustainablePips);
                        Logger.LogLine("[CombatHUDStatusPanel_ShowPreviewMoveIndicators_POSTFIX] SET Fields.WillJumpOrHasJumped: " + Fields.WillJumpOrHasJumped);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
    
    [HarmonyPatch(typeof(CombatHUDActorInfo), "OnEvasiveChanged")]
    public static class CombatHUDActorInfo_OnEvasiveChanged_Patch
    {
        static void Postfix(CombatHUDActorInfo __instance, MessageCenterMessage message, ICombatant ___displayedCombatant, AbstractActor ___displayedActor, Mech ___displayedMech)
        {
            try
            {
                // Limit to relevant actors, otherwise will loop through ALL actors
                EvasiveChangedMessage evasiveChangedMessage = message as EvasiveChangedMessage;
                if (___displayedActor != null && evasiveChangedMessage.affectedObjectGuid == ___displayedActor.GUID && __instance.EvasiveDisplay != null)
                {
                    //Logger.LogLine("[CombatHUDActorInfo_OnEvasiveChanged_POSTFIX] ___displayedCombatant.LogDisplayName: " + ___displayedCombatant.LogDisplayName);
                    //Logger.LogLine("[CombatHUDActorInfo_OnEvasiveChanged_POSTFIX] ___displayedActor.GetPilot().Name: " + ___displayedActor.GetPilot().Name);

                    AbstractActor a = ___displayedActor;
                    if (a != null)
                    {
                        //Mech m = a as Mech;
                        Pilot p = a.GetPilot();
                        if (p != null)
                        {
                            Logger.LogLine("[CombatHUDActorInfo_OnEvasiveChanged_POSTFIX] SETTING fields at EVASIVE CHANGED for pilot: " + p.Name);
                            Fields.SustainablePips = Utilities.GetSustainableEvasion(a);
                            Fields.WillJumpOrHasJumped = a.JumpedLastRound;
                            Fields.LastActorInfo = a.LogDisplayName;
                            Logger.LogLine("[CombatHUDActorInfo_OnEvasiveChanged_POSTFIX] SET Fields.SustainablePips: " + Fields.SustainablePips);
                            Logger.LogLine("[CombatHUDActorInfo_OnEvasiveChanged_POSTFIX] SET Fields.WillJumpOrHasJumped: " + Fields.WillJumpOrHasJumped);
                        }
                    }
                    // This call is needed because "OnEvasiveChange" sometimes immediately is followed by a call to "RefrehAllInfo" (without triggering "ShowCurrent" inbetween)
                    // This leads to the actor having displayed wrong colors for its pips because "RefreshAllInfo" is called on its TARGET (which may report a different movement type
                    // than THIS actor) -> Current actor displays pips as if it had used the wrong movement type (the one of its target)

                    //CombatHUDEvasiveBarPips EvasiveDisplay = (CombatHUDEvasiveBarPips)AccessTools.Property(typeof(CombatHUDActorInfo), "EvasiveDisplay").GetValue(__instance, null);
                    //SustainableEvasion.ColorEvasivePips(EvasiveDisplay, Fields.WillJumpOrHasJumped, Fields.SustainablePips);

                    Utilities.ColorEvasivePips(__instance.EvasiveDisplay, Fields.WillJumpOrHasJumped, Fields.SustainablePips);

                    // Directly invoke patched method would be better but i just can't wrap my head around it
                    //MethodInfo mi = AccessTools.Method(typeof(CombatHUDEvasiveBarPips), "ShowCurrent");
                    //Invoke somehow...
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDActorInfo), "RefreshAllInfo")]
    public static class CombatHUDActorInfo_RefreshAllInfo_Patch
    {
        static void Prefix(CombatHUDActorInfo __instance, ICombatant ___displayedCombatant, AbstractActor ___displayedActor, Mech ___displayedMech)
        {
            try
            {
                // BEWARE: This sometimes thows an exception after an actor is deselected thus it's sometimes called without a "target"? Need to investigate...
                //Logger.LogLine("[CombatHUDActorInfo_RefreshAllInfo_PREFIX] ___displayedCombatant.LogDisplayName: " + ___displayedCombatant.LogDisplayName);
                //Logger.LogLine("[CombatHUDActorInfo_RefreshAllInfo_PREFIX] ___displayedActor.GetPilot().Name: " + ___displayedActor.GetPilot().Name);

                AbstractActor a = ___displayedActor;
                if (a != null)
                {
                    //Mech m = a as Mech;
                    Pilot p = a.GetPilot();
                    if (p != null)
                    {
                        Logger.LogLine("[CombatHUDActorInfo_RefreshAllInfo_PREFIX] SETTING fields at REFRESH INFO for pilot: " + p.Name);
                        Fields.SustainablePips = Utilities.GetSustainableEvasion(a);
                        Fields.WillJumpOrHasJumped = a.JumpedLastRound;
                        Fields.LastActorInfo = a.LogDisplayName;
                        Logger.LogLine("[CombatHUDActorInfo_RefreshAllInfo_PREFIX] SET Fields.SustainablePips: " + Fields.SustainablePips);
                        Logger.LogLine("[CombatHUDActorInfo_RefreshAllInfo_PREFIX] SET Fields.WillJumpOrHasJumped: " + Fields.WillJumpOrHasJumped);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDEvasiveBarPips), "UpdateEvasive")]
    public static class CombatHUDEvasiveBarPips_UpdateEvasive_Patch
    {
        static void Prefix(CombatHUDEvasiveBarPips __instance, float current)
        {
            try
            {
                Logger.LogLine("[CombatHUDEvasiveBarPips_UpdateEvasive_PREFIX] Called");
                // This call is to prevent wrong coloring of pips during movement preview
                // BUT will corrupt pips of selected target in TargetingComputer (if any) DURING movement preview :-( (-> fixed, see below)
                Utilities.ColorEvasivePips(__instance, Fields.WillJumpOrHasJumped, Fields.SustainablePips);

                
                
                // Call the evasiveDisplay of Targeting Computer with the correct values (the ones of the actor displayed in it, even during movement preview...)
                // BEWARE: During mission load this gets called several times with "forced"/simulated visibility!
                CombatHUD HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "HUD").GetValue(__instance, null);

                //Logger.LogLine("[CombatHUDEvasiveBarPips_UpdateEvasive_PREFIX] HUD.Combat.IsLoadingFromSave?: " + HUD.Combat.IsLoadingFromSave.ToString());
                //Logger.LogLine("[CombatHUDEvasiveBarPips_UpdateEvasive_PREFIX] HUD != null?: " + (HUD != null).ToString());
                //Logger.LogLine("[CombatHUDEvasiveBarPips_UpdateEvasive_PREFIX] HUD.TargetingComputerShowing?: " + HUD.TargetingComputerShowing.ToString());

                //CombatHUDStatusPanel StatusPanel = (CombatHUDStatusPanel)AccessTools.Field(typeof(CombatHUDTargetingComputer), "StatusPanel").GetValue(HUD.TargetingComputer);
                //CombatHUDEvasiveBarPips evasiveDisplay = (CombatHUDEvasiveBarPips)AccessTools.Field(typeof(CombatHUDStatusPanel), "evasiveDisplay").GetValue(StatusPanel);

                //Mech displayedMech = (Mech)AccessTools.Field(typeof(CombatHUDActorInfo), "displayedMech").GetValue(HUD.TargetingComputer.ActorInfo);
                //AbstractActor displayedActor = (AbstractActor)AccessTools.Field(typeof(CombatHUDActorInfo), "displayedActor").GetValue(HUD.TargetingComputer.ActorInfo);

                if (HUD.TargetingComputer != null && HUD.TargetingComputer.ActorInfo != null)
                {
                    if (HUD.TargetingComputer.ActorInfo.DisplayedCombatant is AbstractActor displayedActor)
                    {
                        int sustainableEvasion = Utilities.GetSustainableEvasion(displayedActor);
                        Utilities.ColorEvasivePips(HUD.TargetingComputer.ActorInfo.EvasiveDisplay, displayedActor.JumpedLastRound, sustainableEvasion);
                    }
                }



                /* For brain
                Logger.LogLine("SelectedActor: " + HUD.SelectedActor.LogDisplayName);
                Logger.LogLine("SelectedActor: " + HUD.SelectedActor.GUID);
                Logger.LogLine("TargetingComputerShowing: " + HUD.TargetingComputerShowing.ToString());
                if (HUD.TargetingComputer.ActorInfo.DisplayedCombatant != null)
                {
                    Logger.LogLine("TargetingComputer.ActorInfo.DisplayedCombatant.LogDisplayName: " + HUD.TargetingComputer.ActorInfo.DisplayedCombatant.LogDisplayName);
                }
                ICombatant hoveredCombatant = (ICombatant)AccessTools.Field(typeof(CombatHUDTargetingComputer), "hoveredCombatant").GetValue(HUD.TargetingComputer);
                if (hoveredCombatant != null)
                {
                    Logger.LogLine("TargetingComputer.hoveredCombatant: " + hoveredCombatant.LogDisplayName);
                    Logger.LogLine("TargetingComputer.hoveredCombatant: " + hoveredCombatant.GUID);
                }
                Logger.LogLine("HUD.TargetingComputer.StatusPanel.evasiveDisplay.TotalPips: " + evasiveDisplay.TotalPips);
                Logger.LogLine("__instance.TotalPips: " + __instance.TotalPips);
                */





                // Directly invoke patched method would be better but i just can't wrap my head around it
                //MethodInfo mi = AccessTools.Method(typeof(CombatHUDEvasiveBarPips), "ShowCurrent");
                //Invoke somehow...
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            } 
        }
        static void Postfix(CombatHUDEvasiveBarPips __instance, float current)
        {
            try
            {
                Logger.LogLine("[CombatHUDEvasiveBarPips_UpdateEvasive_POSTFIX] Called");
                // Update side panel for last touched actor
                Utilities.UpdateSidePanel(__instance, Fields.WillJumpOrHasJumped, Fields.SustainablePips);



                // Update for currently selected actor in Targeting Computer too
                CombatHUD HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "HUD").GetValue(__instance, null);

                //Logger.LogLine("[CombatHUDEvasiveBarPips_UpdateEvasive_POSTFIX] HUD.Combat.IsLoadingFromSave?: " + HUD.Combat.IsLoadingFromSave.ToString());
                //Logger.LogLine("[CombatHUDEvasiveBarPips_UpdateEvasive_POSTFIX] HUD != null?: " + (HUD != null).ToString());
                //Logger.LogLine("[CombatHUDEvasiveBarPips_UpdateEvasive_POSTFIX] HUD.TargetingComputerShowing?: " + HUD.TargetingComputerShowing.ToString());

                //CombatHUDStatusPanel StatusPanel = (CombatHUDStatusPanel)AccessTools.Field(typeof(CombatHUDTargetingComputer), "StatusPanel").GetValue(HUD.TargetingComputer);
                //CombatHUDEvasiveBarPips evasiveDisplay = (CombatHUDEvasiveBarPips)AccessTools.Field(typeof(CombatHUDStatusPanel), "evasiveDisplay").GetValue(StatusPanel);

                //Mech displayedMech = (Mech)AccessTools.Field(typeof(CombatHUDActorInfo), "displayedMech").GetValue(HUD.TargetingComputer.ActorInfo);
                //AbstractActor displayedActor = (AbstractActor)AccessTools.Field(typeof(CombatHUDActorInfo), "displayedActor").GetValue(HUD.TargetingComputer.ActorInfo);

                if (HUD.TargetingComputer != null && HUD.TargetingComputer.ActorInfo != null)
                {
                    if (HUD.TargetingComputer.ActorInfo.DisplayedCombatant is AbstractActor displayedActor)
                    {
                        int sustainableEvasion = Utilities.GetSustainableEvasion(displayedActor);
                        Utilities.UpdateSidePanel(HUD.TargetingComputer.ActorInfo.EvasiveDisplay, displayedActor.JumpedLastRound, sustainableEvasion);
                    }
                }

            }
            catch(Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDEvasiveBarPips), "ShowCurrent")]
    public static class CombatHUDEvasiveBarPips_ShowCurrent_Patch
    {
        static void Postfix(CombatHUDEvasiveBarPips __instance)
        {
            // This seems to be the only "one fits all" place to color the pips aside from completely overriding stuff
            // It NEEDS to be called no matter what as "ShowValue" from base class (CombatHUDPipBar) ALWAYS colors pips!
            // As CombatHUDEvasiveBarPips has no info of relevant actor itself this needs to be maintained through custom fields
            Utilities.ColorEvasivePips(__instance, Fields.WillJumpOrHasJumped, Fields.SustainablePips);
        }
    }



    [HarmonyPatch(typeof(AbstractActor), "ResolveAttackSequence")]
    public static class AbstractActor_ResolveAttackSequence {

        static bool Prefix(AbstractActor __instance) {

            // Jumping grants no sustainable evasion no matter what
            if (__instance.JumpedLastRound)
            {
                Fields.LoosePip = true;
            }
            // Determine capabilities of current actor
            int sustainableEvasion = Utilities.GetSustainableEvasion(__instance);
            if (sustainableEvasion < __instance.EvasivePipsCurrent)
            {
                Fields.LoosePip = true;
            }

            return false;
        }

        static void Postfix(AbstractActor __instance, string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection) {
            try {

                AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(sequenceID);
                if (attackSequence != null && attackSequence.GetAttackDidDamage(__instance.GUID)) {
                    List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance).FindAll((Effect x) => x.EffectData.targetingData.effectTriggerType == EffectTriggerType.OnDamaged);
                    for (int i = 0; i < list.Count; i++) {
                        list[i].OnEffectTakeDamage(attackSequence.attacker, __instance);
                    }
                    if (attackSequence.isMelee) {
                        int value = attackSequence.attacker.StatCollection.GetValue<int>("MeleeHitPushBackPhases");
                        if (value > 0) {
                            for (int j = 0; j < value; j++) {
                                __instance.ForceUnitOnePhaseDown(sourceID, stackItemID, false);
                            }
                        }
                    }
                }

                // Make sustainable evasion removable if many shots are directed at target?
                Logger.LogLine("[AbstractActor_ResolveAttackSequence_POSTFIX] attackSequence.allSelectedWeapons.Count: " + attackSequence.allSelectedWeapons.Count.ToString());
                Logger.LogLine("[AbstractActor_ResolveAttackSequence_POSTFIX] attackSequence.attackTotalShotsFired: " + attackSequence.attackTotalShotsFired);

                int evasivePipsCurrent = __instance.EvasivePipsCurrent;
                //BEN: Patch
                if (Fields.LoosePip || attackSequence.isMelee) {
                    __instance.ConsumeEvasivePip(true);
                    Fields.LoosePip = false;
                }
                //:NEB
                int evasivePipsCurrent2 = __instance.EvasivePipsCurrent;
                if (evasivePipsCurrent2 < evasivePipsCurrent && !__instance.IsDead && !__instance.IsFlaggedForDeath) {
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "-1 EVASION", FloatieMessage.MessageNature.Debuff));
                }
                //BEN: Patch
                else if (evasivePipsCurrent2 > 0 && !__instance.IsDead && !__instance.IsFlaggedForDeath)
                {
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "SUSTAINED EVASION", FloatieMessage.MessageNature.Neutral));
                }
                //:NEB
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }
}