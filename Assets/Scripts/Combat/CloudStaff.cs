using System.Collections;
using UnityEngine;

// Chapter 4 weapon A — Cloud Staff
// Base  : Dash strike
// Lv2   : Cloud platform (temporary platform under player)
// Lv3   : Wind launch (upward burst + extended air time)
// MAX   : Storm movement (rapid multi-dash)
public class CloudStaff : WeaponBase
{
    [Header("Dash Strike — Base")]
    [SerializeField] private float _dashDistance = 3f;
    [SerializeField] private float _dashDuration = 0.2f;

    [Header("Wind Launch — Lv3")]
    [SerializeField] private float _launchForce = 20f;

    [Header("Storm Movement — MAX")]
    [SerializeField] private int   _stormDashCount    = 4;
    [SerializeField] private float _stormDashInterval = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip _dashClip;
    [SerializeField] private AudioClip _launchClip;

    private Rigidbody2D _playerRb;
    private void Start() => _playerRb = GetComponentInParent<Rigidbody2D>();

    protected override void Attack(Vector2 direction)
    {
        AudioManager.Instance?.PlaySFX(_dashClip);
        if (Level >= 3)
            StartCoroutine(StormDashRoutine(direction));
        else
            StartCoroutine(DashStrikeRoutine(direction));
    }

    protected override void WeaponSkill(Vector2 direction)
    {
        switch (Level)
        {
            case 1: break; // no Lv2 skill defined yet (cloud platform — requires platform prefab)
            case 2: WindLaunch(); break;
            case 3: StartCoroutine(StormDashRoutine(direction)); break;
        }
    }

    private IEnumerator DashStrikeRoutine(Vector2 direction)
    {
        if (_playerRb == null) yield break;
        float elapsed = 0f;
        float speed   = _dashDistance / _dashDuration;
        while (elapsed < _dashDuration)
        {
            _playerRb.linearVelocity = new Vector2(direction.x * speed, _playerRb.linearVelocity.y);
            DealAoeDamage(transform.position, 0.8f);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void WindLaunch()
    {
        AudioManager.Instance?.PlaySFX(_launchClip);
        if (_playerRb != null)
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, _launchForce);
    }

    private IEnumerator StormDashRoutine(Vector2 direction)
    {
        for (int i = 0; i < _stormDashCount; i++)
        {
            AudioManager.Instance?.PlaySFX(_dashClip);
            yield return StartCoroutine(DashStrikeRoutine(direction));
            yield return new WaitForSeconds(_stormDashInterval);
        }
    }
}
