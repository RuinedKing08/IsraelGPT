using Unity.GraphToolkit.Editor;
using System;

namespace SergiosAdventure.GraphToolkit.Editor
{
    [Serializable]
    public class EndingNode : Node
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("SituationId")
                .WithDisplayName("Situation Id")
                .WithDefaultValue("ending")
                .Delayed();

            context.AddOption<string>("EndingId")
                .WithDisplayName("Ending Id")
                .WithDefaultValue("neutral_ending")
                .Delayed();

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
