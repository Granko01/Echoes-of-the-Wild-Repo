using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float      _moveSpeed          = 6f;
    [SerializeField] private float      _jumpForce          = 12f;
    [SerializeField] private float      _climbSpeed         = 4f;
    [SerializeField] private LayerMask  _groundLayer;
    [SerializeField] private Transform  _groundCheck;
    [SerializeField] private float      _groundCheckRadius  = 0.15f;
    [SerializeField] private float      _doubleJumpCooldown = 3f;

    [Header("Characters")]
    [SerializeField] private CharacterData[] _characters;

    private Rigidbody2D     _rb;
    private PlayerAbilities _abilities;
    private Vector2         _moveInput;
    private bool            _isGrounded;
    private bool            _isClimbing;
    private bool            _onVine;

    // 0 = on ground (no jump used), 1 = first jump used, 2 = double jump used
    private int             _jumpCount;
    private float           _jumpLockTimer;
    private float           _doubleJumpTimer;

    private void Awake()
    {
        _rb        = GetComponent<Rigidbody2D>();
        _abilities = GetComponent<PlayerAbilities>();
        ApplySelectedCharacter();
    }

    private void ApplySelectedCharacter()
    {
        if (_characters == null || _characters.Length == 0) return;
        int idx = PlayerPrefs.GetInt(CharacterSelectManager.PrefKey, 0);
        idx = Mathf.Clamp(idx, 0, _characters.Length - 1);
        CharacterData cd = _characters[idx];
        if (cd == null) return;

        _moveSpeed  = cd.moveSpeed;
        _jumpForce  = cd.jumpForce;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = cd.characterColor;
    }

    private void Update()
    {
        if (_jumpLockTimer  > 0) _jumpLockTimer  -= Time.deltaTime;
        if (_doubleJumpTimer > 0) _doubleJumpTimer -= Time.deltaTime;

        if (_groundCheck != null)
        {
            bool wasGrounded = _isGrounded;
            // velocity check prevents false grounded while jumping upward
            bool hit = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
            _isGrounded = hit && _rb.linearVelocity.y <= 0.05f;
            if (_isGrounded && !wasGrounded)
                _jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        if (_isClimbing || _onVine)
        {
            _rb.linearVelocity = _moveInput * _climbSpeed;
            _rb.gravityScale   = 0f;
        }
        else
        {
            _rb.linearVelocity = new Vector2(_moveInput.x * _moveSpeed, _rb.linearVelocity.y);
            _rb.gravityScale   = 3f;
        }
    }

    // ── Input System callbacks ───────────────────────────────────────────────

    public void OnMove(InputValue value)
        => _moveInput = value.Get<Vector2>();

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;

        if (_isGrounded && _jumpCount == 0 && _jumpLockTimer <= 0)
        {
            // First jump — only when standing on ground
            _jumpCount     = 1;
            _jumpLockTimer = 0.15f;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        }
        else if (_onVine)
        {
            ExitVine();
        }
        else if (!_isGrounded && _jumpCount == 1 && _doubleJumpTimer <= 0)
        {
            // Double jump — only after a grounded first jump, with cooldown
            _jumpCount       = 2;
            _doubleJumpTimer = _doubleJumpCooldown;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        }
    }

    public void OnPulse(InputValue value)
    {
        if (value.isPressed) _abilities.UsePulseWave();
    }

    public void OnFocusCalm(InputValue value)
        => _abilities.SetFocusCalm(value.isPressed);

    public void OnEmotionalBurst(InputValue value)
    {
        if (value.isPressed) _abilities.UseEmotionalBurst();
    }

    public void OnSpiritAssist(InputValue value)
    {
        if (value.isPressed) _abilities.UseSpiritAssist();
    }

    // ── Traversal state (called by trigger zones) ────────────────────────────

    public void EnterClimb() => _isClimbing = true;
    public void ExitClimb()  { _isClimbing = false; _rb.gravityScale = 3f; }
    public void EnterVine()  => _onVine = true;
    public void ExitVine()   { _onVine = false; _rb.gravityScale = 3f; }
}
