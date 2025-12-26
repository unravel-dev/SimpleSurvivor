/// <summary>
/// Defines the possible states for an enemy entity.
/// Used by EnemyStateMachine to manage enemy behavior states.
/// </summary>
public enum EnemyState
{
    Idle,       // Enemy is stationary, not moving
    Walking,    // Enemy is moving at normal speed
    Running,    // Enemy is moving at high speed (future use)
    Stunned,    // Enemy is stunned and cannot act
    Attacking,  // Enemy is performing an attack (future use)
    Dying,      // Enemy is in death animation sequence
    Dead        // Enemy is dead (final state)
}
