using System.Collections;
using UnityEngine;

// Attach to each healable biome zone. Transitions from desaturated → alive
// when all required entities in the zone are stabilized.
[RequireComponent(typeof(AudioSource))]
public class BiomeArea : MonoBehaviour
{
    [SerializeField] private EntityController[] _requiredEntities;
    [SerializeField] private Color _dullColor  = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color _aliveColor = Color.white;
    [SerializeField] private float _transitionDuration = 2f;

    private SpriteRenderer[] _renderers;
    private AudioSource       _ambient;
    private bool              _restored;
    private int               _stabilizedCount;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _ambient   = GetComponent<AudioSource>();
        ApplyColor(_dullColor);
        if (_ambient != null) _ambient.volume = 0f;
    }

    private void OnEnable()  => GameEvents.OnStateChange += OnEntityStateChange;
    private void OnDisable() => GameEvents.OnStateChange -= OnEntityStateChange;

    private void OnEntityStateChange(EntityController entity, EntityState state)
    {
        if (_restored || state != EntityState.Stable) return;
        foreach (var required in _requiredEntities)
        {
            if (required == entity)
            {
                _stabilizedCount++;
                break;
            }
        }
        if (_stabilizedCount >= _requiredEntities.Length)
            Restore();
    }

    private void Restore()
    {
        _restored = true;
        StartCoroutine(TransitionRoutine());
        GameEvents.RaiseHealComplete(this);
    }

    private IEnumerator TransitionRoutine()
    {
        float elapsed = 0f;
        float startVol = _ambient != null ? _ambient.volume : 0f;
        while (elapsed < _transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _transitionDuration;
            ApplyColor(Color.Lerp(_dullColor, _aliveColor, t));
            if (_ambient != null) _ambient.volume = Mathf.Lerp(startVol, 1f, t);
            yield return null;
        }
        ApplyColor(_aliveColor);
    }

    private void ApplyColor(Color c)
    {
        foreach (var r in _renderers) r.color = c;
    }
}
