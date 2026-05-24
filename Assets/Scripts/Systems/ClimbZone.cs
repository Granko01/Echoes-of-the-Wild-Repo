using UnityEngine;

// Trigger zone that enables wall-climb or vine-swing on the PlayerController.
[RequireComponent(typeof(Collider2D))]
public class ClimbZone : MonoBehaviour
{
    public enum ZoneType { WallClimb, VineSwing }

    [SerializeField] private ZoneType _type = ZoneType.WallClimb;

    private void Awake() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController>(out var player)) return;
        if (_type == ZoneType.WallClimb) player.EnterClimb();
        else                             player.EnterVine();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController>(out var player)) return;
        if (_type == ZoneType.WallClimb) player.ExitClimb();
        else                             player.ExitVine();
    }
}
