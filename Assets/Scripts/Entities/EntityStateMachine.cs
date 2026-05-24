using UnityEngine;

[RequireComponent(typeof(EntityController))]
public class EntityStateMachine : MonoBehaviour
{
    [SerializeField] private EntityState _startState = EntityState.Distressed;

    public EntityState Current { get; private set; }

    private EntityController _controller;
    private SpriteRenderer   _sprite;

    private void Awake()
    {
        _controller = GetComponent<EntityController>();
        _sprite     = GetComponentInChildren<SpriteRenderer>();
        Current     = _startState;
        ApplyVisuals(Current);
    }

    // Pulse / ability hit: move one step toward Stable (decrease value toward 0)
    public void ReduceState(int steps = 1)
    {
        if (Current == EntityState.Empty || Current == EntityState.Stable) return;
        int next = Mathf.Clamp((int)Current - steps, (int)EntityState.Stable, (int)EntityState.Turbulent);
        TransitionTo((EntityState)next);
    }

    public void TransitionTo(EntityState target)
    {
        if (Current == target) return;
        Current = target;
        GameEvents.RaiseStateChange(_controller, Current);
        ApplyVisuals(Current);

        switch (Current)
        {
            case EntityState.Overwhelmed:
                _controller.OnOverwhelmed();
                break;
            case EntityState.Stable:
                _controller.OnStabilized();
                break;
        }
    }

    // Emotional Burst unlocks empty entities for normal healing
    public void ConvertEmptyToReceptive()
    {
        if (Current == EntityState.Empty)
            TransitionTo(EntityState.Receptive);
    }

    private void ApplyVisuals(EntityState state)
    {
        if (_sprite == null) return;
        _sprite.color = state switch
        {
            EntityState.Empty       => new Color(0.50f, 0.50f, 0.50f),
            EntityState.Turbulent   => new Color(0.90f, 0.15f, 0.15f),
            EntityState.Agitated    => new Color(0.90f, 0.45f, 0.10f),
            EntityState.Distressed  => new Color(0.85f, 0.75f, 0.15f),
            EntityState.Overwhelmed => new Color(0.40f, 0.75f, 0.40f),
            EntityState.Receptive   => new Color(0.25f, 0.65f, 0.95f),
            EntityState.Stable      => Color.white,
            _                       => Color.white
        };
    }
}
