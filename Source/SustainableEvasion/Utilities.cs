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
        /*
        public static void UpdateSidePanel(CombatHUDEvasiveBarPips evasiveDisplay, bool willJumpOrHasJumped, int sustainablePips)
        {
            CombatHUDSidePanelHoverElement ___sidePanelTip = (CombatHUDSidePanelHoverElement)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "sidePanelTip").GetValue(evasiveDisplay, null);
            CombatHUD ___HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "HUD").GetValue(evasiveDisplay, null);
            float ___TargetCurrent = (float)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "TargetCurrent").GetValue(evasiveDisplay, null);
            int currentEvasivePips = (int)___TargetCurrent;
            int currentSustainablePips = willJumpOrHasJumped ? 0 : Math.Min(currentEvasivePips, sustainablePips);

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
                            currentSustainablePips > 0 ? "\n<color=#CADFACFF>" + currentSustainablePips.ToString() + " SUSTAINABLE Evasion: These can only be removed by melee or sensor lock.</color>" : ""
                    });
                }
            }
        }
        */



        public static void ColorEvasivePips(CombatHUDEvasiveBarPips evasiveDisplay, bool willJumpOrHasJumped, int sustainablePips, bool suppressCoilPips = false)
        {
            List<Graphic> ____Pips = (List<Graphic>)typeof(CombatHUDPipBar).GetProperty("Pips", AccessTools.all).GetValue(evasiveDisplay, null);
            CombatHUD ___HUD = (CombatHUD)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "HUD").GetValue(evasiveDisplay, null);
            float ___Current = (float)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "Current").GetValue(evasiveDisplay, null);
            int currentEvasivePips = (int)___Current;
            int currentSustainablePips = willJumpOrHasJumped ? 0 : Math.Min(currentEvasivePips, sustainablePips);

            Logger.Info($"[Utilities_ColorEvasivePips] ____Pips.Count: {____Pips.Count}");
            Logger.Info($"[Utilities_ColorEvasivePips] evasiveDisplay.TotalPips: {evasiveDisplay.TotalPips}");
            Logger.Info($"[Utilities_ColorEvasivePips] currentEvasivePips: {currentEvasivePips}");
            Logger.Info($"[Utilities_ColorEvasivePips] currentSustainablePips: {currentSustainablePips}");



            bool ___ActorHasCOIL = Traverse.Create(evasiveDisplay).Field("ActorHasCOIL").GetValue<bool>();
            bool ___ShouldShowCOILPips = Traverse.Create(evasiveDisplay).Field("ShouldShowCOILPips").GetValue<bool>();
            int ___floorCurrent = Traverse.Create(evasiveDisplay).Field("floorCurrent").GetValue<int>();
            //bool showCoilPips = (___ActorHasCOIL && ___ShouldShowCOILPips) && ((float)___floorCurrent >= ___HUD.Combat.Constants.CombatUIConstants.COIL_Footstep_Charge_Med);
            bool showCoilPips = (___ActorHasCOIL && ___ShouldShowCOILPips) && (___floorCurrent > 1) && !suppressCoilPips;
            Logger.Info($"[Utilities_ColorEvasivePips] ___ActorHasCOIL: {___ActorHasCOIL}");
            Logger.Info($"[Utilities_ColorEvasivePips] ___ShouldShowCOILPips: {___ShouldShowCOILPips}");
            Logger.Info($"[Utilities_ColorEvasivePips] ___floorCurrent: {___floorCurrent}");
            Logger.Info($"[Utilities_ColorEvasivePips] showCoilPips: {showCoilPips}");



            int ___AssassinPipIgnoreCount = Traverse.Create(evasiveDisplay).Field("AssassinPipIgnoreCount").GetValue<int>();
            Logger.Info($"[Utilities_ColorEvasivePips] ___AssassinPipIgnoreCount: {___AssassinPipIgnoreCount}");



            //List<HBSDOTweenButton> ___PipButtons = (List<HBSDOTweenButton>)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "PipButtons").GetValue(evasiveDisplay, null);
            //Logger.Info($"[Utilities_ColorEvasivePips] ___PipButtons.Count: {___PipButtons.Count}");

            Color coilPipColor = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.EvasivePipsCharged.color;
            //Color ignoreColor = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.EvasivePipsIgnored.color;
            Color ignoreColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.whiteHalf;
            Color sustainablePipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.green;
            Color defaultPipColor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.white;

            for (int i = 0; i < evasiveDisplay.TotalPips; i++)
            {
                // Evasive display in CombatHUDMechTray.ActorInfo does not have the tween buttons available so it seems...
                // Need to check for existence to circumvent reference error
                if (____Pips[i].gameObject.GetComponent<HBSDOTweenButton>() != null) {
                    ____Pips[i].gameObject.GetComponent<HBSDOTweenButton>().BlockDOTweenAnimations = true;
                }

                if (showCoilPips)
                {
                    UIHelpers.SetImageColor(____Pips[i], coilPipColor);
                    // Reenable tweens?
                    //if (____Pips[i].gameObject.GetComponent<HBSDOTweenButton>() != null)
                    //{
                    //    ____Pips[i].gameObject.GetComponent<HBSDOTweenButton>().BlockDOTweenAnimations = false;
                    //}
                }
                else if (i < ___AssassinPipIgnoreCount)
                {
                    UIHelpers.SetImageColor(____Pips[i], ignoreColor);
                    // Reenable tweens?
                    //if (____Pips[i].gameObject.GetComponent<HBSDOTweenButton>() != null)
                    //{
                    //    ____Pips[i].gameObject.GetComponent<HBSDOTweenButton>().BlockDOTweenAnimations = false;
                    //}
                }
                else if (i < currentSustainablePips)
                {
                    UIHelpers.SetImageColor(____Pips[i], sustainablePipColor);
                }
                else if (i < currentEvasivePips)
                {
                    UIHelpers.SetImageColor(____Pips[i], defaultPipColor);
                }
            }


            CombatHUDSidePanelHoverElement ___sidePanelTip = (CombatHUDSidePanelHoverElement)AccessTools.Property(typeof(CombatHUDEvasiveBarPips), "sidePanelTip").GetValue(evasiveDisplay, null);

            if (___sidePanelTip != null)
            {
                ___sidePanelTip.Title = new Localize.Text("EVASIVE", new object[0]);
                int num = (int)___Current - 1;
                string[] toHitMovingTargetStrings = new string[]
                {
                        "1 EVASIVE Charge: +{0} Difficulty to hit this unit with ranged attacks. {1}{2}",
                        "2 EVASIVE Charges: +{0} Difficulty to hit this unit with ranged attacks. {1}{2}",
                        "3 EVASIVE Charges: +{0} Difficulty to hit this unit with ranged attacks. {1}{2}",
                        "4 EVASIVE Charges: +{0} Difficulty to hit this unit with ranged attacks. {1}{2}",
                        "5 EVASIVE Charges: +{0} Difficulty to hit this unit with ranged attacks. {1}{2}",
                        "6 EVASIVE Charges: +{0} Difficulty to hit this unit with ranged attacks. {1}{2}",
                        "7 EVASIVE Charges: +{0} Difficulty to hit this unit with ranged attacks. {1}{2}",
                        "8 EVASIVE Charges: +{0} Difficulty to hit this unit with ranged attacks. {1}{2}"
                };
                if (currentEvasivePips > 0 && num < toHitMovingTargetStrings.Length)
                {
                    ___sidePanelTip.Description = new Localize.Text(toHitMovingTargetStrings[num], new object[]
                    {
                            ___HUD.Combat.ToHit.GetEvasivePipsModifier((int)___Current, null),
                            currentSustainablePips > 0 ? "\n<color=#" + ColorUtility.ToHtmlStringRGBA(sustainablePipColor) + ">" + currentSustainablePips.ToString() + " SUSTAINABLE Evasion: These can only be removed by melee or sensor lock.</color>" : "",
                            showCoilPips ? $"\n<color=#{ColorUtility.ToHtmlStringRGBA(coilPipColor)}>{currentEvasivePips.ToString()} COIL Charges generated.</color>" : ""
                    });
                }
            }
        }
    }
}
