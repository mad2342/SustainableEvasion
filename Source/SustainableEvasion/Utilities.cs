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
        public static void UpdateSidePanel(CombatHUDEvasiveBarPips evasiveDisplay, bool willJumpOrHasJumped, int sustainablePips, bool logInfo = true)
        {
            CombatHUDSidePanelHoverElement sidePanelTip = (CombatHUDSidePanelHoverElement)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "sidePanelTip").GetValue(evasiveDisplay, null);
            CombatHUD HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "HUD").GetValue(evasiveDisplay, null);
            float TargetCurrent = (float)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "TargetCurrent").GetValue(evasiveDisplay, null);
            int CurrentEvasivePips = (int)TargetCurrent;
            int MaxPipsSustainable = willJumpOrHasJumped ? 0 : Math.Min(CurrentEvasivePips, sustainablePips);


            if (logInfo)
            {
                Logger.LogLine("[Utilities_UpdateSidePanel] sustainablePips: " + sustainablePips);
                Logger.LogLine("[Utilities_UpdateSidePanel] willJumpOrHasJumped: " + willJumpOrHasJumped);

                Logger.LogLine("[Utilities_UpdateSidePanel] CurrentEvasivePips: " + CurrentEvasivePips);
                Logger.LogLine("[Utilities_UpdateSidePanel] MaxPipsSustainable: " + MaxPipsSustainable);
            }

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
                        "6 EVASIVE charges: +{0} Difficulty to hit this unit with ranged attacks. {1}"
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

        public static void ColorEvasivePips(CombatHUDEvasiveBarPips evasiveDisplay, bool willJumpOrHasJumped, int sustainablePips, bool logInfo = false)
        {
            float Current = (float)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "Current").GetValue(evasiveDisplay, null);
            int CurrentEvasivePips = (int)Current;
            int MaxPipsSustainable = willJumpOrHasJumped ? 0 : Math.Min(CurrentEvasivePips, sustainablePips);

            //CombatHUDPipBar combatHUDPipBar = __instance as CombatHUDPipBar;
            //List<Graphic> Pips = (List<Graphic>)AccessTools.Property(typeof(CombatHUDPipBar), "Pips").GetValue(combatHUDPipBar, null);
            List<Graphic> Pips = (List<Graphic>)typeof(CombatHUDPipBar).GetProperty("Pips", AccessTools.all).GetValue(evasiveDisplay, null);
            //List<Graphic> Pips = (List<Graphic>)AccessTools.Property(typeof(CombatHUDPipBar), "Pips").GetValue(__instance, null);

            Color sustainablePipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.green;
            Color defaultPipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.white;
            Color potentialPipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.whiteHalf;

            if (logInfo)
            {
                Logger.LogLine("[Utilities_ColorEvasivePips] sustainablePips: " + sustainablePips);
                Logger.LogLine("[Utilities_ColorEvasivePips] willJumpOrHasJumped: " + willJumpOrHasJumped);

                Logger.LogLine("[Utilities_ColorEvasivePips] CurrentEvasivePips: " + CurrentEvasivePips);
                Logger.LogLine("[Utilities_ColorEvasivePips] MaxPipsSustainable: " + MaxPipsSustainable);

                // Note that not all of this Pips are active game objects right now
                Logger.LogLine("[Utilities_ColorEvasivePips] PipCount: " + evasiveDisplay.PipCount);
                Logger.LogLine("[Utilities_ColorEvasivePips] TotalPips: " + evasiveDisplay.TotalPips);
            }

            for (int i = 0; i < evasiveDisplay.TotalPips; i++)
            {
                if (i < MaxPipsSustainable)
                {
                    UIHelpers.SetImageColor(Pips[i], sustainablePipColor);
                }
                else if (i < CurrentEvasivePips)
                {
                    UIHelpers.SetImageColor(Pips[i], defaultPipColor);
                }
                //else
                //{
                //    UIHelpers.SetImageColor(Pips[i], potentialPipColor);
                //}
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
                Logger.LogLine("[Utilities_GetSustainableEvasion] actor.UnitType: " + actor.UnitType.ToString());
                Logger.LogLine("[Utilities_GetSustainableEvasion] actor.weightClass: " + weightClass.ToString());
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
                            Logger.LogLine("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " has Evasive Movement");
                        }
                    }
                    if (enumerator.Current.Def.Description.Id == "AbilityDefP8")
                    {
                        pilotIsAcePilot = true;
                        if (logInfo)
                        {
                            Logger.LogLine("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " is Ace Pilot");
                        }
                    }
                    if (enumerator.Current.Def.Description.Id == "AbilityDefT8A")
                    {
                        pilotIsMasterTactician = true;
                        if (logInfo)
                        {
                            Logger.LogLine("[Utilities_GetSustainableEvasion] Pilot " + p.Name + " is Master Tactician");
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
