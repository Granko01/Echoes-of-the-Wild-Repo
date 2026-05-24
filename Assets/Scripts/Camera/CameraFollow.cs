using UnityEngine;

[DefaultExecutionOrder(-10)]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float     _smoothTime = 0.2f;
    [SerializeField] private Vector3   _offset     = new Vector3(0f, 1f, -10f);

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (_target == null) return;
        Vector3 desired  = _target.position + _offset;
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime);
        transform.position = new Vector3(smoothed.x, smoothed.y, _offset.z);
    }
}
