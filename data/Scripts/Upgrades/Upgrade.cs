/// <summary>
/// Base class for all upgrades in the game. Abstract class that defines
/// common properties for upgrades. Derived classes define their own specific values.
/// </summary>
public abstract class Upgrade
{
    /// <summary>
    /// Display name of the upgrade.
    /// </summary>
    public string Name { get; protected set; } = "New Upgrade";
    
    /// <summary>
    /// Description of what this upgrade does.
    /// </summary>
    public string Description { get; protected set; } = "Upgrade description";
    
    /// <summary>
    /// Constructor for creating upgrades with basic properties.
    /// </summary>
    /// <param name="name">Display name of the upgrade.</param>
    /// <param name="description">Description of the upgrade effect.</param>
    protected Upgrade(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Get the type of this upgrade as a string.
    /// </summary>
    /// <returns>The C# type name of this upgrade.</returns>
    public string GetUpgradeType()
    {
        return GetType().Name;
    }
}
