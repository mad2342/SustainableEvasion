using BattleTech;
using Harmony;
using SustainableEvasion.Extensions;
using System;
using System.Collections.Generic;

namespace SustainableEvasion.Patches
{
    class GameLogic
    {
        [HarmonyPatch(typeof(AbstractActor), "ResolveAttackSequence")]
        public static class AbstractActor_ResolveAttackSequence
        {

            static bool Prefix(AbstractActor __instance)
            {

                // Jumping grants no sustainable evasion no matter what
                if (__instance.JumpedLastRound)
                {
                    Fields.LoosePip = true;
                }
                // Determine capabilities of current actor
                int sustainableEvasion = __instance.GetSustainableEvasion();
                if (sustainableEvasion < __instance.EvasivePipsCurrent)
                {
                    Fields.LoosePip = true;
                }

                return false;
            }

            static void Postfix(AbstractActor __instance, string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection)
            {
                try
                {

                    AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(sequenceID);
                    if (attackSequence != null && attackSequence.GetAttackDidDamage(__instance.GUID))
                    {
                        List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance).FindAll((Effect x) => x.EffectData.targetingData.effectTriggerType == EffectTriggerType.OnDamaged);
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].OnEffectTakeDamage(attackSequence.attacker, __instance);
                        }
                        if (attackSequence.isMelee)
                        {
                            int value = attackSequence.attacker.StatCollection.GetValue<int>("MeleeHitPushBackPhases");
                            if (value > 0)
                            {
                                for (int j = 0; j < value; j++)
                                {
                                    __instance.ForceUnitOnePhaseDown(sourceID, stackItemID, false);
                                }
                            }
                        }
                    }

                    // Make sustainable evasion removable if many shots are directed at target?
                    Logger.Info("[AbstractActor_ResolveAttackSequence_POSTFIX] attackSequence.allSelectedWeapons.Count: " + attackSequence.allSelectedWeapons.Count.ToString());
                    Logger.Info("[AbstractActor_ResolveAttackSequence_POSTFIX] attackSequence.attackTotalShotsFired: " + attackSequence.attackTotalShotsFired);

                    int evasivePipsCurrent = __instance.EvasivePipsCurrent;
                    //BEN: Patch
                    if (Fields.LoosePip || attackSequence.isMelee)
                    {
                        __instance.ConsumeEvasivePip(true);
                        Fields.LoosePip = false;
                    }
                    //:NEB
                    int evasivePipsCurrent2 = __instance.EvasivePipsCurrent;
                    if (evasivePipsCurrent2 < evasivePipsCurrent && !__instance.IsDead && !__instance.IsFlaggedForDeath)
                    {
                        __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "-1 EVASION", FloatieMessage.MessageNature.Debuff));
                    }
                    //BEN: Patch
                    else if (evasivePipsCurrent2 > 0 && !__instance.IsDead && !__instance.IsFlaggedForDeath)
                    {
                        __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "SUSTAINED EVASION", FloatieMessage.MessageNature.Neutral));
                    }
                    //:NEB
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }
}
