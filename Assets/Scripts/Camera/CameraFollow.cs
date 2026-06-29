using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float     _smoothTime = 0.2f;
    [SerializeField] private Vector3   _offset     = new Vector3(0f, 2f, -18f);

    [Header("2.5D Perspective")]
    [SerializeField] private float _fov      = 55f;   // field of view in degrees
    [SerializeField] private float _tiltX    = 8f;    // downward camera tilt (degrees)

    private Camera  _cam;
    private Vector3 _velocity = Vector3.zero;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = false;
        _cam.fieldOfView  = _fov;
        transform.rotation = Quaternion.Euler(_tiltX, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desired  = _target.position + _offset;
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime);

        // Lock Z so perspective depth stays constant; rotation stays fixed from Awake.
        transform.position = new Vector3(smoothed.x, smoothed.y, _offset.z);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var cam = GetComponent<Camera>();
        if (cam == null) return;
        cam.orthographic = false;
        cam.fieldOfView  = _fov;
        transform.rotation = Quaternion.Euler(_tiltX, 0f, 0f);
    }
#endif
}
