using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IHidable
{
    // Bounce back when hittng collider with enemy tag  
    [Header("Health & Bounds")]
    [SerializeField] private float fallKillHeight = -20f;

    [Header("Movement Settings")]
    [SerializeField] public float speed = 8f;
    [SerializeField] public float sprintSpeed = 12f;
    public bool sprintEnabled = true;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float groundGrav = 9f;
    [SerializeField] private float airGrav = 2.5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsSlow;
    [SerializeField] private int extraJumpCount = 1;
    [SerializeField] private GameObject jumpEffect;

    [Header("Wall Mechanics")]
    public Vector2 grabRightOffset = new Vector2(0.16f, 0f);
    public Vector2 grabLeftOffset = new Vector2(-0.16f, 0f);
    public float grabCheckRadius = 0.24f;
    public float slideSpeed = 2.5f;
    public Vector2 wallJumpForce = new Vector2(10.5f, 18f);
    public Vector2 wallClimbForce = new Vector2(4f, 14f);

    [Header("Lanterns")]
    [SerializeField] private GameObject normalLantern;
    [SerializeField] private GameObject brokenLantern;

    [Header("Dashing")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float startDashTime = 0.1f;
    [SerializeField] private float dashCooldown = 0.2f;
    [SerializeField] private GameObject dashEffect;

    [Header("Slope / Ground Stick")]
    [SerializeField] private float groundRayDistance = 1.5f;
    [SerializeField] private float slopeStickForce = 40f;
    [SerializeField] private float maxSlopeAngle = 55f;

    [Header("Crouch / Slide")]
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float crouch_slideSpeed = 9f;
    [SerializeField] float slideDuration = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkSound;
    // Logic States
    public bool isGrounded;
    public bool isSlowed;
    [HideInInspector] public float moveInput;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool actuallyWallGrabbing = false;
    [HideInInspector] public bool isSprinting = false;
    [HideInInspector] public bool isDashing = false;
    [HideInInspector] public bool isCrouching = false;
    [HideInInspector] public bool isSliding = false;

    // Internal References
    [SerializeField] private Rigidbody2D m_rb;
    private CapsuleCollider2D m_col;
    private SpriteRenderer[] _spriteRenderers;
    private ParticleSystem m_dustParticle;
    private ILantern _lantern;
    public ILantern Lantern => _lantern;
    private ILantern _normalLantern;
    private ILantern _brokenLantern;

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
    private float m_originalDrag;
    private bool isKnockedBack = false;
    // Slope internals
    private Vector2 m_groundNormal = Vector2.up;
    private float m_slopeAngle;
    private bool m_onSlope;
    private bool m_isJumping;

    [SerializeField] Vector2 standColliderSize = new(.5f, 1.5f);
    [SerializeField] Vector2 crouchColliderSize = new(.5f, .75f);

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_col = GetComponent<CapsuleCollider2D>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        m_dustParticle = GetComponentInChildren<ParticleSystem>();
        _lantern = normalLantern.GetComponent<ILantern>();

        // Cache lantern interfaces from the serialized GameObjects
        if (normalLantern != null)
            _normalLantern = normalLantern.GetComponent<ILantern>();
        if (brokenLantern != null)
            _brokenLantern = brokenLantern.GetComponent<ILantern>();
        m_originalDrag = m_rb.drag;
    }

    void Start()
    {
        PoolManager.instance.CreatePool(dashEffect, 2);
        PoolManager.instance.CreatePool(jumpEffect, 2);

        m_extraJumps = extraJumpCount;
        m_dashTime = startDashTime;
        m_dashCooldown = dashCooldown;

        audioSource.clip = walkSound;

        if (GameController.I != null)
        {
            GameController.I.OnLanternStateChanged += SwapLantern;
        }
    }

    void OnEnable()
    {
        // Subscribe in Start() instead to avoid race condition with GameController.Awake()
    }

    void OnDisable()
    {
        if(GameController.I != null)
        {
            GameController.I.OnLanternStateChanged -= SwapLantern;
        }
    }

    private void FixedUpdate()
    {
        // 1. Environmental Checks
        _prevGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        // isGrounded = CheckGroundWithRaycast(); 
        isSlowed = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsSlow);

        var pos = (Vector2)transform.position;
        m_onRightWall = Physics2D.OverlapCircle(pos + grabRightOffset, grabCheckRadius, whatIsGround);
        m_onLeftWall = Physics2D.OverlapCircle(pos + grabLeftOffset, grabCheckRadius, whatIsGround);
        m_onWall = m_onRightWall || m_onLeftWall;

        CalculateSides();
        DetectGroundNormal();

        if ((m_wallGrabbing || isGrounded) && m_wallJumping) m_wallJumping = false;

        // Clear jump flag once the player has landed (grounded and no longer rising)
        if (isGrounded && m_rb.velocity.y <= 0.1f) m_isJumping = false;

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
            {
                m_rb.velocity = new Vector2(moveInput * run, m_rb.velocity.y);
            }
            else if (!canMove)
            {
                m_rb.velocity = new Vector2(0f, m_rb.velocity.y);
            }
        }

        // 4. Gravity Modifiers
        if (m_rb.velocity.y < 0f)
            m_rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;

        // Water and Snow speed Modifiers
        if (isSlowed)
        {
            sprintEnabled = false;
        }
        else
        {
            sprintEnabled = true;
        }

        // Slope Stick — push player into the slope surface when grounded
        if (isGrounded && m_onSlope && !m_wallGrabbing && !isDashing && !m_isJumping && Mathf.Abs(moveInput) > 0.01f)
        {
            m_rb.AddForce(-m_groundNormal * slopeStickForce, ForceMode2D.Force);
            Vector2 slopeTangent = Vector2.Perpendicular(m_groundNormal); // tangent along the slope
            // Perpendicular gives a vector rotated 90 CCW ensure it points
            // in the player's movement direction.
            if (slopeTangent.x * moveInput < 0f) slopeTangent = -slopeTangent;

            float currentSpeed = isSprinting ? sprintSpeed : speed;
            if (isCrouching) currentSpeed = crouchSpeed;
            if (isSliding) currentSpeed = crouch_slideSpeed;

            m_rb.velocity = slopeTangent * (Mathf.Abs(moveInput) * currentSpeed);
        }

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
        bool isWalking =
        canMove &&
        isGrounded &&
        !m_wallGrabbing &&
        !isSliding &&
        Mathf.Abs(moveInput) > 0.1f;

        if (isWalking)
        {
        if (!audioSource.isPlaying)
        audioSource.Play();
        }
        else
        {
        if (audioSource.isPlaying)
        audioSource.Stop();
        }
        // 8. Visuals
        HandleVisuals();
    }

    private void Update()
    {
        moveInput = InputSystem.HorizontalRaw();
        
        if (isGrounded)
        {
            fallMultiplier = groundGrav;
            m_extraJumps = extraJumpCount;
            m_hasDashedInAir = false;
            m_groundedRemember = m_groundedRememberTime;
        }
        else
        {
            fallMultiplier = airGrav;
        }
        m_groundedRemember -= Time.deltaTime;

        // Guard input if not playable
        if (GameController.I != null && GameController.I.State != PlayState.Exploring) return;

        isSprinting = sprintEnabled;

        // Dash Input
        if (InputSystem.Dash() && !isDashing && !m_hasDashedInAir && m_dashCooldown <= 0f)
        {
            if (_lantern == null || _lantern.TryUseDash())
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
                m_isJumping = true;
                m_rb.velocity = new Vector2(m_rb.velocity.x, jumpForce);
                PoolManager.instance.ReuseObject(jumpEffect, groundCheck.position, Quaternion.identity);
            }
            else if (m_extraJumps > 0 && !m_wallGrabbing) // Double Jump
            {
                if (_lantern == null || _lantern.TryUseDoubleJump())
                {
                    m_isJumping = true;
                    m_rb.velocity = new Vector2(m_rb.velocity.x, jumpForce * 0.8f);
                    m_extraJumps--;
                    PoolManager.instance.ReuseObject(jumpEffect, groundCheck.position, Quaternion.identity);
                }
            }
            else if (m_wallGrabbing) // Wall Jumps
            {
                m_isJumping = true;
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

        if (!isSliding)
        { //CROUCH DISABLED
            bool wantsToCrouch = false;
                //InputSystem.CrouchHeld();

            if (!wantsToCrouch && HeadClear())
            {
                isCrouching = false;
                ResizeColliderHeight(standColliderSize.y);
            }
            else if(wantsToCrouch)
            {
                isCrouching = true;
                ResizeColliderHeight(crouchColliderSize.y);
            }
        }
        // Flip Logic
        if ((!m_facingRight && moveInput > 0f) || (m_facingRight && moveInput < 0f)) Flip();

        // Out of bounds check
        if (transform.position.y < fallKillHeight)
        {
            // Instantly kill by dealing massive damage
            if (_lantern != null) _lantern.TakeDamage(9999);
        }
        

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
        // if (_spriteRenderers != null)
        // {
        //     foreach (var sr in _spriteRenderers)
        //     {
        //     sr.flipX = !m_facingRight;
        //     }
        // }
        transform.Rotate(0f, 180f, 0f);
        // _lantern?.UpdateFacingDirection(!m_facingRight);
    }
    void CalculateSides()
    {
        m_onWallSide = m_onRightWall ? 1 : (m_onLeftWall ? -1 : 0);
        m_playerSide = m_facingRight ? 1 : -1;
    }
    void DetectGroundNormal()
    {
        /// Raycasts downward from the ground-check point to read the surface
        /// normal. Sets m_groundNormal, m_slopeAngle, and m_onSlope.
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundRayDistance,
            whatIsGround
        );

        if (hit.collider != null)
        {
            m_groundNormal = hit.normal;
            m_slopeAngle = Vector2.Angle(m_groundNormal, Vector2.up);
            m_onSlope = m_slopeAngle > 0.5f && m_slopeAngle <= maxSlopeAngle;
        }
        else
        {
            // isGrounded = false;
            m_groundNormal = Vector2.up;
            m_slopeAngle = 0f;
            m_onSlope = false;
        }
    }
    

    bool HeadClear()
    {
        float crouchTop = m_col.bounds.max.y;
        float distanceToCheck = (standColliderSize.y - crouchColliderSize.y) + 0.1f;

        // Cast a box that is the width of the player, but very thin vertically, 
        // starting from the top of the head and moving upwards.
        RaycastHit2D hit = Physics2D.BoxCast(
            new Vector2(transform.position.x, crouchTop), 
            new Vector2(standColliderSize.x * 0.9f, 0.1f), // Slightly thinner width to avoid snagging walls
            0f, 
            Vector2.up, 
            distanceToCheck, 
            whatIsGround
        );

        return hit.collider == null;
    }


    public void EnterHiding(Vector3 hidePos)
    {
        SetMoveable(false);
        m_rb.simulated = false; 
        transform.position = hidePos;
        SetVisible(false);
    }

    public void ExitHiding(Vector3 exitPos)
    {
        transform.position = exitPos;
        m_rb.simulated = true;
        SetMoveable(true);
        SetVisible(true);
    }

    public void RespawnReset()
    {
        SetMoveable(true); 
        SetVisible(true);
        m_rb.velocity = Vector2.zero;
        m_rb.angularVelocity = 0f;

        // Reset Logic Flags
        isDashing = false;
        isSliding = false;
        isCrouching = false;
        m_wallGrabbing = false;
        m_extraJumps = extraJumpCount; // Give them their jumps back

        _lantern?.ResetHealth();
    }

    public void SetMoveable(bool canMove)
    {
        
        if (!canMove)
        {
            m_rb.velocity = Vector2.zero;
            m_rb.bodyType = RigidbodyType2D.Kinematic; 
            this.canMove = false;
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

    public void SwapLantern(bool useBroken)
    {
        normalLantern.SetActive(!useBroken);
        brokenLantern.SetActive(useBroken);
        
        _lantern = useBroken ? _brokenLantern : _normalLantern;
        _lantern.ResetHealth();
    }
    public IEnumerator ApplyKnockback(Vector2 force)
    {
        isKnockedBack = true;
        m_rb.velocity = Vector2.zero;
        m_rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(.6f);
        isKnockedBack = false;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Icicle"))
        {
            if (other.transform.position.y > transform.position.y)
            {
                _lantern?.TakeDamage(40);
                Vector2 contactPoint = other.transform.position;
                Vector2 knockbackDir = ((Vector2)transform.position - contactPoint).normalized;
                StartCoroutine(ApplyKnockback(knockbackDir * 2000f));
                other.gameObject.SetActive(false);
            }
        }

        if (other.CompareTag("Tail"))
        {
            _lantern?.TakeDamage(25, 2);
            Vector2 contactPoint = other.transform.position;
            Vector2 knockbackDir = ((Vector2)transform.position - contactPoint).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir * 2000f));
            other.gameObject.SetActive(false);
        }
        if (other.CompareTag("deathzone"))
        {
            _lantern?.TakeDamage(9999, 9999);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            _lantern?.TakeDamage(25);
            Vector2 contactPoint = collision.GetContact(0).point;
            Vector2 knockbackDir = ((Vector2)transform.position - contactPoint).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir * 1000f));
        }

        if (collision.collider.CompareTag("Spike"))
        {
            _lantern?.TakeDamage(10, 5);
            Vector2 contactPoint = collision.GetContact(0).point;
            Vector2 knockbackDir = ((Vector2)transform.position - contactPoint).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir * 1250f));
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Lamp"))
        {
            _lantern?.RegenFireflies(5f);
        }
    }

}