using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatEncounterController : MonoBehaviour
{
    const string DefaultPlayerProfilePath = "Combat/Profiles/DefaultPlayerCombatProfile";

    [SerializeField] PlayerCombatProfile playerProfile;
    [SerializeField] CombatPanelUI combatUI;

    readonly List<string> combatLog = new List<string>();

    EnemyRuntimeState currentEnemy;
    bool encounterActive;

    public event Action<CombatResult> OnEncounterFinished;

    sealed class EnemyRuntimeState
    {
        public EnemyRuntimeState(EnemyDefinition definition)
        {
            Definition = definition;
            CurrentHealth = definition.MaxHealth;
        }

        public EnemyDefinition Definition { get; }
        public int CurrentHealth { get; set; }
    }

    void Awake()
    {
        ResolveReferences();
        ResolveProfile();
        CombatSession.Initialize(playerProfile);
        RefreshCombatHud();
    }

    public void BeginEncounter(EnemyDefinition enemy)
    {
        if (enemy == null || encounterActive)
        {
            return;
        }

        ResolveReferences();
        ResolveProfile();

        if (!CombatSession.IsInitialized)
        {
            CombatSession.Initialize(playerProfile);
        }

        currentEnemy = new EnemyRuntimeState(enemy);
        encounterActive = true;
        combatLog.Clear();
        AppendLog($"Comienza el combate contra {enemy.EnemyName}.");
        combatUI.SetEncounterVisible(true);
        combatUI.SetDebugButtonsInteractable(false, false);
        RefreshCombatHud();
        ShowMainActions();
    }

    public void ResetSession()
    {
        ResolveReferences();
        ResolveProfile();
        CombatSession.ResetFromProfile(playerProfile);
        encounterActive = false;
        currentEnemy = null;
        combatLog.Clear();
        combatUI.SetEncounterVisible(false);
        combatUI.SetDebugButtonsInteractable(true, true);
        RefreshCombatHud();
    }

    void Attack()
    {
        if (!encounterActive || currentEnemy == null)
        {
            return;
        }

        int damage = CombatSession.BaseDamage;
        currentEnemy.CurrentHealth = CombatMath.ApplyDamage(currentEnemy.CurrentHealth, damage);
        AppendLog($"{PlayerDisplayName} ataca y causa {damage} de dano.");

        if (currentEnemy.CurrentHealth <= 0)
        {
            HandleVictory();
            return;
        }

        ResolveEnemyTurn();
    }

    void OpenItemMenu()
    {
        if (!encounterActive)
        {
            return;
        }

        List<CombatItemStack> usableItems = CombatSession.GetUsableHealingItems();
        if (usableItems.Count == 0)
        {
            AppendLog("No tienes items curativos disponibles.");
            RefreshCombatHud();
            ShowMainActions();
            return;
        }

        var buttons = new List<CombatPanelUI.ButtonModel>();
        foreach (CombatItemStack stack in usableItems)
        {
            ItemDefinition item = stack.Item;
            buttons.Add(new CombatPanelUI.ButtonModel(
                $"{item.DisplayName} x{stack.Quantity} (+{item.HealAmount} HP)",
                () => UseHealingItem(item)));
        }

        combatUI.SetItemButtons("Items curativos", buttons, CloseItemMenu);
    }

    void CloseItemMenu()
    {
        combatUI.SetItemMenuVisible(false);
    }

    void UseHealingItem(ItemDefinition item)
    {
        if (!encounterActive || item == null)
        {
            return;
        }

        if (!CombatSession.TryUseHealingItem(item, out int healedAmount))
        {
            AppendLog($"No se pudo usar {item.DisplayName}.");
            RefreshCombatHud();
            ShowMainActions();
            return;
        }

        AppendLog($"{PlayerDisplayName} usa {item.DisplayName} y recupera {healedAmount} HP.");
        combatUI.SetItemMenuVisible(false);
        RefreshCombatHud();
        ResolveEnemyTurn();
    }

    void ResolveEnemyTurn()
    {
        if (!encounterActive || currentEnemy == null)
        {
            return;
        }

        int remainingHealth = CombatSession.ApplyDamage(currentEnemy.Definition.Damage);
        AppendLog($"{currentEnemy.Definition.EnemyName} contraataca y causa {currentEnemy.Definition.Damage} de dano.");
        RefreshCombatHud();

        if (remainingHealth <= 0)
        {
            HandleDefeat();
            return;
        }

        ShowMainActions();
    }

    void HandleVictory()
    {
        List<CombatItemStack> drops = CombatMath.RollDrops(currentEnemy.Definition.Drops, () => UnityEngine.Random.value);
        foreach (CombatItemStack drop in drops)
        {
            CombatSession.AddItem(drop.Item, drop.Quantity);
        }

        AppendLog($"{currentEnemy.Definition.EnemyName} ha sido derrotado.");
        AppendLog(drops.Count == 0 ? "No hubo drops." : $"Drops obtenidos: {FormatItemStacks(drops)}.");
        CombatResult result = new CombatResult(CombatOutcome.Victory, currentEnemy.Definition, drops, CombatSession.CurrentHealth);
        EndEncounter(result);
    }

    void HandleDefeat()
    {
        AppendLog($"{PlayerDisplayName} ha sido derrotado.");
        CombatResult result = new CombatResult(CombatOutcome.Defeat, currentEnemy.Definition, Array.Empty<CombatItemStack>(), CombatSession.CurrentHealth);
        EndEncounter(result);
    }

    void EndEncounter(CombatResult result)
    {
        encounterActive = false;
        currentEnemy = null;
        RefreshCombatHud();
        combatUI.SetEncounterVisible(false);
        combatUI.SetDebugButtonsInteractable(true, true);
        OnEncounterFinished?.Invoke(result);
    }

    void ShowMainActions()
    {
        combatUI.SetActionButtons(new[]
        {
            new CombatPanelUI.ButtonModel("Atacar", Attack),
            new CombatPanelUI.ButtonModel("Usar item", OpenItemMenu)
        });
    }

    void RefreshCombatHud()
    {
        combatUI.SetPlayerStats(PlayerDisplayName, CombatSession.CurrentHealth, CombatSession.MaxHealth);

        if (currentEnemy == null)
        {
            combatUI.SetEnemyStats("Sin enemigo", 0, 0);
        }
        else
        {
            combatUI.SetEnemyStats(currentEnemy.Definition.EnemyName, currentEnemy.CurrentHealth, currentEnemy.Definition.MaxHealth);
        }

        combatUI.SetCombatLog(combatLog);
    }

    void AppendLog(string line)
    {
        combatLog.Add(line);
        combatUI.SetCombatLog(combatLog);
    }

    void ResolveReferences()
    {
        if (combatUI == null)
        {
            combatUI = GetComponent<CombatPanelUI>();
        }

        if (combatUI == null)
        {
            combatUI = gameObject.AddComponent<CombatPanelUI>();
        }

        combatUI.EnsureBuilt();
    }

    void ResolveProfile()
    {
        if (playerProfile == null)
        {
            playerProfile = Resources.Load<PlayerCombatProfile>(DefaultPlayerProfilePath);
        }
    }

    string FormatItemStacks(IReadOnlyList<CombatItemStack> itemStacks)
    {
        return string.Join(", ", itemStacks.Select(itemStack => $"{itemStack.Item.DisplayName} x{itemStack.Quantity}"));
    }

    string PlayerDisplayName => string.IsNullOrWhiteSpace(PlayerSession.PlayerName) ? "Jugador" : PlayerSession.PlayerName;
}
