using Harmony;
using BattleTech;
using BattleTech.UI;
using System;
using System.Collections.Generic;
using HBS;
using UnityEngine;
using UnityEngine.UI;

namespace SustainableEvasion
{
    class Utilities
    {
        public static void UpdateSidePanel(CombatHUDEvasiveBarPips evasiveDisplay, bool willJumpOrHasJumped, int sustainablePips)
        {
            CombatHUDSidePanelHoverElement ___sidePanelTip = (CombatHUDSidePanelHoverElement)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "sidePanelTip").GetValue(evasiveDisplay, null);
            CombatHUD ___HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "HUD").GetValue(evasiveDisplay, null);
            float ___TargetCurrent = (float)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "TargetCurrent").GetValue(evasiveDisplay, null);
            int currentEvasivePips = (int)___TargetCurrent;
            int maxSustainablePips = willJumpOrHasJumped ? 0 : Math.Min(currentEvasivePips, sustainablePips);

            if (___sidePanelTip != null)
            {
                ___sidePanelTip.Title = new Localize.Text("EVASIVE", new object[0]);
                int num = (int)___TargetCurrent - 1;
                string[] toHitMovingTargetStrings = new string[]
                {
                        "1 EVASIVE charge: +{0} Difficulty to hit this unit with ranged attacks. {1}",
                        "2 EVASIVE charges: +{0} Difficulty to hit this unit with ranged attacks. {1}",
                        "3 EVASIVE charges: +{0} Difficulty to hit this unit with ranged attacks. {1}",
                        "4 EVASIVE charges: +{0} Difficulty to hit this unit with ranged attacks. {1}",
                        "5 EVASIVE charges: +{0} Difficulty to hit this unit with ranged attacks. {1}",
                        "6 EVASIVE charges: +{0} Difficulty to hit this unit with ranged attacks. {1}",
                        "7 EVASIVE charges: +{0} Difficulty to hit this unit with ranged attacks. {1}",
                        "8 EVASIVE charges: +{0} Difficulty to hit this unit with ranged attacks. {1}"
                };
                if (currentEvasivePips > 0 && num < toHitMovingTargetStrings.Length)
                {
                    ___sidePanelTip.Description = new Localize.Text(toHitMovingTargetStrings[num], new object[]
                    {
                            ___HUD.Combat.ToHit.GetEvasivePipsModifier((int)___TargetCurrent, null),
                            maxSustainablePips > 0 ? "\n<color=#CADFACFF>" + maxSustainablePips.ToString() + " SUSTAINABLE Evasion: These can only be removed by melee or sensor lock.</color>" : ""
                    });
                }
            }
        }



        public static void ColorEvasivePips(CombatHUDEvasiveBarPips evasiveDisplay, bool willJumpOrHasJumped, int sustainablePips)
        {
            List<Graphic> ____Pips = (List<Graphic>)typeof(CombatHUDPipBar).GetProperty("Pips", AccessTools.all).GetValue(evasiveDisplay, null);
            float ___Current = (float)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "Current").GetValue(evasiveDisplay, null);
            int currentEvasivePips = (int)___Current;
            int maxPipsSustainable = willJumpOrHasJumped ? 0 : Math.Min(currentEvasivePips, sustainablePips);

            Logger.Info($"[Utilities_ColorEvasivePips] ____Pips.Count: {____Pips.Count}");
            Logger.Info($"[Utilities_ColorEvasivePips] evasiveDisplay.TotalPips: {evasiveDisplay.TotalPips}");
            Logger.Info($"[Utilities_ColorEvasivePips] currentEvasivePips: {currentEvasivePips}");
            Logger.Info($"[Utilities_ColorEvasivePips] maxPipsSustainable: {maxPipsSustainable}");



            bool ___ActorHasCOIL = Traverse.Create(evasiveDisplay).Field("ActorHasCOIL").GetValue<bool>();
            bool ___ShouldShowCOILPips = Traverse.Create(evasiveDisplay).Field("ShouldShowCOILPips").GetValue<bool>();
            int ___AssassinPipIgnoreCount = Traverse.Create(evasiveDisplay).Field("AssassinPipIgnoreCount").GetValue<int>();
            Logger.Info($"[Utilities_ColorEvasivePips] ___ActorHasCOIL: {___ActorHasCOIL}");
            Logger.Info($"[Utilities_ColorEvasivePips] ___ShouldShowCOILPips: {___ShouldShowCOILPips}");
            Logger.Info($"[Utilities_ColorEvasivePips] ___AssassinPipIgnoreCount: {___AssassinPipIgnoreCount}");



            //List<HBSDOTweenButton> ___PipButtons = (List<HBSDOTweenButton>)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "PipButtons").GetValue(evasiveDisplay, null);
            //Logger.Info($"[Utilities_ColorEvasivePips] ___PipButtons.Count: {___PipButtons.Count}");

            Color coilPipColor = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.EvasivePipsSuperCharged.color;
            Color ignoreColor = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.EvasivePipsIgnored.color;
            Color sustainablePipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.green;
            Color defaultPipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.white;

            for (int i = 0; i < evasiveDisplay.TotalPips; i++)
            {
                // Evasive display in CombatHUDMechTray.ActorInfo does not have the tween buttons available so it seems...
                // Need to check for existence
                // @ToDo: Enable this shit if actor has coil weapons or ignores evasive charges (Assassin)?
                if (____Pips[i].gameObject.GetComponent<HBSDOTweenButton>() != null) {
                    ____Pips[i].gameObject.GetComponent<HBSDOTweenButton>().BlockDOTweenAnimations = true;
                }

                if (___ActorHasCOIL && ___ShouldShowCOILPips)
                {
                    UIHelpers.SetImageColor(____Pips[i], coilPipColor);
                }
                else if (i < ___AssassinPipIgnoreCount)
                {
                    UIHelpers.SetImageColor(____Pips[i], ignoreColor);
                }
                else if (i < maxPipsSustainable)
                {
                    UIHelpers.SetImageColor(____Pips[i], sustainablePipColor);
                }
                else if (i < currentEvasivePips)
                {
                    UIHelpers.SetImageColor(____Pips[i], defaultPipColor);
                }
            }
        }



        // Legacy
        public static int GetSustainableEvasion(AbstractActor actor)
        {
            WeightClass weightClass = new WeightClass();
            if (actor.UnitType == UnitType.Mech)
            {
                weightClass = (actor as Mech).weightClass;
            }
            else if (actor.UnitType == UnitType.Vehicle)
            {
                weightClass = (actor as Vehicle).weightClass;
            }
            else
            {
                return 0;
            }

            // TEST
            //if (actor.UnitType == UnitType.Vehicle)
            //{
            //    return 4;
            //}

            // Calculate
            int sustainableEvasion = 0;
            Pilot p = actor.GetPilot();
            bool pilotHasEvasiveMovement = false;
            bool pilotIsAcePilot = false;
            bool pilotIsMasterTactician = false;

            using (List<Ability>.Enumerator enumerator = p.Abilities.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Def.Description.Id == "AbilityDefP5")
                    {
                        pilotHasEvasiveMovement = true;
                        Logger.Info("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " has Evasive Movement");
                    }
                    if (enumerator.Current.Def.Description.Id == "AbilityDefP8")
                    {
                        pilotIsAcePilot = true;
                        Logger.Info("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " is Ace Pilot");
                    }
                    if (enumerator.Current.Def.Description.Id == "AbilityDefT8A")
                    {
                        pilotIsMasterTactician = true;
                        Logger.Info("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " is Master Tactician");
                    }
                }
            }

            // Adding up
            if (pilotHasEvasiveMovement)
            {
                // Switch weight classes
                if (weightClass == WeightClass.LIGHT)
                {
                    sustainableEvasion = SustainableEvasion.Settings.SustainableEvasionLight;
                }
                else if (weightClass == WeightClass.MEDIUM)
                {
                    sustainableEvasion = SustainableEvasion.Settings.SustainableEvasionMedium;
                }
                else if (weightClass == WeightClass.HEAVY)
                {
                    sustainableEvasion = SustainableEvasion.Settings.SustainableEvasionHeavy;
                }
                else if (weightClass == WeightClass.ASSAULT)
                {
                    sustainableEvasion = SustainableEvasion.Settings.SustainableEvasionAssault;
                }

                // Check bonus
                else if (pilotIsMasterTactician)
                {
                    sustainableEvasion += SustainableEvasion.Settings.MasterTacticianSustainableBonus;
                }
            }
            return sustainableEvasion;
        }
    }
}
