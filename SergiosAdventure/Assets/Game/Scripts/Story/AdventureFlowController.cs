using System.Text;
using SergiosAdventure.Game.Combat;
using SergiosAdventure.Game.Core;
using SergiosAdventure.Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SergiosAdventure.Game.Story
{
    public sealed class AdventureFlowController : MonoBehaviour
    {
        [SerializeField] private StoryDatabase storyDatabase;
        [SerializeField] private DialogueView dialogueView;
        [SerializeField] private CombatController combatController;
        [SerializeField] private string startNodeId = "intro_gate";

        private StoryNodeData currentNode;
        private bool storyStarted;

        public void BeginStory()
        {
            if (storyDatabase == null || dialogueView == null || combatController == null)
            {
                Debug.LogError("AdventureFlowController necesita referencias completas.");
                return;
            }

            if (!GameSession.HasActiveRun)
            {
                GameSession.StartNewRun("Aventurero");
            }

            storyStarted = true;
            EnterNode(startNodeId);
        }

        private void Start()
        {
            if (!storyStarted)
            {
                BeginStory();
            }
        }

        private void EnterNode(string nodeId)
        {
            currentNode = storyDatabase.GetNode(nodeId);
            if (currentNode == null)
            {
                Debug.LogError($"No existe el nodo {nodeId}.");
                return;
            }

            var status = BuildStatusText();

            switch (currentNode.NodeType)
            {
                case StoryNodeType.Dialogue:
                    dialogueView.ShowDialogueNode(currentNode, status, ResolveChoice);
                    break;
                case StoryNodeType.Combat:
                    dialogueView.ShowCombatNode(currentNode, status, StartCombat);
                    break;
                case StoryNodeType.Ending:
                    dialogueView.ShowEnding(currentNode, status, BuildEndingSummary(currentNode), RestartAdventure, ReturnToMenu);
                    break;
            }
        }

        private void ResolveChoice(int choiceIndex)
        {
            if (currentNode == null || currentNode.Choices == null || choiceIndex < 0 || choiceIndex >= currentNode.Choices.Count)
            {
                return;
            }

            var choice = currentNode.Choices[choiceIndex];
            if (choice == null)
            {
                return;
            }

            if (choice.GrantedItem != null)
            {
                GameSession.AddItem(choice.GrantedItem);
            }

            if (choice.HealthDelta != 0)
            {
                GameSession.ApplyHealthDelta(choice.HealthDelta);
            }

            if (choice.DamageDelta != 0)
            {
                GameSession.ApplyDamageDelta(choice.DamageDelta);
            }

            EnterNode(choice.NextNodeId);
        }

        private void StartCombat()
        {
            if (currentNode == null || currentNode.Enemy == null)
            {
                return;
            }

            combatController.BeginEncounter(currentNode.Enemy, result => ResolveCombat(result));
        }

        private void ResolveCombat(SergiosAdventure.Game.Combat.CombatResult result)
        {
            if (result == null || currentNode == null)
            {
                return;
            }

            var nextNodeId = result.Outcome == SergiosAdventure.Game.Combat.CombatOutcome.Victory
                ? currentNode.VictoryNodeId
                : currentNode.DefeatNodeId;
            EnterNode(nextNodeId);
        }

        private void RestartAdventure()
        {
            var playerName = GameSession.HasActiveRun ? GameSession.PlayerName : "Aventurero";
            GameSession.StartNewRun(playerName);
            SceneManager.LoadScene("Adventure");
        }

        private void ReturnToMenu()
        {
            SceneManager.LoadScene("SampleScene");
        }

        private static string BuildStatusText()
        {
            var builder = new StringBuilder();
            builder.AppendLine(GameSession.PlayerName);
            builder.AppendLine($"Vida: {GameSession.CurrentHealth}/{GameSession.MaxHealth}");
            builder.AppendLine($"Dano: {GameSession.BaseDamage}");
            builder.Append("Bolsa: ");
            builder.Append(GameSession.GetInventorySummary());
            return builder.ToString();
        }

        private static string BuildEndingSummary(StoryNodeData node)
        {
            var builder = new StringBuilder();
            builder.AppendLine(node.Body);
            builder.AppendLine();
            builder.AppendLine($"Vida final: {GameSession.CurrentHealth}/{GameSession.MaxHealth}");
            builder.AppendLine($"Dano final: {GameSession.BaseDamage}");
            builder.Append("Objetos: ");
            builder.Append(GameSession.GetInventorySummary());
            return builder.ToString();
        }
    }
}
