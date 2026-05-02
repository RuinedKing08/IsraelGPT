using System.Collections.Generic;

public class EndingManager
{
    private List<EndingSO> endings;

    public EndingManager(List<EndingSO> endingsList)
    {
        endings = endingsList;
    }

    public EndingSO GetEnding(GameState state)
    {
        foreach (EndingSO ending in endings)
        {
            bool valid = true;

            foreach (ConditionSO condition in ending.conditions)
            {
                if (!condition.Evaluate(state))
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                return ending;
            }
        }

        return null;
    }
}
