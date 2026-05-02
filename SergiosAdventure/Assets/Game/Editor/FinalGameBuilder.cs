using System.Collections.Generic;
using System.Reflection;
using SergiosAdventure.Game.Combat;
using SergiosAdventure.Game.Core;
using SergiosAdventure.Game.Data;
using SergiosAdventure.Game.Story;
using SergiosAdventure.Game.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SergiosAdventure.Game.Editor
{
    public static class FinalGameBuilder
    {
        private static readonly Color BackgroundColor = new Color32(20, 14, 11, 255);
        private static readonly Color BackdropColor = new Color32(40, 28, 22, 255);
        private static readonly Color CardColor = new Color32(202, 176, 132, 242);
        private static readonly Color CardBorderColor = new Color32(88, 58, 40, 255);
        private static readonly Color PanelColor = new Color32(228, 211, 177, 238);
        private static readonly Color ButtonColor = new Color32(92, 61, 42, 255);
        private static readonly Color ButtonHighlightColor = new Color32(120, 79, 54, 255);
        private static readonly Color ButtonPressedColor = new Color32(67, 45, 31, 255);
        private static readonly Color ButtonTextColor = new Color32(247, 238, 222, 255);
        private static readonly Color BodyTextColor = new Color32(36, 25, 19, 255);
        private static readonly Color AccentTextColor = new Color32(104, 61, 35, 255);
        private static readonly Color OverlayColor = new Color32(13, 9, 8, 196);
        private static readonly Color ErrorTextColor = new Color32(132, 42, 31, 255);

        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string RegistroScenePath = "Assets/Scenes/Registro.unity";
        private const string AdventureScenePath = "Assets/Scenes/Adventure.unity";
        private const string StoryAssetPath = "Assets/Game/Data/Story/MainStoryDatabase.asset";
        private const string SalveAssetPath = "Assets/Game/Data/Combat/PomadaDeRescoldo.asset";
        private const string TonicAssetPath = "Assets/Game/Data/Combat/TisanaDelGuardian.asset";
        private const string EnemyAssetPath = "Assets/Game/Data/Combat/GuardianDelVelo.asset";

        [MenuItem("SergiosAdventure/Rebuild Final Game")]
        public static void RebuildFinalGame()
        {
            EnsureProjectFolders();

            var salve = CreateOrUpdateItem(SalveAssetPath, "pomada_rescoldo", "Pomada de rescoldo", "Una mezcla tibia de hierbas y ceniza que calma heridas recientes.", 8);
            var tonic = CreateOrUpdateItem(TonicAssetPath, "tisana_guardian", "Tisana del guardian", "Un brebaje espeso que mantiene el pulso firme despues del combate.", 6);
            var guardian = CreateOrUpdateEnemy(EnemyAssetPath, tonic);
            var storyDatabase = CreateOrUpdateStoryDatabase(StoryAssetPath, salve, guardian);

            BuildSampleScene();
            BuildRegistroScene();
            BuildAdventureScene(storyDatabase);
            ConfigureBuildSettings();
            ArchiveLegacyAssets();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Juego final reconstruido correctamente.");
        }

        public static void RebuildFinalGameBatch()
        {
            RebuildFinalGame();
        }

        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets/Game");
            EnsureFolder("Assets/Game/Data");
            EnsureFolder("Assets/Game/Data/Story");
            EnsureFolder("Assets/Game/Data/Combat");
            EnsureFolder("Assets/Game/Scripts");
            EnsureFolder("Assets/Game/Editor");
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Contributors");
            EnsureFolder("Assets/Contributors/Sergio");
            EnsureFolder("Assets/Contributors/Mateo");
            EnsureFolder("Assets/Contributors/Juan");
            EnsureFolder("Assets/Contributors/Milton");
            EnsureFolder("Assets/Contributors/Oscar");
        }

        private static ItemData CreateOrUpdateItem(string assetPath, string itemId, string displayName, string description, int healAmount)
        {
            var item = LoadOrCreateAsset<ItemData>(assetPath);
            item.ItemId = itemId;
            item.DisplayName = displayName;
            item.Description = description;
            item.HealAmount = healAmount;
            EditorUtility.SetDirty(item);
            return item;
        }

        private static EnemyData CreateOrUpdateEnemy(string assetPath, ItemData dropItem)
        {
            var enemy = LoadOrCreateAsset<EnemyData>(assetPath);
            enemy.EnemyName = "Guardian del Velo";
            enemy.MaxHealth = 18;
            enemy.Damage = 5;
            enemy.Drops = new List<SergiosAdventure.Game.Data.EnemyDropEntry>
            {
                new SergiosAdventure.Game.Data.EnemyDropEntry
                {
                    Item = dropItem,
                    DropChance = 1f,
                },
            };

            EditorUtility.SetDirty(enemy);
            return enemy;
        }

        private static StoryDatabase CreateOrUpdateStoryDatabase(string assetPath, ItemData salve, EnemyData guardian)
        {
            var database = LoadOrCreateAsset<StoryDatabase>(assetPath);
            var nodes = database.MutableNodes;
            nodes.Clear();

            nodes.Add(CreateDialogueNode(
                "intro_gate",
                "Las ruinas del velo",
                "La puerta de piedra respira humedad y ceniza. Una campana rota cuelga del arco, como si alguien hubiera querido avisar que entrar aqui tiene un precio.",
                CreateChoice("Revisar el brasero apagado junto al porton", "brazier_cache"),
                CreateChoice("Seguir la voz del cuidador entre las sombras", "caretaker_warning")));

            nodes.Add(CreateDialogueNode(
                "brazier_cache",
                "Bajo la ceniza",
                "Bajo el brasero encuentras una bolsa de cuero seca. Dentro queda una pomada tibia y un mapa a medio quemar que apunta hacia el archivo.",
                CreateChoice("Guardar la pomada y seguir al archivo", "archive", grantedItem: salve),
                CreateChoice("Dejar la bolsa intacta y avanzar con cuidado", "archive")));

            nodes.Add(CreateDialogueNode(
                "caretaker_warning",
                "El cuidador sin rostro",
                "Una figura envuelta en velos murmura que la reliquia escucha el miedo. Si respetas el recorrido, llegaras entero; si te apuras, el santuario cobrara su deuda.",
                CreateChoice("Escuchar la advertencia y tomar aire", "archive", healthDelta: 2),
                CreateChoice("Ignorarlo y correr hacia el pasillo espinoso", "thorn_corridor", healthDelta: -3)));

            nodes.Add(CreateDialogueNode(
                "archive",
                "El archivo hundido",
                "Decenas de tablillas narran como cayeron los antiguos guardianes. Entre polvo y cera, una nota describe el punto debil de la criatura que custodia la camara final.",
                CreateChoice("Leer las notas y preparar el golpe", "thorn_corridor", damageDelta: 2),
                CreateChoice("Atajar entre estantes derruidos", "thorn_corridor", healthDelta: -2)));

            nodes.Add(CreateDialogueNode(
                "thorn_corridor",
                "Corredor de espinas",
                "El ultimo pasaje esta cubierto de lianas negras que se cierran al escuchar pasos. Al fondo, una luz roja late al ritmo de un corazon ajeno.",
                CreateChoice("Cruzar el corredor con paciencia", "guardian_combat"),
                CreateChoice("Forzar el paso antes de que cierre", "guardian_combat", healthDelta: -2)));

            nodes.Add(new StoryNodeData
            {
                Id = "guardian_combat",
                Title = "La camara del guardian",
                Body = "La reliquia descansa sobre un altar de obsidiana. Frente a ella despierta el Guardian del Velo, cubierto de placas y espinas.",
                NodeType = StoryNodeType.Combat,
                Enemy = guardian,
                VictoryNodeId = "relic_choice",
                DefeatNodeId = "ending_bad",
                EndingType = EndingType.None,
                Choices = new List<StoryChoiceData>(),
            });

            nodes.Add(CreateDialogueNode(
                "relic_choice",
                "El altar abierto",
                "La criatura cae y el santuario queda en silencio. La reliquia late en tu palma como si esperara una ultima orden.",
                CreateChoice("Destruir la reliquia y romper el ciclo", "ending_good"),
                CreateChoice("Salir con la reliquia envuelta en tela", "ending_neutral"),
                CreateChoice("Dejarla quieta y marcharte con el conocimiento", "ending_neutral")));

            nodes.Add(CreateEndingNode(
                "ending_good",
                "Ceniza liberada",
                "Golpeas la reliquia contra el altar y la luz roja se disuelve en una lluvia de polvo. Las ruinas por fin exhalan un silencio limpio.",
                EndingType.Good));

            nodes.Add(CreateEndingNode(
                "ending_neutral",
                "Una deuda incompleta",
                "Abandonas las ruinas con un pacto a medias. Tal vez te llevas la reliquia, tal vez solo sus secretos, pero el santuario seguira pesando sobre tu memoria.",
                EndingType.Neutral));

            nodes.Add(CreateEndingNode(
                "ending_bad",
                "Bajo el velo",
                "El guardian te derriba antes de que puedas alcanzar el altar. La camara vuelve a cerrarse y tu nombre queda atrapado entre espinas y piedra.",
                EndingType.Bad));

            EditorUtility.SetDirty(database);
            return database;
        }

        private static StoryNodeData CreateDialogueNode(string id, string title, string body, params StoryChoiceData[] choices)
        {
            return new StoryNodeData
            {
                Id = id,
                Title = title,
                Body = body,
                NodeType = StoryNodeType.Dialogue,
                Choices = new List<StoryChoiceData>(choices),
                EndingType = EndingType.None,
            };
        }

        private static StoryNodeData CreateEndingNode(string id, string title, string body, EndingType endingType)
        {
            return new StoryNodeData
            {
                Id = id,
                Title = title,
                Body = body,
                NodeType = StoryNodeType.Ending,
                Choices = new List<StoryChoiceData>(),
                EndingType = endingType,
            };
        }

        private static StoryChoiceData CreateChoice(string label, string nextNodeId, int healthDelta = 0, int damageDelta = 0, ItemData grantedItem = null)
        {
            return new StoryChoiceData
            {
                Label = label,
                NextNodeId = nextNodeId,
                HealthDelta = healthDelta,
                DamageDelta = damageDelta,
                GrantedItem = grantedItem,
            };
        }

        private static void BuildSampleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera("Main Camera");
            CreateEventSystem();

            var canvas = CreateCanvas("MainMenuCanvas");
            CreateFullScreenImage(canvas.transform, "Background", BackgroundColor);
            CreateFullScreenImage(canvas.transform, "Backdrop", new Color(BackdropColor.r, BackdropColor.g, BackdropColor.b, 0.75f));

            var menuCard = CreateCenteredPanel(canvas.transform, "MenuCard", new Vector2(840f, 620f), Vector2.zero, CardColor);
            AddShadow(menuCard, CardBorderColor);
            var menuContent = CreateVerticalLayoutRoot(menuCard.transform, "Content", new RectOffset(48, 48, 48, 48), 18f, TextAnchor.UpperCenter);

            CreateLayoutText(menuContent.transform, "Title", "Las ruinas del velo", 58, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.Center, 100f);
            CreateLayoutText(menuContent.transform, "Subtitle", "Una aventura breve de decisiones, combate y reliquias malditas.", 24, FontStyles.Normal, AccentTextColor, TextAlignmentOptions.Center, 64f);
            CreateSpacer(menuContent.transform, "Spacer", 10f);
            CreateDivider(menuContent.transform, "Divider");

            var buttonGroup = CreateVerticalLayoutRoot(menuContent.transform, "ButtonGroup", new RectOffset(40, 40, 12, 12), 16f, TextAnchor.UpperCenter);
            SetLayoutElement(buttonGroup, -1f, 300f, true);

            var startButton = CreateButton(buttonGroup.transform, "StartButton", "Comenzar", 78f);
            var creditsButton = CreateButton(buttonGroup.transform, "CreditsButton", "Creditos", 78f);
            var exitButton = CreateButton(buttonGroup.transform, "ExitButton", "Salir", 78f);

            CreateLayoutText(menuContent.transform, "Footer", "Disenado para PC 16:9. Todo el flujo queda visible y editable en la jerarquia.", 18, FontStyles.Italic, AccentTextColor, TextAlignmentOptions.Center, 64f);

            var creditsOverlay = CreateFullScreenPanel(canvas.transform, "CreditsOverlay", OverlayColor);
            creditsOverlay.SetActive(false);
            var creditsCard = CreateCenteredPanel(creditsOverlay.transform, "CreditsCard", new Vector2(760f, 430f), Vector2.zero, PanelColor);
            AddShadow(creditsCard, CardBorderColor);
            var creditsContent = CreateVerticalLayoutRoot(creditsCard.transform, "Content", new RectOffset(40, 40, 40, 40), 16f, TextAnchor.UpperCenter);

            CreateLayoutText(creditsContent.transform, "CreditsTitle", "Equipo", 40, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.Center, 62f);
            CreateLayoutText(
                creditsContent.transform,
                "CreditsBody",
                "Sergio\nMateo\nJuan\nMilton\nOscar\n\nCada carpeta de legado conserva sus aportes originales para consulta.",
                24,
                FontStyles.Normal,
                BodyTextColor,
                TextAlignmentOptions.Center,
                210f);
            var closeCreditsButton = CreateButton(creditsContent.transform, "CloseCreditsButton", "Cerrar", 72f);

            var controller = canvas.gameObject.AddComponent<MainMenuController>();
            SetObjectField(controller, "startButton", startButton);
            SetObjectField(controller, "creditsButton", creditsButton);
            SetObjectField(controller, "exitButton", exitButton);
            SetObjectField(controller, "closeCreditsButton", closeCreditsButton);
            SetObjectField(controller, "creditsPanel", creditsOverlay);

            EditorSceneManager.SaveScene(scene, SampleScenePath);
        }

        private static void BuildRegistroScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera("Main Camera");
            CreateEventSystem();

            var canvas = CreateCanvas("RegistroCanvas");
            CreateFullScreenImage(canvas.transform, "Background", BackgroundColor);
            CreateFullScreenImage(canvas.transform, "Backdrop", new Color(BackdropColor.r, BackdropColor.g, BackdropColor.b, 0.78f));

            var card = CreateCenteredPanel(canvas.transform, "RegistroCard", new Vector2(760f, 560f), Vector2.zero, CardColor);
            AddShadow(card, CardBorderColor);
            var content = CreateVerticalLayoutRoot(card.transform, "Content", new RectOffset(44, 44, 44, 44), 18f, TextAnchor.UpperCenter);

            CreateLayoutText(content.transform, "Title", "Registro de expedicion", 50, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.Center, 78f);
            CreateLayoutText(content.transform, "Subtitle", "Antes de entrar al santuario, deja el nombre con el que la ruina te recordara.", 23, FontStyles.Normal, AccentTextColor, TextAlignmentOptions.Center, 78f);
            CreateLayoutText(content.transform, "Label", "Nombre del aventurero", 22, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.Left, 34f);
            var inputField = CreateInputField(content.transform, "NameInput", "Escribe tu nombre aqui");
            CreateLayoutText(content.transform, "Hint", "Pulsa Enter o usa el boton Continuar.", 18, FontStyles.Italic, AccentTextColor, TextAlignmentOptions.Left, 28f);
            var feedbackText = CreateLayoutText(content.transform, "FeedbackText", string.Empty, 20, FontStyles.Bold, ErrorTextColor, TextAlignmentOptions.Left, 30f);

            var buttonRow = CreateHorizontalLayoutRoot(content.transform, "ButtonRow", new RectOffset(0, 0, 8, 0), 20f, TextAnchor.MiddleCenter);
            SetLayoutElement(buttonRow, -1f, 82f, true);
            var backButton = CreateButton(buttonRow.transform, "BackButton", "Volver", 76f, flexibleWidth: 1f);
            var continueButton = CreateButton(buttonRow.transform, "ContinueButton", "Continuar", 76f, flexibleWidth: 1f);

            var controller = canvas.gameObject.AddComponent<RegistroController>();
            SetObjectField(controller, "nameInput", inputField);
            SetObjectField(controller, "continueButton", continueButton);
            SetObjectField(controller, "backButton", backButton);
            SetObjectField(controller, "feedbackText", feedbackText);

            EditorSceneManager.SaveScene(scene, RegistroScenePath);
        }

        private static void BuildAdventureScene(StoryDatabase storyDatabase)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera("Main Camera");
            CreateEventSystem();

            var canvas = CreateCanvas("AdventureCanvas");
            CreateFullScreenImage(canvas.transform, "Background", BackgroundColor);
            CreateFullScreenImage(canvas.transform, "Backdrop", new Color(BackdropColor.r, BackdropColor.g, BackdropColor.b, 0.78f));

            var header = CreateAnchoredPanel(canvas.transform, "AdventureHeader", new Vector2(420f, 92f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -48f), PanelColor);
            AddShadow(header, CardBorderColor);
            var headerText = CreateText(header.transform, "HeaderText", "Las ruinas del velo", 32, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.Center);
            StretchToFill(headerText.rectTransform, new Vector2(18f, 14f), new Vector2(-18f, -14f));

            var statusPanel = CreateAnchoredPanel(canvas.transform, "StatusPanel", new Vector2(360f, 176f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-54f, -56f), PanelColor);
            AddShadow(statusPanel, CardBorderColor);
            var statusText = CreateText(statusPanel.transform, "StatusText", "Estado", 24, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.TopLeft);
            StretchToFill(statusText.rectTransform, new Vector2(22f, 18f), new Vector2(-22f, -18f));

            var dialoguePanel = CreateAnchoredPanel(canvas.transform, "DialoguePanel", new Vector2(1240f, 340f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 54f), CardColor);
            AddShadow(dialoguePanel, CardBorderColor);
            var dialogueContent = CreateVerticalLayoutRoot(dialoguePanel.transform, "Content", new RectOffset(34, 34, 28, 28), 14f, TextAnchor.UpperLeft);

            var titleText = CreateLayoutText(dialogueContent.transform, "TitleText", "Titulo", 38, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.Left, 54f);
            var bodyText = CreateLayoutText(dialogueContent.transform, "BodyText", "Cuerpo", 24, FontStyles.Normal, BodyTextColor, TextAlignmentOptions.TopLeft, 110f);
            bodyText.textWrappingMode = TextWrappingModes.Normal;

            var choiceGroup = CreateVerticalLayoutRoot(dialogueContent.transform, "ChoiceGroup", new RectOffset(0, 0, 4, 0), 12f, TextAnchor.UpperCenter);
            SetLayoutElement(choiceGroup, -1f, 166f, true);

            var choiceViews = new DialogueView.ChoiceButtonView[3];
            for (var index = 0; index < choiceViews.Length; index++)
            {
                var button = CreateButton(choiceGroup.transform, $"ChoiceButton{index + 1}", $"Opcion {index + 1}", 48f);
                choiceViews[index] = new DialogueView.ChoiceButtonView
                {
                    Button = button,
                    Label = button.GetComponentInChildren<TMP_Text>(),
                };
            }

            var endingOverlay = CreateFullScreenPanel(canvas.transform, "EndingOverlay", OverlayColor);
            endingOverlay.SetActive(false);
            var endingCard = CreateCenteredPanel(endingOverlay.transform, "EndingCard", new Vector2(860f, 490f), Vector2.zero, PanelColor);
            AddShadow(endingCard, CardBorderColor);
            var endingContent = CreateVerticalLayoutRoot(endingCard.transform, "Content", new RectOffset(38, 38, 38, 38), 18f, TextAnchor.UpperCenter);
            var endingTitleText = CreateLayoutText(endingContent.transform, "EndingTitleText", "Fin", 42, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.Center, 60f);
            var endingBodyText = CreateLayoutText(endingContent.transform, "EndingBodyText", "Resumen", 24, FontStyles.Normal, BodyTextColor, TextAlignmentOptions.TopLeft, 230f);
            endingBodyText.textWrappingMode = TextWrappingModes.Normal;
            var endingButtons = CreateHorizontalLayoutRoot(endingContent.transform, "EndingButtons", new RectOffset(0, 0, 8, 0), 18f, TextAnchor.MiddleCenter);
            SetLayoutElement(endingButtons, -1f, 80f, true);
            var restartButton = CreateButton(endingButtons.transform, "RestartButton", "Reiniciar", 72f, flexibleWidth: 1f);
            var menuButton = CreateButton(endingButtons.transform, "MenuButton", "Volver al menu", 72f, flexibleWidth: 1f);

            var combatOverlay = CreateFullScreenPanel(canvas.transform, "CombatOverlay", OverlayColor);
            combatOverlay.SetActive(false);
            var combatCard = CreateCenteredPanel(combatOverlay.transform, "CombatCard", new Vector2(820f, 460f), Vector2.zero, PanelColor);
            AddShadow(combatCard, CardBorderColor);
            var combatContent = CreateVerticalLayoutRoot(combatCard.transform, "Content", new RectOffset(32, 32, 28, 28), 14f, TextAnchor.UpperCenter);
            var combatTitleText = CreateLayoutText(combatContent.transform, "CombatTitleText", "Combate", 40, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.Center, 52f);

            var statRow = CreateHorizontalLayoutRoot(combatContent.transform, "StatRow", new RectOffset(0, 0, 0, 0), 16f, TextAnchor.UpperCenter);
            SetLayoutElement(statRow, -1f, 92f, true);
            var playerStatsPanel = CreateStretchPanel(statRow.transform, "PlayerStatsPanel", PanelColor);
            var playerStatsText = CreateText(playerStatsPanel.transform, "PlayerStatsText", "Jugador", 22, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.TopLeft);
            StretchToFill(playerStatsText.rectTransform, new Vector2(16f, 12f), new Vector2(-16f, -12f));

            var enemyStatsPanel = CreateStretchPanel(statRow.transform, "EnemyStatsPanel", PanelColor);
            var enemyStatsText = CreateText(enemyStatsPanel.transform, "EnemyStatsText", "Enemigo", 22, FontStyles.Bold, BodyTextColor, TextAlignmentOptions.TopLeft);
            StretchToFill(enemyStatsText.rectTransform, new Vector2(16f, 12f), new Vector2(-16f, -12f));

            var logPanel = CreateStretchPanel(combatContent.transform, "LogPanel", CardColor);
            SetLayoutElement(logPanel, -1f, 140f, true);
            var logText = CreateText(logPanel.transform, "LogText", "Log", 23, FontStyles.Normal, BodyTextColor, TextAlignmentOptions.TopLeft);
            StretchToFill(logText.rectTransform, new Vector2(18f, 16f), new Vector2(-18f, -16f));
            logText.textWrappingMode = TextWrappingModes.Normal;

            var actionRow = CreateHorizontalLayoutRoot(combatContent.transform, "ActionRow", new RectOffset(0, 0, 4, 0), 18f, TextAnchor.MiddleCenter);
            SetLayoutElement(actionRow, -1f, 76f, true);
            var attackButton = CreateButton(actionRow.transform, "AttackButton", "Atacar", 72f, flexibleWidth: 1f);
            var itemsButton = CreateButton(actionRow.transform, "ItemsButton", "Usar item", 72f, flexibleWidth: 1f);

            var itemPanel = CreateVerticalLayoutRoot(combatContent.transform, "ItemPanel", new RectOffset(0, 0, 4, 0), 10f, TextAnchor.UpperCenter);
            itemPanel.gameObject.SetActive(false);
            SetLayoutElement(itemPanel, -1f, 170f, true);
            var itemButtons = new CombatView.ItemButtonView[3];
            for (var index = 0; index < itemButtons.Length; index++)
            {
                var button = CreateButton(itemPanel.transform, $"ItemButton{index + 1}", $"Objeto {index + 1}", 48f);
                itemButtons[index] = new CombatView.ItemButtonView
                {
                    Button = button,
                    Label = button.GetComponentInChildren<TMP_Text>(),
                };
            }

            var itemBackButton = CreateButton(itemPanel.transform, "ItemBackButton", "Volver", 48f);
            var continueButton = CreateButton(combatContent.transform, "ContinueButton", "Continuar", 72f);
            continueButton.gameObject.SetActive(false);

            var dialogueView = dialoguePanel.gameObject.AddComponent<DialogueView>();
            SetObjectField(dialogueView, "titleText", titleText);
            SetObjectField(dialogueView, "bodyText", bodyText);
            SetObjectField(dialogueView, "statusText", statusText);
            SetFieldValue(dialogueView, "choiceButtons", choiceViews);
            SetObjectField(dialogueView, "endingPanel", endingOverlay);
            SetObjectField(dialogueView, "endingTitleText", endingTitleText);
            SetObjectField(dialogueView, "endingBodyText", endingBodyText);
            SetObjectField(dialogueView, "restartButton", restartButton);
            SetObjectField(dialogueView, "menuButton", menuButton);

            var combatView = combatOverlay.gameObject.AddComponent<CombatView>();
            SetObjectField(combatView, "root", combatOverlay);
            SetObjectField(combatView, "titleText", combatTitleText);
            SetObjectField(combatView, "logText", logText);
            SetObjectField(combatView, "playerStatsText", playerStatsText);
            SetObjectField(combatView, "enemyStatsText", enemyStatsText);
            SetObjectField(combatView, "attackButton", attackButton);
            SetObjectField(combatView, "itemsButton", itemsButton);
            SetObjectField(combatView, "continueButton", continueButton);
            SetObjectField(combatView, "continueButtonText", continueButton.GetComponentInChildren<TMP_Text>());
            SetObjectField(combatView, "actionRow", actionRow);
            SetObjectField(combatView, "itemPanel", itemPanel.gameObject);
            SetObjectField(combatView, "itemBackButton", itemBackButton);
            SetFieldValue(combatView, "itemButtons", itemButtons);

            var runtimeRoot = new GameObject("AdventureRuntime");
            var combatController = runtimeRoot.AddComponent<CombatController>();
            SetObjectField(combatController, "combatView", combatView);

            var flowController = runtimeRoot.AddComponent<AdventureFlowController>();
            SetObjectField(flowController, "storyDatabase", storyDatabase);
            SetObjectField(flowController, "dialogueView", dialogueView);
            SetObjectField(flowController, "combatController", combatController);
            SetObjectField(flowController, "startNodeId", "intro_gate");

            EditorSceneManager.SaveScene(scene, AdventureScenePath);
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SampleScenePath, true),
                new EditorBuildSettingsScene(RegistroScenePath, true),
                new EditorBuildSettingsScene(AdventureScenePath, true),
            };

            EditorBuildSettings.RemoveConfigObject("com.unity.addressableassets");
            EditorBuildSettings.RemoveConfigObject("com.unity.localization.settings");
        }

        private static void ArchiveLegacyAssets()
        {
            MoveAssetIfPresent("Assets/Scripts/Combat", "Assets/Contributors/Sergio/LegacyCombat~");
            MoveAssetIfPresent("Assets/Scripts/Core", "Assets/Contributors/Sergio/LegacyCore~");
            MoveAssetIfPresent("Assets/Scripts/Story", "Assets/Contributors/Mateo/LegacyStory~");
            MoveAssetIfPresent("Assets/Scripts/Localization", "Assets/Contributors/Juan/LegacyLocalizationScripts~");
            MoveAssetIfPresent("Assets/Scripts/UI", "Assets/Contributors/Oscar/LegacyUi~");

            MoveAssetIfPresent("Assets/Resources/Combat", "Assets/Contributors/Sergio/LegacyCombatData~");
            MoveAssetIfPresent("Assets/Resources/Story", "Assets/Contributors/Mateo/LegacyStoryData~");
            MoveAssetIfPresent("Assets/Localization", "Assets/Contributors/Juan/LegacyLocalizationAssets~");
            MoveAssetIfPresent("Assets/Settings", "Assets/Contributors/Oscar/LegacySettings~");
            MoveAssetIfPresent("Assets/Prefab", "Assets/Contributors/Sergio/LegacyPrefabs~");
            MoveAssetIfPresent("Assets/AddressableAssetsData", "Assets/Contributors/Juan/LegacyAddressables~");
            MoveAssetIfPresent("Assets/Tests", "Assets/Contributors/Milton/LegacyTests~");
            MoveAssetIfPresent("Assets/TutorialInfo", "Assets/Contributors/Oscar/TutorialInfo~");

            DeleteFolderIfEmpty("Assets/Scripts");
            DeleteFolderIfEmpty("Assets/Resources");
        }

        private static void MoveAssetIfPresent(string sourcePath, string destinationPath)
        {
            if (!PathExists(sourcePath))
            {
                return;
            }

            if (PathExists(destinationPath))
            {
                AssetDatabase.DeleteAsset(destinationPath);
            }

            EnsureFolder(GetParentFolder(destinationPath));
            var error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning($"No se pudo mover {sourcePath}: {error}");
            }
        }

        private static void DeleteFolderIfEmpty(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var children = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
            if (children.Length <= 1)
            {
                AssetDatabase.DeleteAsset(folderPath);
            }
        }

        private static T LoadOrCreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            EnsureFolder(GetParentFolder(assetPath));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void CreateCamera(string name)
        {
            var cameraObject = new GameObject(name, typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.orthographic = true;
            camera.nearClipPlane = -100f;
            camera.farClipPlane = 100f;
        }

        private static void CreateEventSystem()
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private static GameObject CreateFullScreenImage(Transform parent, string name, Color color)
        {
            var go = CreateUiObject(name, parent);
            StretchToFill(go.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var image = go.AddComponent<Image>();
            image.sprite = LoadSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            return go;
        }

        private static GameObject CreateFullScreenPanel(Transform parent, string name, Color color)
        {
            return CreateFullScreenImage(parent, name, color);
        }

        private static GameObject CreateCenteredPanel(Transform parent, string name, Vector2 size, Vector2 position, Color color)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.AddComponent<Image>();
            image.sprite = LoadSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            return go;
        }

        private static GameObject CreateAnchoredPanel(Transform parent, string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Color color)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.AddComponent<Image>();
            image.sprite = LoadSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            return go;
        }

        private static GameObject CreateStretchPanel(Transform parent, string name, Color color)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.sprite = LoadSprite();
            image.type = Image.Type.Sliced;
            image.color = color;

            SetLayoutElement(go, -1f, -1f, true, 1f);
            return go;
        }

        private static TMP_Text CreateText(Transform parent, string name, string text, int fontSize, FontStyles fontStyle, Color color, TextAlignmentOptions alignment)
        {
            var go = CreateUiObject(name, parent);
            var textComponent = go.AddComponent<TextMeshProUGUI>();
            textComponent.font = LoadFont();
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.color = color;
            textComponent.alignment = alignment;
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            textComponent.text = text;
            return textComponent;
        }

        private static TMP_Text CreateLayoutText(Transform parent, string name, string text, int fontSize, FontStyles fontStyle, Color color, TextAlignmentOptions alignment, float preferredHeight)
        {
            var textComponent = CreateText(parent, name, text, fontSize, fontStyle, color, alignment);
            StretchToFill(textComponent.rectTransform, Vector2.zero, Vector2.zero);
            SetLayoutElement(textComponent.gameObject, -1f, preferredHeight, false);
            return textComponent;
        }

        private static void CreateSpacer(Transform parent, string name, float height)
        {
            var spacer = CreateUiObject(name, parent);
            SetLayoutElement(spacer, -1f, height, false);
        }

        private static void CreateDivider(Transform parent, string name)
        {
            var divider = CreateUiObject(name, parent);
            var image = divider.AddComponent<Image>();
            image.sprite = LoadSprite();
            image.type = Image.Type.Sliced;
            image.color = CardBorderColor;
            SetLayoutElement(divider, -1f, 3f, false);
        }

        private static GameObject CreateVerticalLayoutRoot(Transform parent, string name, RectOffset padding, float spacing, TextAnchor alignment)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            return go;
        }

        private static GameObject CreateHorizontalLayoutRoot(Transform parent, string name, RectOffset padding, float spacing, TextAnchor alignment)
        {
            var go = CreateUiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            return go;
        }

        private static Button CreateButton(Transform parent, string name, string label, float preferredHeight, float flexibleWidth = 0f)
        {
            var go = CreateUiObject(name, parent);
            var image = go.AddComponent<Image>();
            image.sprite = LoadSprite();
            image.type = Image.Type.Sliced;
            image.color = ButtonColor;

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = ButtonHighlightColor;
            colors.selectedColor = ButtonHighlightColor;
            colors.pressedColor = ButtonPressedColor;
            colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.6f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            AddShadow(go, CardBorderColor);

            var labelText = CreateText(go.transform, "Label", label, 24, FontStyles.Bold, ButtonTextColor, TextAlignmentOptions.Center);
            StretchToFill(labelText.rectTransform, new Vector2(18f, 8f), new Vector2(-18f, -8f));

            SetLayoutElement(go, -1f, preferredHeight, true, flexibleWidth);
            return button;
        }

        private static TMP_InputField CreateInputField(Transform parent, string name, string placeholderText)
        {
            var go = CreateUiObject(name, parent);
            var image = go.AddComponent<Image>();
            image.sprite = LoadSprite();
            image.type = Image.Type.Sliced;
            image.color = PanelColor;
            AddShadow(go, CardBorderColor);

            var inputField = go.AddComponent<TMP_InputField>();
            inputField.characterLimit = 24;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.selectionColor = new Color32(150, 109, 76, 84);
            inputField.caretColor = BodyTextColor;

            var textArea = CreateUiObject("TextArea", go.transform);
            StretchToFill(textArea.GetComponent<RectTransform>(), new Vector2(20f, 14f), new Vector2(-20f, -14f));

            var text = CreateText(textArea.transform, "Text", string.Empty, 24, FontStyles.Normal, BodyTextColor, TextAlignmentOptions.MidlineLeft);
            StretchToFill(text.rectTransform, Vector2.zero, Vector2.zero);
            text.textWrappingMode = TextWrappingModes.NoWrap;

            var placeholder = CreateText(textArea.transform, "Placeholder", placeholderText, 24, FontStyles.Italic, new Color(0.42f, 0.32f, 0.26f, 0.65f), TextAlignmentOptions.MidlineLeft);
            StretchToFill(placeholder.rectTransform, Vector2.zero, Vector2.zero);

            inputField.textViewport = textArea.GetComponent<RectTransform>();
            inputField.textComponent = text;
            inputField.placeholder = placeholder;

            SetLayoutElement(go, -1f, 72f, false);
            return inputField;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchToFill(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void SetLayoutElement(GameObject go, float preferredWidth, float preferredHeight, bool flexibleHeight, float flexibleWidth = 0f)
        {
            var layout = go.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = go.AddComponent<LayoutElement>();
            }

            if (preferredWidth >= 0f)
            {
                layout.preferredWidth = preferredWidth;
            }

            if (preferredHeight >= 0f)
            {
                layout.preferredHeight = preferredHeight;
            }

            layout.flexibleHeight = flexibleHeight ? 1f : 0f;
            layout.flexibleWidth = flexibleWidth;
        }

        private static void AddShadow(GameObject go, Color shadowColor)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 0.55f);
            shadow.effectDistance = new Vector2(4f, -4f);
        }

        private static TMP_FontAsset LoadFont()
        {
            if (TMP_Settings.defaultFontAsset != null)
            {
                return TMP_Settings.defaultFontAsset;
            }

            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        }

        private static Sprite LoadSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parts = assetPath.Split('/');
            var current = parts[0];

            for (var index = 1; index < parts.Length; index++)
            {
                var next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private static string GetParentFolder(string assetPath)
        {
            var separatorIndex = assetPath.LastIndexOf('/');
            return separatorIndex <= 0 ? "Assets" : assetPath.Substring(0, separatorIndex);
        }

        private static bool PathExists(string assetPath)
        {
            return AssetDatabase.IsValidFolder(assetPath) || AssetDatabase.LoadMainAssetAtPath(assetPath) != null;
        }

        private static void SetObjectField(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectField(Object target, string fieldName, string value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFieldValue<T>(Object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
            EditorUtility.SetDirty(target);
        }
    }
}
