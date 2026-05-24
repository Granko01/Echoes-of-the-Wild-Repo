using UnityEngine;

// Attach to any platform that has a kinematic Rigidbody2D.
// The platform oscillates between two positions driven by a sine wave.
[RequireComponent(typeof(Rigidbody2D))]
public class PlatformMover : MonoBehaviour
{
    [SerializeField] private float _distance = 3f;
    [SerializeField] private float _speed    = 1.2f;
    [SerializeField] private bool  _vertical = false;

    private Rigidbody2D _rb;
    private Vector3     _origin;

    private void Start()
    {
        _rb                  = GetComponent<Rigidbody2D>();
        _rb.isKinematic       = true;
        _rb.interpolation     = RigidbodyInterpolation2D.Interpolate;
        _rb.freezeRotation    = true;
        _origin              = transform.position;
    }

    private void FixedUpdate()
    {
        float   offset = Mathf.Sin(Time.fixedTime * _speed * Mathf.PI) * _distance;
        Vector3 newPos = _origin + (_vertical ? Vector3.up : Vector3.right) * offset;
        _rb.MovePosition(newPos);
    }
}
