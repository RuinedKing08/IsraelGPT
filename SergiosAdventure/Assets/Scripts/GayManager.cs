using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GayManager : MonoBehaviour
{
    const string DefaultPlayerProfilePath = "Combat/Profiles/DefaultPlayerCombatProfile";
    const string EnemyResourceRoot = "Combat/Enemies/";
    const string ItemResourceRoot = "Combat/Items/";

    [SerializeField] private RuntimeStoryGraph storyGraph;
    [SerializeField] private Player player;
    [SerializeField] private PlayerCombatProfile playerProfile;
    [SerializeField] private CombatEncounterController combatController;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private TMP_Text choiceText1;
    [SerializeField] private TMP_Text choiceText2;

    [Header("Fallback Player Stats")]
    [SerializeField] private int initialHealth = 20;
    [SerializeField] private int initialDamage = 4;
    [SerializeField] private int initialBravery;

    private RuntimeStoryNode currentNode;
    private bool waitingForCombat;

    private void Awake()
    {
        ResolveReferences();

        if (combatController != null)
        {
            combatController.OnEncounterFinished += HandleCombatFinished;
        }
    }

    private void Start()
    {
        InitializePlayerState();

        if (storyGraph == null)
        {
            Debug.LogError("Runtime story graph is not assigned.");
            return;
        }

        LoadStartNode();
    }

    private void OnDestroy()
    {
        if (combatController != null)
        {
            combatController.OnEncounterFinished -= HandleCombatFinished;
        }
    }

    private void LoadStartNode()
    {
        RuntimeStoryNode startNode = storyGraph.GetStartNode();
        if (startNode == null)
        {
            Debug.LogError("Runtime story graph has no valid nodes.");
            return;
        }

        LoadNode(startNode.id);
    }

    private void LoadNode(string nodeId)
    {
        currentNode = storyGraph.GetNode(nodeId);

        if (currentNode == null)
        {
            Debug.LogError("Node not found: " + nodeId);
            return;
        }

        titleText.text = currentNode.title;
        bodyText.text = currentNode.body;
        RefreshStatsText();

        switch (currentNode.type)
        {
            case NodeType.Combat:
                HideChoiceButtons();
                StartCombatNode(currentNode);
                break;
            case NodeType.Ending:
                HideChoiceButtons();
                ShowEnding(currentNode);
                break;
            default:
                SetupChoiceButton(choiceButton1, choiceText1, 0);
                SetupChoiceButton(choiceButton2, choiceText2, 1);
                break;
        }
    }

    private void SetupChoiceButton(Button button, TMP_Text buttonText, int index)
    {
        bool hasChoice = currentNode != null && index < currentNode.choices.Count;
        button.gameObject.SetActive(hasChoice);

        if (!hasChoice)
        {
            button.onClick.RemoveAllListeners();
            return;
        }

        RuntimeStoryChoice choice = currentNode.choices[index];
        buttonText.text = choice.text;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectChoice(choice));
    }

    private void SelectChoice(RuntimeStoryChoice choice)
    {
        if (choice == null)
        {
            return;
        }

        ApplyChoiceEffects(choice);
        LoadNode(choice.nextNodeId);
    }

    private void ApplyChoiceEffects(RuntimeStoryChoice choice)
    {
        player.PushDecision($"{currentNode.id}:{choice.text}");

        if (choice.healthChange < 0)
        {
            player.TakeDamage(-choice.healthChange);
            CombatSession.ApplyDamage(-choice.healthChange);
        }
        else if (choice.healthChange > 0)
        {
            player.Heal(choice.healthChange);
            CombatSession.ApplyHealing(choice.healthChange);
        }

        if (choice.damageChange != 0)
        {
            player.ModifyStat(StatType.Attack, choice.damageChange);
            CombatSession.AddDamageBonus(choice.damageChange);
        }

        if (choice.courageChange != 0)
        {
            player.ModifyStat(StatType.Bravery, choice.courageChange);
        }

        if (!string.IsNullOrWhiteSpace(choice.grantedItemId))
        {
            ItemDefinition item = ResolveItem(choice.grantedItemId);
            if (item != null)
            {
                CombatSession.AddItem(item);
            }
            else
            {
                Debug.LogWarning("Item not found: " + choice.grantedItemId);
            }
        }

        RefreshStatsText();
    }

    private void StartCombatNode(RuntimeStoryNode node)
    {
        if (combatController == null)
        {
            Debug.LogError("Combat controller is not assigned.");
            ContinueFromCombat(false);
            return;
        }

        EnemyDefinition enemy = ResolveEnemy(node.enemyId);
        if (enemy == null)
        {
            Debug.LogError("Enemy not found: " + node.enemyId);
            ContinueFromCombat(false);
            return;
        }

        waitingForCombat = true;
        combatController.BeginEncounter(enemy);
    }

    private void HandleCombatFinished(CombatResult result)
    {
        if (!waitingForCombat)
        {
            return;
        }

        waitingForCombat = false;
        SyncPlayerHealthFromCombat(result.PlayerRemainingHealth);
        RefreshStatsText();
        ContinueFromCombat(result.Outcome == CombatOutcome.Victory);
    }

    private void ContinueFromCombat(bool victory)
    {
        string routeNodeId = victory ? currentNode.victoryNodeId : currentNode.defeatNodeId;
        if (!string.IsNullOrWhiteSpace(routeNodeId))
        {
            LoadNode(routeNodeId);
            return;
        }

        int choiceIndex = victory ? 0 : 1;

        if (currentNode == null || choiceIndex >= currentNode.choices.Count)
        {
            Debug.LogWarning("Combat node does not have a " + (victory ? "victory" : "defeat") + " route.");
            return;
        }

        RuntimeStoryChoice route = currentNode.choices[choiceIndex];
        LoadNode(route.nextNodeId);
    }

    private void ShowEnding(RuntimeStoryNode node)
    {
        string endingLabel = string.IsNullOrWhiteSpace(node.endingName) ? node.title : node.endingName;
        Debug.Log($"Ending reached: {endingLabel} ({node.endingId})");
    }

    private EnemyDefinition ResolveEnemy(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
        {
            return null;
        }

        string path = enemyId.Contains("/") ? enemyId : EnemyResourceRoot + enemyId;
        EnemyDefinition enemy = Resources.Load<EnemyDefinition>(path);
        if (enemy != null)
        {
            return enemy;
        }

        foreach (EnemyDefinition candidate in Resources.LoadAll<EnemyDefinition>(EnemyResourceRoot))
        {
            if (candidate.name == enemyId || candidate.EnemyName == enemyId)
            {
                return candidate;
            }
        }

        return null;
    }

    private ItemDefinition ResolveItem(string itemId)
    {
        string path = itemId.Contains("/") ? itemId : ItemResourceRoot + itemId;
        ItemDefinition item = Resources.Load<ItemDefinition>(path);
        if (item != null)
        {
            return item;
        }

        foreach (ItemDefinition candidate in Resources.LoadAll<ItemDefinition>(ItemResourceRoot))
        {
            if (candidate.name == itemId || candidate.ItemId == itemId || candidate.DisplayName == itemId)
            {
                return candidate;
            }
        }

        return null;
    }

    private void InitializePlayerState()
    {
        if (playerProfile == null)
        {
            playerProfile = Resources.Load<PlayerCombatProfile>(DefaultPlayerProfilePath);
        }

        CombatSession.Initialize(playerProfile);

        int health = playerProfile != null ? playerProfile.MaxHealth : initialHealth;
        int damage = playerProfile != null ? playerProfile.BaseDamage : initialDamage;
        string playerName = string.IsNullOrWhiteSpace(PlayerSession.PlayerName) ? "Jugador" : PlayerSession.PlayerName;
        player.Initialize(playerName, health, damage, initialBravery);
        RefreshStatsText();
    }

    private void SyncPlayerHealthFromCombat(int combatHealth)
    {
        int currentHealth = player.GetHealth();
        if (combatHealth < currentHealth)
        {
            player.TakeDamage(currentHealth - combatHealth);
        }
        else if (combatHealth > currentHealth)
        {
            player.Heal(combatHealth - currentHealth);
        }
    }

    private void RefreshStatsText()
    {
        if (statsText == null || player == null)
        {
            return;
        }

        statsText.text = $"HP: {player.GetHealth()} | DMG: {CombatSession.BaseDamage} | Bravery: {player.GetBravery()}";
    }

    private void HideChoiceButtons()
    {
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();
    }

    private void ResolveReferences()
    {
        if (player == null)
        {
            player = GetComponent<Player>();
        }

        if (player == null)
        {
            player = gameObject.AddComponent<Player>();
        }

        if (combatController == null)
        {
            combatController = FindFirstObjectByType<CombatEncounterController>();
        }
    }
}
