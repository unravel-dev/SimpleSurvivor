/// <summary>
/// Defines the possible types for enemy entities.
/// Used by Enemy component and LootSystem to determine enemy behavior and loot drops.
/// </summary>
public enum EnemyType
{
    Basic,  // Standard enemy type
    Elite,  // Elite enemy with better loot
    Boss,   // Boss enemy with best loot
    Heavy,  // Heavy enemy type
    Fast,   // Fast enemy type
    Weak,   // Weak enemy type
    Small   // Small enemy type
}

