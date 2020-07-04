using System.Collections.Generic;
using BattleTech;

namespace SustainableEvasion.Extensions
{
    public static class AbstractActorExtensions
    {
        public static int GetSustainableEvasion(this AbstractActor actor)
        {
            Pilot pilot = actor.GetPilot();
            bool pilotHasEvasiveMovement = false;
            int sustainableEvasion = 0;

            Logger.Info("[Utilities_GetSustainableEvasion] actor.Initiative: " + actor.InitiativeAsString);

            using (List<Ability>.Enumerator enumerator = pilot.Abilities.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Def.Description.Id == "AbilityDefP5")
                    {
                        pilotHasEvasiveMovement = true;
                        Logger.Info($"[Utilities_GetSustainableEvasion] {pilot.Name} has evasive movement.");

                        break;
                    }
                }
            }



            if (pilotHasEvasiveMovement)
            {
                // Using the CURRENT initiative here. Interesting effect as actors who defer their phase are getting less and less sustainable evasion...
                /*
                if (Int32.TryParse(actor.InitiativeAsString, out int initiative))
                {
                    sustainableEvasion = initiative;
                }
                */

                // Using BASE initiative
                int baseInitiative = actor.BaseInitiative; // Includes phase modifiers from master tactician or battle computer
                switch (baseInitiative)
                {
                    case 1:
                        sustainableEvasion = 5;
                        break;
                    case 2:
                        sustainableEvasion = 4;
                        break;
                    case 3:
                        sustainableEvasion = 3;
                        break;
                    case 4:
                        sustainableEvasion = 2;
                        break;
                    case 5:
                        sustainableEvasion = 1;
                        break;
                    default:
                        break;
                }
            }
            Logger.Info($"[Utilities_GetSustainableEvasion] {pilot.Name} can sustain {sustainableEvasion} evasion.");

            return sustainableEvasion;
        }
    }
}
