using System.Collections.Generic;
using System.Linq;

public sealed class CombatResult
{
    readonly List<CombatItemStack> drops;

    public CombatOutcome Outcome { get; }
    public EnemyDefinition Enemy { get; }
    public IReadOnlyList<CombatItemStack> Drops => drops;
    public int PlayerRemainingHealth { get; }

    public CombatResult(CombatOutcome outcome, EnemyDefinition enemy, IEnumerable<CombatItemStack> drops, int playerRemainingHealth)
    {
        Outcome = outcome;
        Enemy = enemy;
        PlayerRemainingHealth = playerRemainingHealth;
        this.drops = drops == null
            ? new List<CombatItemStack>()
            : drops.Select(stack => new CombatItemStack(stack.Item, stack.Quantity)).ToList();
    }
}
