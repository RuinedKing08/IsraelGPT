using Unity.GraphToolkit.Editor;
using System;

namespace SergiosAdventure.GraphToolkit.Editor
{
    public class CombatNode : Node
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("SituationId")
                .WithDisplayName("Situation Id")
                .WithDefaultValue("combat")
                .Delayed();

            context.AddOption<string>("EnemyId")
                .WithDisplayName("Enemy Id")
                .WithDefaultValue("DebugWolf")
                .Delayed();

            context.AddOption<string>("VictoryNodeId")
                .WithDisplayName("Victory Node Id")
                .WithDefaultValue("")
                .Delayed();

            context.AddOption<string>("DefeatNodeId")
                .WithDisplayName("Defeat Node Id")
                .WithDefaultValue("")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("Previous").Build();
            context.AddOutputPort("Victory").Build();
            context.AddOutputPort("Defeat").Build();
        }
    }
}
