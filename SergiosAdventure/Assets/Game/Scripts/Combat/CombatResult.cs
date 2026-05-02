using System.Collections.Generic;
using SergiosAdventure.Game.Data;

namespace SergiosAdventure.Game.Combat
{
    public sealed class CombatResult
    {
        public CombatResult(CombatOutcome outcome, int remainingHealth, List<ItemData> drops)
        {
            Outcome = outcome;
            RemainingHealth = remainingHealth;
            Drops = drops ?? new List<ItemData>();
        }

        public CombatOutcome Outcome { get; }

        public int RemainingHealth { get; }

        public IReadOnlyList<ItemData> Drops { get; }
    }
}
