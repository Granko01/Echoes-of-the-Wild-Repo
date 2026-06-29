using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float     _moveSpeed          = 6f;
    [SerializeField] private float     _jumpForce          = 12f;
    [SerializeField] private float     _climbSpeed         = 4f;

    [Header("Wall Jump")]
    [SerializeField] private Transform _wallCheckLeft;
    [SerializeField] private Transform _wallCheckRight;
    [SerializeField] private float     _wallCheckRadius    = 0.15f;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float     _wallJumpForceX     = 8f;
    [SerializeField] private float     _wallJumpForceY     = 10f;

    [Header("Slide")]
    [SerializeField] private float     _slideDuration      = 0.4f;
    [SerializeField] private float     _slideSpeedMult     = 2f;
    [SerializeField] private float     _slideCooldown      = 0.8f;

    [Header("Dodge")]
    [SerializeField] private float     _dodgeDuration      = 0.3f;
    [SerializeField] private float     _dodgeForce         = 10f;
    [SerializeField] private float     _dodgeCooldown      = 1.0f;

    [Header("Characters")]
    [SerializeField] private CharacterData[] _characters;

    private Rigidbody2D      _rb;
    private PlayerAbilities  _abilities;
    private WeaponHolder     _weaponHolder;
    private PurifyBurst      _purifyBurst;
    private PlayerHealth     _health;

    private Vector2 _moveInput;
    private bool    _isGrounded;
    private bool    _onGround;       // raw contact flag set by physics callbacks
    private bool    _isClimbing;
    private bool    _onVine;
    private bool    _isTouchingWallLeft;
    private bool    _isTouchingWallRight;
    private bool    _isSliding;
    private bool    _isDodging;

    // 0 = on ground, 1 = first jump used, 2 = double jump / wall jump used
    private int   _jumpCount;
    private float _jumpLockTimer;
    private float _jumpDebounceTimer;
    private float _slideTimer;
    private float _slideCooldownTimer;
    private float _dodgeTimer;
    private float _dodgeCooldownTimer;
    private float _comboSpeedMultiplier = 1f;
    private float _facingDir = 1f;

    public bool    IsGrounded => _isGrounded;
    public bool    IsMoving   => Mathf.Abs(_moveInput.x) > 0.05f || Mathf.Abs(_moveInput.y) > 0.05f;
    public Vector2 FacingDir  => new Vector2(_facingDir, 0f);

    private void Awake()
    {
        _rb           = GetComponent<Rigidbody2D>();
        _abilities    = GetComponent<PlayerAbilities>();
        _weaponHolder = GetComponent<WeaponHolder>();
        _purifyBurst  = GetComponent<PurifyBurst>();
        _health       = GetComponent<PlayerHealth>();
        ApplySelectedCharacter();
    }

    private void ApplySelectedCharacter()
    {
        if (_characters == null || _characters.Length == 0) return;
        int idx = PlayerPrefs.GetInt(CharacterSelectManager.PrefKey, 0);
        idx = Mathf.Clamp(idx, 0, _characters.Length - 1);
        CharacterData cd = _characters[idx];
        if (cd == null) return;
        _moveSpeed = cd.moveSpeed;
        _jumpForce = cd.jumpForce;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = cd.characterColor;
    }

    private void Update()
    {
        if (_jumpLockTimer      > 0) _jumpLockTimer      -= Time.deltaTime;
        if (_jumpDebounceTimer  > 0) _jumpDebounceTimer  -= Time.deltaTime;
        if (_slideCooldownTimer > 0) _slideCooldownTimer -= Time.deltaTime;
        if (_dodgeCooldownTimer > 0) _dodgeCooldownTimer -= Time.deltaTime;

        // Ground state: derived from physics collision callbacks; blocked during jump debounce.
        bool wasGrounded = _isGrounded;
        _isGrounded = _onGround && _jumpDebounceTimer <= 0;
        if (_isGrounded && !wasGrounded)
        {
            _jumpCount = 0;
            SoundDetector.Instance?.RegisterSource(
                new SoundSource(transform.position, 1f, isPlayer: true));
        }

        // Wall detection
        _isTouchingWallLeft  = _wallCheckLeft  != null &&
            Physics2D.OverlapCircle(_wallCheckLeft.position,  _wallCheckRadius, _wallLayer);
        _isTouchingWallRight = _wallCheckRight != null &&
            Physics2D.OverlapCircle(_wallCheckRight.position, _wallCheckRadius, _wallLayer);

        // Update facing — scale-flip so child objects (weapon) also face the right way
        if (_moveInput.x > 0.05f)       _facingDir =  1f;
        else if (_moveInput.x < -0.05f) _facingDir = -1f;
        var s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x) * _facingDir, s.y, s.z);

        // Slide timer
        if (_isSliding)
        {
            _slideTimer -= Time.deltaTime;
            if (_slideTimer <= 0f) EndSlide();
        }
    }

    private void FixedUpdate()
    {
        // Reset each physics step; OnCollisionStay2D re-sets it if contact is maintained.
        _onGround = false;

        if (_isDodging || _isSliding) return;

        if (_isClimbing || _onVine)
        {
            _rb.linearVelocity = _moveInput * _climbSpeed;
            _rb.gravityScale   = 0f;
        }
        else
        {
            float speed = _moveSpeed * _comboSpeedMultiplier;
            _rb.linearVelocity = new Vector2(_moveInput.x * speed, _rb.linearVelocity.y);
            _rb.gravityScale   = 3f;
        }
    }

    // ── Ground contact detection ───────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D col) => EvaluateGroundContact(col);
    private void OnCollisionStay2D(Collision2D col)  => EvaluateGroundContact(col);

    private void EvaluateGroundContact(Collision2D col)
    {
        for (int i = 0; i < col.contactCount; i++)
        {
            // Contact normal pointing up (> 45°) means we're standing on a surface.
            if (col.GetContact(i).normal.y > 0.7f)
            {
                _onGround = true;
                return;
            }
        }
    }

    // ── Input System callbacks ─────────────────────────────────────────────────

    public void OnMove(InputValue value)
        => _moveInput = value.Get<Vector2>();

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;

        bool touchingWall = _isTouchingWallLeft || _isTouchingWallRight;

        if (_isGrounded && _jumpCount == 0 && _jumpLockTimer <= 0)
        {
            _jumpCount         = 1;
            _jumpLockTimer     = 0.15f;
            _jumpDebounceTimer = 0.25f;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        }
        else if (_onVine)
        {
            ExitVine();
        }
        else if (!_isGrounded && touchingWall && _jumpCount < 2)
        {
            // Wall jump — kick away from wall
            float kickDir      = _isTouchingWallRight ? -1f : 1f;
            _jumpCount         = 2;
            _jumpLockTimer     = 0.2f;
            _jumpDebounceTimer = 0.25f;
            _rb.linearVelocity = new Vector2(kickDir * _wallJumpForceX, _wallJumpForceY);

            SoundDetector.Instance?.RegisterSource(
                new SoundSource(transform.position, 1.5f, isPlayer: true));
        }
        else if (!_isGrounded && _jumpCount == 1 && _jumpLockTimer <= 0)
        {
            // Double jump — one per air time, gated solely by _jumpCount
            _jumpCount         = 2;
            _jumpLockTimer     = 0.15f;
            _jumpDebounceTimer = 0.25f;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        }
    }

    public void OnSlide(InputValue value)
    {
        if (!value.isPressed || !_isGrounded || _isSliding || _slideCooldownTimer > 0) return;
        if (Mathf.Abs(_moveInput.x) < 0.1f) return;
        StartSlide();
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed) _weaponHolder?.OnAttack();
    }

    public void OnDodge(InputValue value)
    {
        if (value.isPressed && !_isDodging && _dodgeCooldownTimer <= 0)
            StartCoroutine(DodgeRoutine());
    }

    public void OnWeaponSkill(InputValue value)
    {
        if (value.isPressed) _weaponHolder?.OnWeaponSkill();
    }

    public void OnPurifyBurst(InputValue value)
    {
        if (value.isPressed) _purifyBurst?.TryActivate();
    }

    // ── Traversal state (called by trigger zones) ──────────────────────────────

    public void EnterClimb() => _isClimbing = true;
    public void ExitClimb()  { _isClimbing = false; _rb.gravityScale = 3f; }
    public void EnterVine()  => _onVine = true;
    public void ExitVine()   { _onVine = false; _rb.gravityScale = 3f; }

    // ── Combo speed bonus (set by ComboMeter) ─────────────────────────────────

    public void SetComboSpeedBonus(float multiplier) => _comboSpeedMultiplier = multiplier;

    // ── Slide ─────────────────────────────────────────────────────────────────

    private void StartSlide()
    {
        _isSliding  = true;
        _slideTimer = _slideDuration;
        _slideCooldownTimer = _slideCooldown;

        float speed = _moveSpeed * _slideSpeedMult * _facingDir;
        _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
    }

    private void EndSlide()
    {
        _isSliding = false;
    }

    // ── Dodge ─────────────────────────────────────────────────────────────────

    private IEnumerator DodgeRoutine()
    {
        _isDodging          = true;
        _dodgeCooldownTimer = _dodgeCooldown;
        if (_health != null) _health.SetInvincible(true);

        float dir = _moveInput.x != 0f ? Mathf.Sign(_moveInput.x) : _facingDir;
        _rb.linearVelocity = new Vector2(dir * _dodgeForce, _rb.linearVelocity.y);

        yield return new WaitForSeconds(_dodgeDuration);

        _isDodging = false;
        if (_health != null) _health.SetInvincible(false);
    }
}
