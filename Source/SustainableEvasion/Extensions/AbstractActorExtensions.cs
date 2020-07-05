using System.Collections.Generic;
using BattleTech;

namespace SustainableEvasion.Extensions
{
    public static class AbstractActorExtensions
    {
        public static int GetSustainableEvasion(this AbstractActor actor)
        {
            if (actor.UnitType == UnitType.Turret)
            {
                return 0;
            }

            WeightClass weightClass = actor.GetWeightClass();
            Pilot p = actor.GetPilot();

            int sustainableEvasion = 0;
            bool pilotHasEvasiveMovement = false;
            bool pilotIsMasterTactician = false;

            using (List<Ability>.Enumerator enumerator = p.Abilities.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Def.Description.Id == "AbilityDefP5")
                    {
                        pilotHasEvasiveMovement = true;
                        Logger.Info($"[Utilities_GetSustainableEvasion] Pilot {p.Name} has Evasive Movement");
                    }
                    if (enumerator.Current.Def.Description.Id == "AbilityDefT8A")
                    {
                        pilotIsMasterTactician = true;
                        Logger.Info($"[Utilities_GetSustainableEvasion] Pilot {p.Name} is Master Tactician");
                    }
                }
            }

            if (pilotHasEvasiveMovement)
            {
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

                // Bonus for Scouts
                if (pilotIsMasterTactician)
                {
                    sustainableEvasion += SustainableEvasion.Settings.MasterTacticianSustainableBonus;
                }
            }
            return sustainableEvasion;
        }



        public static WeightClass GetWeightClass(this AbstractActor actor)
        {
            WeightClass weightClass = WeightClass.LIGHT;

            if (actor.UnitType == UnitType.Turret)
            {
                weightClass = (actor as Turret).TurretDef.Chassis.weightClass;
            }
            else if (actor.UnitType == UnitType.Vehicle)
            {
                weightClass = (actor as Vehicle).VehicleDef.Chassis.weightClass;
            }
            else if (actor.UnitType == UnitType.Mech)
            {
                weightClass = (actor as Mech).MechDef.Chassis.weightClass;
            }
            else
            {
                // Throw error
                Logger.Debug($"[AbstractActorExtensions_GetWeightClass] ({actor.DisplayName}) is an unknown specialization of AbstractActor");
            }

            return weightClass;
        }
    }
}
