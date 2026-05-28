using UnityEngine;

// A transient sound event in the world. Created by player movement, weapon impacts,
// Echo Clones, and crystal activations. CaveMaw's AI uses SoundDetector to find these.
public class SoundSource
{
    public Vector2 Position  { get; }
    public float   Lifetime  { get; private set; }
    public bool    IsPlayer  { get; }   // true = came from actual player movement
    public bool    IsExpired => Lifetime <= 0f;

    public SoundSource(Vector2 position, float lifetime, bool isPlayer)
    {
        Position = position;
        Lifetime = lifetime;
        IsPlayer = isPlayer;
    }

    public void Tick(float deltaTime) => Lifetime -= deltaTime;
}
