using UnityEngine;
using Unity.GraphToolkit.Editor;
using System;
using UnityEditor;

namespace SergiosAdventure.GraphToolkit.Editor
{
    public class EndingNode : Node
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("EndingName")
                .WithDisplayName("Ending Name")
                .WithDefaultValue("Neutral Ending")
                .Delayed();

            context.AddOption<string>("EndingText")
                .WithDisplayName("Ending Text")
                .WithDefaultValue("The story ends here.")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("Previous").Build();
        }
    }
}
