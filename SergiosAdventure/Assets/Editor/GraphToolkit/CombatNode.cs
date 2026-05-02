using UnityEngine;
using Unity.GraphToolkit.Editor;
using System;
using UnityEditor;

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

            context.AddOption<string>("EnemyName")
                .WithDisplayName("Enemy Name")
                .WithDefaultValue("Enemy")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("Previous").Build();
            context.AddOutputPort("Win").Build();
            context.AddOutputPort("Lose").Build();
        }
    }

}
