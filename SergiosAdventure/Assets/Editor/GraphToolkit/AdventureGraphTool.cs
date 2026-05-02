using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace SergiosAdventure.GraphToolkit.Editor
{
    [Graph(AssetExtension, GraphOptions.SupportsSubgraphs)]
    [Serializable]
    sealed class AdventureGraphTool : Graph
    {
        public const string AssetExtension = "adventuregraph";

        [MenuItem("Assets/Create/Adventure/Graph Toolkit Adventure Graph", false)]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<AdventureGraphTool>();
        }
    }

    [Serializable]
    abstract class AdventureSituationNode : Node
    {
        const string SituationIdOption = "SituationId";
        const string TitleOption = "Title";
        const string BodyOption = "Body";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(SituationIdOption)
                .WithDisplayName("Situation Id")
                .WithDefaultValue("situation")
                .Delayed();

            context.AddOption<string>(TitleOption)
                .WithDisplayName("Title")
                .WithDefaultValue("Situation")
                .Delayed();

            context.AddOption<string>(BodyOption)
                .WithDisplayName("Story Text")
                .WithDefaultValue("Story text goes here.")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("Previous").Build();
            context.AddOutputPort("Choice A").Build();
            context.AddOutputPort("Choice B").Build();
        }
    }

    [Serializable]
    sealed class StartSituationNode : Node
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("StartSituationId")
                .WithDisplayName("Start Situation Id")
                .WithDefaultValue("start")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort("First Situation").Build();
        }
    }

    [Serializable]
    sealed class TextSituationNode : AdventureSituationNode
    {
    }

    [Serializable]
    sealed class EventSituationNode : AdventureSituationNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<string>("FlagToSet")
                .WithDisplayName("Flag To Set")
                .WithDefaultValue("")
                .Delayed();

            context.AddOption<string>("GrantedItemId")
                .WithDisplayName("Granted Item Id")
                .WithDefaultValue("")
                .Delayed();

            context.AddOption<int>("HealthDelta")
                .WithDisplayName("Health Delta")
                .WithDefaultValue(0)
                .Delayed();

            context.AddOption<int>("DamageDelta")
                .WithDisplayName("Damage Delta")
                .WithDefaultValue(0)
                .Delayed();
        }
    }

    [Serializable]
    sealed class CombatSituationNode : AdventureSituationNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<string>("EnemyId")
                .WithDisplayName("Enemy Id")
                .WithDefaultValue("enemy")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("Previous").Build();
            context.AddOutputPort("Victory").Build();
            context.AddOutputPort("Defeat").Build();
        }
    }

    [Serializable]
    sealed class EndingSituationNode : Node
    {
        enum EndingKind
        {
            Good,
            Neutral,
            Bad
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("SituationId")
                .WithDisplayName("Situation Id")
                .WithDefaultValue("ending")
                .Delayed();

            context.AddOption<EndingKind>("Ending")
                .WithDisplayName("Ending");

            context.AddOption<string>("Body")
                .WithDisplayName("Ending Text")
                .WithDefaultValue("The End")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("Previous").Build();
        }
    }
}
