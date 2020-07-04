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
            CombatHUDSidePanelHoverElement sidePanelTip = (CombatHUDSidePanelHoverElement)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "sidePanelTip").GetValue(evasiveDisplay, null);
            CombatHUD HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "HUD").GetValue(evasiveDisplay, null);
            float TargetCurrent = (float)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "TargetCurrent").GetValue(evasiveDisplay, null);
            int CurrentEvasivePips = (int)TargetCurrent;
            int MaxPipsSustainable = willJumpOrHasJumped ? 0 : Math.Min(CurrentEvasivePips, sustainablePips);

            if (sidePanelTip != null)
            {
                sidePanelTip.Title = new Localize.Text("EVASIVE", new object[0]);
                int num = (int)TargetCurrent - 1;
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
                if (CurrentEvasivePips > 0 && num < toHitMovingTargetStrings.Length)
                {
                    sidePanelTip.Description = new Localize.Text(toHitMovingTargetStrings[num], new object[]
                    {
                            HUD.Combat.ToHit.GetEvasivePipsModifier((int)TargetCurrent, null),
                            MaxPipsSustainable > 0 ? "\n<color=#CADFACFF>" + MaxPipsSustainable.ToString() + " SUSTAINABLE Evasion: These can only be removed by melee or sensor lock.</color>" : ""
                    });
                }
            }
        }

        public static void ColorEvasivePips(CombatHUDEvasiveBarPips evasiveDisplay, bool willJumpOrHasJumped, int sustainablePips)
        {
            float Current = (float)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "Current").GetValue(evasiveDisplay, null);
            int CurrentEvasivePips = (int)Current;
            int MaxPipsSustainable = willJumpOrHasJumped ? 0 : Math.Min(CurrentEvasivePips, sustainablePips);

            Logger.Info($"[Utilities_ColorEvasivePips] CurrentEvasivePips: {CurrentEvasivePips}");
            Logger.Info($"[Utilities_ColorEvasivePips] MaxPipsSustainable: {MaxPipsSustainable}");

            List<Graphic> ____Pips = (List<Graphic>)typeof(CombatHUDPipBar).GetProperty("Pips", AccessTools.all).GetValue(evasiveDisplay, null);
            Logger.Info($"[Utilities_ColorEvasivePips] ____Pips.Count: {____Pips.Count}");
            Logger.Info($"[Utilities_ColorEvasivePips] evasiveDisplay.TotalPips: {evasiveDisplay.TotalPips}");

            List<HBSDOTweenButton> ___PipButtons = (List<HBSDOTweenButton>)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "PipButtons").GetValue(evasiveDisplay, null);
            Logger.Info($"[Utilities_ColorEvasivePips] ___PipButtons.Count: {___PipButtons.Count}");
            bool evasiveDisplayHasTweenButtons = ___PipButtons.Count > 0;

            Color sustainablePipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.green;
            Color defaultPipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.white;
            Color potentialPipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.gold;

            for (int i = 0; i < evasiveDisplay.TotalPips; i++)
            {
                // Evasive display in CombatHUDMechTray.ActorInfo does not have the tween buttons available so it seems...
                // Need to check for existence
                // @ToDo: Enable this shit if actor has coil weapons or ignores evasive charges (Assassin)
                if (____Pips[i].gameObject.GetComponent<HBSDOTweenButton>() != null) {
                    ____Pips[i].gameObject.GetComponent<HBSDOTweenButton>().BlockDOTweenAnimations = true;
                }

                if (i < MaxPipsSustainable)
                {
                    UIHelpers.SetImageColor(____Pips[i], sustainablePipColor);
                }
                else if (i < CurrentEvasivePips)
                {
                    UIHelpers.SetImageColor(____Pips[i], defaultPipColor);
                }
            }
        }

        public static int GetSustainableEvasion(AbstractActor actor, bool logInfo = true)
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

            if (logInfo)
            {
                Logger.Debug("[Utilities_GetSustainableEvasion] actor.UnitType: " + actor.UnitType.ToString());
                Logger.Debug("[Utilities_GetSustainableEvasion] actor.weightClass: " + weightClass.ToString());
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
                        if (logInfo)
                        {
                            Logger.Debug("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " has Evasive Movement");
                        }
                    }
                    if (enumerator.Current.Def.Description.Id == "AbilityDefP8")
                    {
                        pilotIsAcePilot = true;
                        if (logInfo)
                        {
                            Logger.Debug("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " is Ace Pilot");
                        }
                    }
                    if (enumerator.Current.Def.Description.Id == "AbilityDefT8A")
                    {
                        pilotIsMasterTactician = true;
                        if (logInfo)
                        {
                            Logger.Debug("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " is Master Tactician");
                        }
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

                // Check boni
                if (pilotIsAcePilot)
                {
                    sustainableEvasion += SustainableEvasion.Settings.AcePilotSustainableBonus;
                }
                else if (pilotIsMasterTactician)
                {
                    sustainableEvasion += SustainableEvasion.Settings.MasterTacticianSustainableBonus;
                }
            }
            return sustainableEvasion;
        }
    }
}
