using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Lightning Bolt specific upgrades.
/// </summary>
[ScriptSourceFile]
public class LightningSplitUpgrade : Upgrade
{
    public int SplitCount { get; set; }

    public LightningSplitUpgrade(int splitCount = 2)
        : base("Lightning Split", $"Lightning Bolt can split {splitCount} times, creating 2 projectiles each split")
    {
        SplitCount = splitCount;
    }

    /// <summary>
    /// Generate a new LightningSplitUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minSplitCount">Minimum split count.</param>
    /// <param name="maxSplitCount">Maximum split count.</param>
    /// <returns>A new LightningSplitUpgrade with random values from the ranges.</returns>
    public static LightningSplitUpgrade Generate(int minSplitCount = 2, int maxSplitCount = 3)
    {
        int splitCount = Random.Range(minSplitCount, maxSplitCount + 1);
        return new LightningSplitUpgrade(splitCount);
    }
}

