using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Leaf : MonoBehaviour
{
    [SerializeField] private int   _value     = 1;
    [SerializeField] private float _bobSpeed  = 2.5f;
    [SerializeField] private float _bobHeight = 0.12f;

    private Vector3    _startPos;
    private HUDManager _hud;

    private void Start()
    {
        _startPos = transform.position;
        GetComponent<CircleCollider2D>().isTrigger = true;
        _hud = Object.FindFirstObjectByType<HUDManager>();
    }

    private void Update()
    {
        transform.position = _startPos + Vector3.up * (Mathf.Sin(Time.time * _bobSpeed) * _bobHeight);
        transform.Rotate(0, 0, 70f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        if (_hud != null) _hud.AddLeaves(_value);
        Destroy(gameObject);
    }
}
