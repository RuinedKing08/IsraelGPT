using UnityEngine;
using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace SergiosAdventure.GraphToolkit.Editor
{
    public class SituationNode : Node
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("SituationId")
                .WithDisplayName("Situation Id")
                .WithDefaultValue("situation")
                .Delayed();

            context.AddOption<string>("Title")
                .WithDisplayName("Title")
                .WithDefaultValue("New Situation")
                .Delayed();

            context.AddOption<string>("StoryText")
                .WithDisplayName("Story Text")
                .WithDefaultValue("Write the situation text here.")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("Previous").Build();
            context.AddOutputPort("Option 1").Build();
            context.AddOutputPort("Option 2").Build();
        }
    }
}
