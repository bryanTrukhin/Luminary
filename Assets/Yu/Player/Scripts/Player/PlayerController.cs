using UnityEngine;

public class PlayerController : MonoBehaviour, IHidable
{
    #if UNITY_EDITOR
    private Color _groundGizmoColor = Color.red;
    #endif

    [Header("Ability Energy Costs")]
    [SerializeField] private float dashEnergyCost = 20f;
    [SerializeField] private float doubleJumpEnergyCost = 15f;

    [Header("Movement Settings")]
    [SerializeField] public float speed = 8f;
    [SerializeField] public float sprintSpeed = 12f;
    public bool sprintEnabled = true;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private int extraJumpCount = 1;
    [SerializeField] private GameObject jumpEffect;

    [Header("Wall Mechanics")]
    public Vector2 grabRightOffset = new Vector2(0.16f, 0f);
    public Vector2 grabLeftOffset = new Vector2(-0.16f, 0f);
    public float grabCheckRadius = 0.24f;
    public float slideSpeed = 2.5f;
    public Vector2 wallJumpForce = new Vector2(10.5f, 18f);
    public Vector2 wallClimbForce = new Vector2(4f, 14f);

    [Header("Dashing")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float startDashTime = 0.1f;
    [SerializeField] private float dashCooldown = 0.2f;
    [SerializeField] private GameObject dashEffect;

    [Header("Crouch / Slide")]
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float crouch_slideSpeed = 9f;
    [SerializeField] float slideDuration = 0.4f;
    
    // Logic States
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public float moveInput;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool actuallyWallGrabbing = false;
    [HideInInspector] public bool isSprinting = false;
    [HideInInspector] public bool isDashing = false;
    [HideInInspector] public bool isCrouching = false;
    [HideInInspector] public bool isSliding = false;

    // Internal References
    private Rigidbody2D m_rb;
    private CapsuleCollider2D m_col;
    private SpriteRenderer[] _spriteRenderers;
    private ParticleSystem m_dustParticle;
    private ILantern _lantern;
    public ILantern Lantern => _lantern;

    // Internal Calculations
    private bool _prevGrounded;
    private bool m_facingRight = true;
    private float m_groundedRemember = 0f;
    private readonly float m_groundedRememberTime = 0.25f;
    private int m_extraJumps;
    private float m_dashTime;
    private float m_dashCooldown;
    private bool m_hasDashedInAir = false;
    private bool m_onWall, m_onRightWall, m_onLeftWall, m_wallGrabbing, m_wallJumping;
    private float m_wallStick, slideTimer;
    private readonly float m_wallStickTime = 0.25f;
    private int m_onWallSide = 0;
    private int m_playerSide = 1;

    [SerializeField] Vector2 standColliderSize = new(.5f, 1.5f);
    [SerializeField] Vector2 crouchColliderSize = new(.5f, .75f);

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_col = GetComponent<CapsuleCollider2D>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        m_dustParticle = GetComponentInChildren<ParticleSystem>();
        _lantern = GetComponentInChildren<ILantern>();
    }

    void Start()
    {
        PoolManager.instance.CreatePool(dashEffect, 2);
        PoolManager.instance.CreatePool(jumpEffect, 2);

        m_extraJumps = extraJumpCount;
        m_dashTime = startDashTime;
        m_dashCooldown = dashCooldown;
    }

    private void FixedUpdate()
    {
        // 1. Environmental Checks
        _prevGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        
        var pos = (Vector2)transform.position;
        m_onRightWall = Physics2D.OverlapCircle(pos + grabRightOffset, grabCheckRadius, whatIsGround);
        m_onLeftWall = Physics2D.OverlapCircle(pos + grabLeftOffset, grabCheckRadius, whatIsGround);
        m_onWall = m_onRightWall || m_onLeftWall;

        CalculateSides();

        if ((m_wallGrabbing || isGrounded) && m_wallJumping) m_wallJumping = false;

        // 2. State Guard
        bool shouldProcess = GameController.I == null || GameController.I.State == PlayState.Exploring;
        if (!shouldProcess) return;

        // 3. Movement Physics
        float run = isSprinting ? sprintSpeed : speed;
        if (isCrouching) run = crouchSpeed;
        if (isSliding) run = crouch_slideSpeed;

        if (m_wallJumping)
        {
            m_rb.velocity = Vector2.Lerp(m_rb.velocity, (new Vector2(moveInput * speed, m_rb.velocity.y)), 1.5f * Time.fixedDeltaTime);
        }
        else
        {
            if (canMove && !m_wallGrabbing && !isSliding)
                m_rb.velocity = new Vector2(moveInput * run, m_rb.velocity.y);
            else if (!canMove)
                m_rb.velocity = new Vector2(0f, m_rb.velocity.y);
        }

        // 4. Gravity Modifiers
        if (m_rb.velocity.y < 0f)
            m_rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;

        // 5. Dashing Logic
        if (isDashing)
        {
            m_dashTime -= Time.fixedDeltaTime;
            m_rb.velocity = (m_facingRight ? Vector2.right : Vector2.left) * dashSpeed;

            if (m_dashTime <= 0f)
            {
                isDashing = false;
                m_dashTime = startDashTime;
                m_rb.velocity = Vector2.zero;
            }
        }

        // 6. Wall Grab Logic
        if (m_onWall && !isGrounded && m_rb.velocity.y <= 0f && m_playerSide == m_onWallSide)
        {
            actuallyWallGrabbing = true;
            m_wallGrabbing = true;
            m_rb.velocity = new Vector2(moveInput * speed, -slideSpeed);
            m_wallStick = m_wallStickTime;
        }
        else
        {
            m_wallStick -= Time.fixedDeltaTime;
            actuallyWallGrabbing = false;
            if (m_wallStick <= 0f) m_wallGrabbing = false;
        }

        // 7. Sliding Timer
        if (isSliding)
        {
            slideTimer -= Time.fixedDeltaTime;
            if (slideTimer <= 0)
            {
                isSliding = false;
                isCrouching = InputSystem.CrouchHeld();
                ResizeColliderHeight(isCrouching ? crouchColliderSize.y : standColliderSize.y);
            }
        }

        // 8. Visuals
        HandleVisuals();
    }

    private void Update()
    {
        moveInput = InputSystem.HorizontalRaw();
        
        if (isGrounded)
        {
            m_extraJumps = extraJumpCount;
            m_hasDashedInAir = false;
            m_groundedRemember = m_groundedRememberTime;
        }
        m_groundedRemember -= Time.deltaTime;

        // Guard input if not playable
        if (GameController.I != null && GameController.I.State != PlayState.Exploring) return;

        isSprinting = sprintEnabled;

        // Dash Input
        if (InputSystem.Dash() && !isDashing && !m_hasDashedInAir && m_dashCooldown <= 0f)
        {
            if (_lantern == null || _lantern.TryConsumeEnergy(dashEnergyCost))
            {
                isDashing = true;
                m_dashCooldown = dashCooldown;
                PoolManager.instance.ReuseObject(dashEffect, transform.position, Quaternion.identity);
                if (!isGrounded) m_hasDashedInAir = true;
            }
        }
        m_dashCooldown -= Time.deltaTime;

        // Jump Input
        if (InputSystem.Jump())
        {
            if (isGrounded || m_groundedRemember > 0f) // Normal Jump
            {
                m_rb.velocity = new Vector2(m_rb.velocity.x, jumpForce);
                PoolManager.instance.ReuseObject(jumpEffect, groundCheck.position, Quaternion.identity);
            }
            else if (m_extraJumps > 0 && !m_wallGrabbing) // Double Jump
            {
                if (_lantern == null || _lantern.TryConsumeEnergy(doubleJumpEnergyCost))
                {
                    m_rb.velocity = new Vector2(m_rb.velocity.x, jumpForce * 0.8f);
                    m_extraJumps--;
                    PoolManager.instance.ReuseObject(jumpEffect, groundCheck.position, Quaternion.identity);
                }
            }
            else if (m_wallGrabbing) // Wall Jumps
            {
                m_wallGrabbing = false;
                m_wallJumping = true;
                if (m_playerSide == m_onWallSide) Flip();
                
                Vector2 force = (moveInput != m_onWallSide) ? wallJumpForce : wallClimbForce;
                m_rb.velocity = Vector2.zero; // Reset before punch
                m_rb.AddForce(new Vector2(-m_onWallSide * force.x, force.y), ForceMode2D.Impulse);
            }
        }

        // Slide/Crouch Input
        if (InputSystem.SlidePressed() && isGrounded && !isSliding && isSprinting && Mathf.Abs(moveInput) > 0.1f)
        {
            isSliding = true;
            isCrouching = false;
            slideTimer = slideDuration;
            m_rb.velocity = new Vector2((m_facingRight ? 1f : -1f) * crouch_slideSpeed, 0f);
        }

        if (!isSliding || InputSystem.CrouchHeld())
        {
            isCrouching = InputSystem.CrouchHeld();
            if (!isCrouching && HeadClear()) ResizeColliderHeight(standColliderSize.y);
            else if (isCrouching) ResizeColliderHeight(crouchColliderSize.y);
        }

        // Lantern Flash
        if (InputSystem.FlashLantern() && _lantern != null)
        {
            _lantern.TriggerFlash();
        }

        // Flip Logic
        if ((!m_facingRight && moveInput > 0f) || (m_facingRight && moveInput < 0f)) Flip();
    }

    void HandleVisuals()
    {
        float vel = m_rb.velocity.sqrMagnitude;
        if (m_dustParticle.isPlaying && vel == 0f) m_dustParticle.Stop();
        else if (!m_dustParticle.isPlaying && vel > 0.1f) m_dustParticle.Play();
    }

    void Flip()
    {
        m_facingRight = !m_facingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void CalculateSides()
    {
        m_onWallSide = m_onRightWall ? 1 : (m_onLeftWall ? -1 : 0);
        m_playerSide = m_facingRight ? 1 : -1;
    }

    bool HeadClear()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.up * crouchColliderSize.y * .5f;
        return !Physics2D.BoxCast(origin, standColliderSize, 0f, Vector2.up, (standColliderSize.y - crouchColliderSize.y) + 0.05f, whatIsGround);
    }


    public void EnterHiding(Vector3 hidePos)
    {
        // 1. Stop Movement
        SetMoveable(false);
        m_rb.simulated = false; // Completely disable physics (collisions/gravity)

        // 2. Move to position
        transform.position = hidePos;

        // 3. Visuals (Hide Sprite + Lantern)
        SetVisible(false);
    }

    public void ExitHiding(Vector3 exitPos)
    {
        // 1. Reset Position
        transform.position = exitPos;

        // 2. Re-enable Physics/Movement
        m_rb.simulated = true;
        SetMoveable(true);

        // 3. Visuals
        SetVisible(true);
    }

    public void SetMoveable(bool canMove)
    {
        
        if (!canMove)
        {
            m_rb.velocity = Vector2.zero;
            m_rb.bodyType = RigidbodyType2D.Kinematic; 
            this.canMove = false; // Assuming you use this bool in Update()
        }
        else
        {
            m_rb.bodyType = RigidbodyType2D.Dynamic;
            this.canMove = true;
        }
    }

    public void SetVisible(bool visible)
    {
        if (_spriteRenderers != null)
            foreach (var sr in _spriteRenderers) sr.enabled = visible;
        
        // Use the interface accessor for the lantern
        _lantern?.SetVisible(visible);
    }

    void ResizeColliderHeight(float newHeight)
    {
        float oldHeight = m_col.size.y;
        if (Mathf.Approximately(oldHeight, newHeight)) return;

        m_col.size = new Vector2(m_col.size.x, newHeight);
        m_col.offset = new Vector2(m_col.offset.x, m_col.offset.y - (oldHeight - newHeight) * 0.5f);
    }
}