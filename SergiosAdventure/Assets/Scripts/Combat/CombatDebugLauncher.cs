using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CombatDebugLauncher : MonoBehaviour
{
    const string DefaultDebugEnemyPath = "Combat/Enemies/DebugWolf";

    [SerializeField] CombatEncounterController encounterController;
    [SerializeField] CombatPanelUI combatUI;
    [SerializeField] EnemyDefinition debugEnemy;

    void Awake()
    {
        if (encounterController == null)
        {
            encounterController = GetComponent<CombatEncounterController>();
        }

        if (combatUI == null)
        {
            combatUI = GetComponent<CombatPanelUI>();
        }

        if (debugEnemy == null)
        {
            debugEnemy = Resources.Load<EnemyDefinition>(DefaultDebugEnemyPath);
        }
    }

    void OnEnable()
    {
        if (encounterController != null)
        {
            encounterController.OnEncounterFinished += HandleEncounterFinished;
        }
    }

    void Start()
    {
        combatUI?.ConfigureDebugButtons("Iniciar combate", debugEnemy != null ? StartDebugEncounter : null, "Resetear estado", ResetDebugState);
        RefreshStatus("Panel de combate listo.");
    }

    void OnDisable()
    {
        if (encounterController != null)
        {
            encounterController.OnEncounterFinished -= HandleEncounterFinished;
        }
    }

    void StartDebugEncounter()
    {
        if (encounterController == null || debugEnemy == null)
        {
            RefreshStatus("No hay enemigo de prueba configurado.");
            return;
        }

        encounterController.BeginEncounter(debugEnemy);
        RefreshStatus($"Combate iniciado contra {debugEnemy.EnemyName}.");
    }

    void ResetDebugState()
    {
        encounterController?.ResetSession();
        RefreshStatus("Estado de combate reiniciado.");
    }

    void HandleEncounterFinished(CombatResult result)
    {
        if (result == null)
        {
            RefreshStatus("El combate termino sin resultado.");
            return;
        }

        string resultText = result.Outcome == CombatOutcome.Victory
            ? $"Victoria contra {result.Enemy.EnemyName}."
            : $"Derrota contra {result.Enemy.EnemyName}.";

        if (result.Outcome == CombatOutcome.Victory && result.Drops.Count > 0)
        {
            resultText += $" Drops: {BuildItemSummary(result.Drops)}.";
        }

        RefreshStatus(resultText);
    }

    void RefreshStatus(string message)
    {
        if (combatUI == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(message))
        {
            builder.AppendLine(message);
        }

        builder.AppendLine($"Jugador: {PlayerDisplayName}");
        builder.AppendLine($"Vida actual: {CombatSession.CurrentHealth}/{CombatSession.MaxHealth}");
        builder.AppendLine($"Dano base: {CombatSession.BaseDamage}");
        builder.AppendLine($"Enemigo debug: {(debugEnemy != null ? debugEnemy.EnemyName : "No configurado")}");
        builder.Append("Inventario: ").Append(BuildItemSummary(CombatSession.GetInventorySnapshot()));

        combatUI.SetDebugStatus(builder.ToString());
    }

    string BuildItemSummary(IReadOnlyList<CombatItemStack> itemStacks)
    {
        if (itemStacks == null || itemStacks.Count == 0)
        {
            return "sin items";
        }

        var parts = new List<string>(itemStacks.Count);
        foreach (CombatItemStack itemStack in itemStacks)
        {
            parts.Add($"{itemStack.Item.DisplayName} x{itemStack.Quantity}");
        }

        return string.Join(", ", parts);
    }

    string PlayerDisplayName => string.IsNullOrWhiteSpace(PlayerSession.PlayerName) ? "Jugador" : PlayerSession.PlayerName;
}
