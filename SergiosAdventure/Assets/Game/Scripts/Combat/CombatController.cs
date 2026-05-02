using System;
using System.Collections.Generic;
using System.Text;
using SergiosAdventure.Game.Core;
using SergiosAdventure.Game.Data;
using SergiosAdventure.Game.UI;
using UnityEngine;

namespace SergiosAdventure.Game.Combat
{
    public sealed class CombatController : MonoBehaviour
    {
        [SerializeField] private CombatView combatView;

        private readonly StringBuilder logBuilder = new StringBuilder();
        private readonly List<ItemData> currentDrops = new List<ItemData>();
        private readonly List<ItemData> usableItems = new List<ItemData>();

        private EnemyData activeEnemy;
        private Action<CombatResult> onFinished;
        private CombatResult pendingResult;
        private int enemyHealth;
        private bool encounterActive;

        public void BeginEncounter(EnemyData enemy, Action<CombatResult> onEncounterFinished)
        {
            if (enemy == null || combatView == null)
            {
                return;
            }

            activeEnemy = enemy;
            enemyHealth = Mathf.Max(1, enemy.MaxHealth);
            onFinished = onEncounterFinished;
            pendingResult = null;
            encounterActive = true;

            currentDrops.Clear();
            logBuilder.Clear();
            AppendLog($"Un {activeEnemy.EnemyName} bloquea el santuario.");
            AppendLog("El combate empieza. Cada decision cuenta.");

            combatView.Show();
            RefreshActionState();
        }

        private void Attack()
        {
            if (!encounterActive)
            {
                return;
            }

            var damage = GameSession.BaseDamage;
            enemyHealth = Mathf.Max(0, enemyHealth - damage);
            AppendLog($"{GameSession.PlayerName} golpea y causa {damage} de dano.");

            if (enemyHealth <= 0)
            {
                ResolveVictory();
                return;
            }

            ResolveEnemyTurn();
        }

        private void OpenItemMenu()
        {
            if (!encounterActive)
            {
                return;
            }

            usableItems.Clear();
            usableItems.AddRange(GameSession.GetHealingItems());

            if (usableItems.Count == 0)
            {
                AppendLog("No llevas objetos curativos.");
                RefreshActionState();
                return;
            }

            combatView.SetHeader(activeEnemy.EnemyName, BuildPlayerStats(), BuildEnemyStats(), logBuilder.ToString());
            combatView.ShowItems(usableItems, BuildItemLabel, UseItem, RefreshActionState);
        }

        private void UseItem(int index)
        {
            if (!encounterActive || index < 0 || index >= usableItems.Count)
            {
                return;
            }

            var item = usableItems[index];
            if (!GameSession.ConsumeItem(item))
            {
                AppendLog("Ese objeto ya no esta disponible.");
                RefreshActionState();
                return;
            }

            var healed = GameSession.ApplyHealing(item.HealAmount);
            AppendLog($"Usas {item.DisplayName} y recuperas {healed} de vida.");

            if (enemyHealth <= 0)
            {
                ResolveVictory();
                return;
            }

            ResolveEnemyTurn();
        }

        private void ResolveEnemyTurn()
        {
            var dealt = GameSession.ApplyDamage(activeEnemy.Damage);
            AppendLog($"{activeEnemy.EnemyName} contraataca y te quita {dealt} de vida.");

            if (GameSession.CurrentHealth <= 0)
            {
                ResolveDefeat();
                return;
            }

            RefreshActionState();
        }

        private void ResolveVictory()
        {
            encounterActive = false;
            RollDrops();

            if (currentDrops.Count == 0)
            {
                AppendLog($"Has vencido a {activeEnemy.EnemyName}.");
            }
            else
            {
                AppendLog($"Has vencido a {activeEnemy.EnemyName} y recoges {FormatDrops(currentDrops)}.");
            }

            pendingResult = new CombatResult(CombatOutcome.Victory, GameSession.CurrentHealth, new List<ItemData>(currentDrops));
            RefreshResultState("Continuar");
        }

        private void ResolveDefeat()
        {
            encounterActive = false;
            AppendLog("Tu cuerpo cede antes de alcanzar la reliquia.");
            pendingResult = new CombatResult(CombatOutcome.Defeat, GameSession.CurrentHealth, new List<ItemData>());
            RefreshResultState("Aceptar");
        }

        private void RollDrops()
        {
            currentDrops.Clear();

            foreach (var drop in activeEnemy.Drops)
            {
                if (drop == null || drop.Item == null)
                {
                    continue;
                }

                if (UnityEngine.Random.value <= drop.DropChance)
                {
                    currentDrops.Add(drop.Item);
                    GameSession.AddItem(drop.Item);
                }
            }
        }

        private void FinishEncounter()
        {
            combatView.Hide();

            var result = pendingResult;
            var callback = onFinished;

            activeEnemy = null;
            onFinished = null;
            pendingResult = null;
            encounterActive = false;
            usableItems.Clear();
            currentDrops.Clear();
            logBuilder.Clear();

            callback?.Invoke(result);
        }

        private void RefreshActionState()
        {
            combatView.SetHeader(activeEnemy.EnemyName, BuildPlayerStats(), BuildEnemyStats(), logBuilder.ToString());
            combatView.ShowActionState(Attack, OpenItemMenu);
        }

        private void RefreshResultState(string buttonLabel)
        {
            combatView.SetHeader(activeEnemy.EnemyName, BuildPlayerStats(), BuildEnemyStats(), logBuilder.ToString());
            combatView.ShowContinue(buttonLabel, FinishEncounter);
        }

        private string BuildPlayerStats()
        {
            return $"{GameSession.PlayerName}\nVida: {GameSession.CurrentHealth}/{GameSession.MaxHealth}\nDano: {GameSession.BaseDamage}";
        }

        private string BuildEnemyStats()
        {
            return $"{activeEnemy.EnemyName}\nVida: {enemyHealth}/{activeEnemy.MaxHealth}\nDano: {activeEnemy.Damage}";
        }

        private string BuildItemLabel(ItemData item)
        {
            return $"{item.DisplayName} (+{item.HealAmount} vida) x{GameSession.GetItemCount(item)}";
        }

        private void AppendLog(string line)
        {
            if (logBuilder.Length > 0)
            {
                logBuilder.AppendLine();
                logBuilder.AppendLine();
            }

            logBuilder.Append(line);
        }

        private static string FormatDrops(IReadOnlyList<ItemData> drops)
        {
            if (drops == null || drops.Count == 0)
            {
                return "nada";
            }

            var builder = new StringBuilder();

            for (var index = 0; index < drops.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(drops[index].DisplayName);
            }

            return builder.ToString();
        }
    }
}
