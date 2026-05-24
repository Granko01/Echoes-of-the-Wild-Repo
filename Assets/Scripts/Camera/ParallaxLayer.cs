using UnityEngine;

[DefaultExecutionOrder(10)]
public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax")]
    [SerializeField] [Range(0f, 1f)] private float _parallaxFactorX = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float _parallaxFactorY = 0f;

    [Header("Menu Auto-Scroll")]
    [SerializeField] private Vector2 _autoScrollSpeed = Vector2.zero;

    private Camera  _cam;
    private Vector3 _startPos;
    private Vector3 _startCamPos;
    private Vector2 _scrollAccum;

    private void Start()
    {
        _cam         = Camera.main;
        _startPos    = transform.position;
        _startCamPos = _cam != null ? _cam.transform.position : Vector3.zero;
    }

    private void LateUpdate()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        float cx = (_cam.transform.position.x - _startCamPos.x) * _parallaxFactorX;
        float cy = (_cam.transform.position.y - _startCamPos.y) * _parallaxFactorY;

        _scrollAccum += _autoScrollSpeed * Time.deltaTime;

        transform.position = new Vector3(
            _startPos.x + cx + _scrollAccum.x,
            _startPos.y + cy + _scrollAccum.y,
            _startPos.z
        );
    }
}
