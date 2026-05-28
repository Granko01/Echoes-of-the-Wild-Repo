using System.Collections;
using UnityEngine;

// Added to the Baby Deer mini-boss GameObject. MiniBossController calls ActivateCompanionMode()
// on defeat — the deer then follows the player and can trigger secret reveals in BiomeAreas.
public class DeerCompanion : MonoBehaviour
{
    [SerializeField] private float _followSpeed    = 3f;
    [SerializeField] private float _followDistance = 2f;  // stay this far behind the player

    public bool IsActive { get; private set; }

    private Transform   _player;
    private Rigidbody2D _rb;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void ActivateCompanionMode()
    {
        IsActive = true;
        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) _player = pc.transform;
        StartCoroutine(FollowPlayer());
    }

    // Called by BiomeArea or secret trigger zones to check if the companion is present
    public bool CanRevealSecrets() => IsActive;

    private IEnumerator FollowPlayer()
    {
        while (IsActive)
        {
            if (_player == null) { yield return null; continue; }

            float dist = Vector2.Distance(transform.position, _player.position);
            if (dist > _followDistance)
            {
                Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                _rb.linearVelocity = dir * _followSpeed;
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
            }

            yield return null;
        }
    }
}
