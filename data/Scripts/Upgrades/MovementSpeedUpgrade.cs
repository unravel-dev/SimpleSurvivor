using Unravel.Core;

/// <summary>
/// Upgrade that increases the player's movement speed.
/// Improves mobility and ability to dodge attacks.
/// </summary>
public class MovementSpeedUpgrade : Upgrade
{
    /// <summary>
    /// Percentage movement speed increase this upgrade provides.
    /// </summary>
    public float SpeedPercent { get; private set; }
    
    /// <summary>
    /// Create a new movement speed upgrade with specified parameters.
    /// </summary>
    /// <param name="speedPercent">Percentage movement speed increase (e.g., 15 for 15%).</param>
    public MovementSpeedUpgrade(float speedPercent = 15.0f) 
        : base("Speed Boost", $"Increases movement speed by {speedPercent:F1}%")
    {
        SpeedPercent = speedPercent;
    }
    
    /// <summary>
    /// Generate a new MovementSpeedUpgrade with random values based on rarity.
    /// </summary>
    /// <param name="rarity">The rarity level determining the value ranges.</param>
    /// <returns>A new MovementSpeedUpgrade with randomized values.</returns>
    public static MovementSpeedUpgrade Generate(UpgradeRarity rarity)
    {
        float speedPercent;
        
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                speedPercent = Random.Range(8.0f, 15.0f);
                break;
            case UpgradeRarity.Common:
                speedPercent = Random.Range(12.0f, 20.0f);
                break;
            case UpgradeRarity.Epic:
                speedPercent = Random.Range(18.0f, 28.0f);
                break;
            case UpgradeRarity.Legendary:
                speedPercent = Random.Range(25.0f, 40.0f);
                break;
            default:
                speedPercent = 15.0f;
                break;
        }
        
        return new MovementSpeedUpgrade(speedPercent);
    }

}
