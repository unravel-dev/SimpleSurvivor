using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Lightning Bolt specific upgrades.
/// </summary>
[ScriptSourceFile]
public class LightningSplitUpgrade : Upgrade
{
    public int SplitCount { get; set; }
    public float SplitRange { get; set; }

    public LightningSplitUpgrade(int splitCount = 2, float splitRange = 10.0f)
        : base("Lightning Split", $"Lightning Bolt splits into {splitCount} projectiles on hit")
    {
        SplitCount = splitCount;
        SplitRange = splitRange;
    }

    /// <summary>
    /// Generate a new LightningSplitUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minSplitCount">Minimum split count.</param>
    /// <param name="maxSplitCount">Maximum split count.</param>
    /// <param name="minSplitRange">Minimum split range.</param>
    /// <param name="maxSplitRange">Maximum split range.</param>
    /// <returns>A new LightningSplitUpgrade with random values from the ranges.</returns>
    public static LightningSplitUpgrade Generate(int minSplitCount = 2, int maxSplitCount = 3, float minSplitRange = 8.0f, float maxSplitRange = 12.0f)
    {
        int splitCount = Random.Range(minSplitCount, maxSplitCount + 1);
        float splitRange = Random.Range(minSplitRange, maxSplitRange);
        return new LightningSplitUpgrade(splitCount, splitRange);
    }
}

