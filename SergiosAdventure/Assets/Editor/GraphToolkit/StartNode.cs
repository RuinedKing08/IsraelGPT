using UnityEngine;
using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;


namespace SergiosAdventure.GraphToolkit.Editor
{
    public class StartNode : Node
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("StartId")
                .WithDisplayName("Start Situation Id")
                .WithDefaultValue("start")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort("Start").Build();
        }
    }
}
