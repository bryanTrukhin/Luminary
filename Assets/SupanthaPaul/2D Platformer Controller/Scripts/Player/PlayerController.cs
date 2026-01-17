using UnityEngine;

namespace SupanthaPaul //this demo's movement system is built ontop of SupanthaPaul's 2d character controller!
{
	public class PlayerController : MonoBehaviour
	{
		#if UNITY_EDITOR
		private Color _groundGizmoColor = Color.red;
		#endif
		private bool  _prevGrounded;           // remember last frame's state

		[HideInInspector] public bool isCurrentlyPlayable = true;

		//public so animator can access
		[SerializeField] public float speed;

		[Header("Sprint")]
		[SerializeField] public float sprintSpeed = 12f;
		public bool sprintEnabled = true;

		[Header("Jumping")]
		[SerializeField] private float jumpForce;
		[SerializeField] private float fallMultiplier;
		[SerializeField] private Transform groundCheck;
		[SerializeField] private float groundCheckRadius;
		[SerializeField] private LayerMask whatIsGround;
		[SerializeField] private int extraJumpCount = 1;
		[SerializeField] private GameObject jumpEffect;

		[Header("Wall grab & jump")]
		[Tooltip("Right offset of the wall detection sphere")]
		public Vector2 grabRightOffset = new Vector2(0.16f, 0f);
		public Vector2 grabLeftOffset = new Vector2(-0.16f, 0f);
		public float grabCheckRadius = 0.24f;
		public float slideSpeed = 2.5f;
		public Vector2 wallJumpForce = new Vector2(10.5f, 18f);
		public Vector2 wallClimbForce = new Vector2(4f, 14f);


		[Header("Dashing")]
		[SerializeField] private float dashSpeed = 30f;
		[Tooltip("Amount of time (in seconds) the player will be in the dashing speed")]
		[SerializeField] private float startDashTime = 0.1f;
		[Tooltip("Time (in seconds) between dashes")]
		[SerializeField] private float dashCooldown = 0.2f;
		[SerializeField] private GameObject dashEffect;


		[Header("Crouch / Slide")]
		[SerializeField] float crouchSpeed = 2f;
		[SerializeField] float crouch_slideSpeed  = 9f;
		[SerializeField] float slideDuration = 0.4f;
		float slideTimer;


		// Access needed for handling animation in Player script and other uses
		[HideInInspector] public bool isGrounded;
		[HideInInspector] public float moveInput;
		[HideInInspector] public bool canMove = true;
		[HideInInspector] public bool actuallyWallGrabbing = false;
		[HideInInspector] public bool isSprinting = false;
		[HideInInspector] public bool isDashing = false;
		[HideInInspector] public bool isCrouching = false;
		[HideInInspector] public bool isSliding = false;

		// controls whether this instance is currently playable or not



		private Rigidbody2D m_rb;
		public CapsuleCollider2D m_col;
		private SpriteRenderer[] _spriteRenderers;
		[SerializeField] Vector2 standColliderSize = new(.5f, 1.5f);
		[SerializeField] Vector2 crouchColliderSize = new(.5f, .75f);


		private ParticleSystem m_dustParticle;
		private bool m_facingRight = true;
		private readonly float m_groundedRememberTime = 0.25f;
		private float m_groundedRemember = 0f;
		private int m_extraJumps;
		private float m_extraJumpForce;
		private float m_dashTime;
		private bool m_hasDashedInAir = false;
		private bool m_onWall = false;
		private bool m_onRightWall = false;
		private bool m_onLeftWall = false;
		private bool m_wallGrabbing = false;
		private readonly float m_wallStickTime = 0.25f;
		private float m_wallStick = 0f;
		private bool m_wallJumping = false;
		private float m_dashCooldown;

		// 0 -> none, 1 -> right, -1 -> left
		private int m_onWallSide = 0;
		private int m_playerSide = 1;

		[Header("Lantern")]
		[SerializeField] private LanternController lantern;
		[SerializeField] public LanternController Lantern => lantern;

		// NEW
		[Header("Lantern Flash")]
		[SerializeField] private float flashBonusIntensity = 2.5f;
		[SerializeField] private float flashDuration       = 0.25f;
		[SerializeField] private float flashCooldown       = 0.5f;
		private float _flashCooldownTimer;


        void Awake()
        {
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
        void Start()
		{
			// create pools for particles
			PoolManager.instance.CreatePool(dashEffect, 2);
			PoolManager.instance.CreatePool(jumpEffect, 2);

			// if it's the player, make this instance currently playable
			if (transform.CompareTag("Player"))
				isCurrentlyPlayable = true;
			if (lantern == null)
        	lantern = GetComponentInChildren<LanternController>();

			m_extraJumps = extraJumpCount;
			m_dashTime = startDashTime;
			m_dashCooldown = dashCooldown;
			m_extraJumpForce = jumpForce * 0.7f;

			m_rb = GetComponent<Rigidbody2D>();
			m_col = GetComponent<CapsuleCollider2D>();
			m_dustParticle = GetComponentInChildren<ParticleSystem>();
		}

		private void FixedUpdate()
		{
			_prevGrounded = isGrounded;
			// check if grounded
			isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

			if (_prevGrounded != isGrounded)       // only spam when it flips
			{
				Debug.Log($"Ground change → {isGrounded} at {Time.time:F2}s");

				// OPTIONAL: log what collider we hit
				#if UNITY_EDITOR
				if (isGrounded)
				{
					var c = Physics2D.OverlapCircle(groundCheck.position,
													groundCheckRadius,
													whatIsGround);
					Debug.Log($"Hit object: {c.transform.name}   Layer: {LayerMask.LayerToName(c.gameObject.layer)}");
				}
				#endif
			}
			#if UNITY_EDITOR
			// cache gizmo colour for OnDrawGizmos
			_groundGizmoColor = isGrounded ? Color.green : Color.red;
			#endif

			var position = transform.position;
			// check if on wall
			m_onWall = Physics2D.OverlapCircle((Vector2)position + grabRightOffset, grabCheckRadius, whatIsGround)
			          || Physics2D.OverlapCircle((Vector2)position + grabLeftOffset, grabCheckRadius, whatIsGround);
			m_onRightWall = Physics2D.OverlapCircle((Vector2)position + grabRightOffset, grabCheckRadius, whatIsGround);
			m_onLeftWall = Physics2D.OverlapCircle((Vector2)position + grabLeftOffset, grabCheckRadius, whatIsGround);

			// calculate player and wall sides as integers
			CalculateSides();

			if ((m_wallGrabbing || isGrounded) && m_wallJumping)
			{
				m_wallJumping = false;
			}
			


			// if this instance is currently playable
			if (isCurrentlyPlayable)
			{
				float run = isSprinting ? sprintSpeed : speed;
				if (isCrouching)          run = crouchSpeed;
				if (isSliding)            run = crouch_slideSpeed;   // keeps momentum
				// horizontal movement
				if(m_wallJumping)
				{
					m_rb.velocity = Vector2.Lerp(m_rb.velocity, (new Vector2(moveInput * speed, m_rb.velocity.y)), 1.5f * Time.fixedDeltaTime);
				}
				else
				{
					if(canMove && !m_wallGrabbing && !isSliding) // walking
						m_rb.velocity = new Vector2(moveInput * run, m_rb.velocity.y);
					else if(!canMove)
						m_rb.velocity = new Vector2(0f, m_rb.velocity.y);
				}
				// better jump physics
				if (m_rb.velocity.y < 0f)
				{
					m_rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
				}

				// Flipping
				if (!m_facingRight && moveInput > 0f)
					Flip();
				else if (m_facingRight && moveInput < 0f)
					Flip();

				// Dashing logic
				if (isDashing)
				{
					if (m_dashTime <= 0f)
					{
						isDashing = false;
						m_dashCooldown = dashCooldown;
						m_dashTime = startDashTime;
						m_rb.velocity = Vector2.zero;
					}
					else
					{
						m_dashTime -= Time.deltaTime;
						if(m_facingRight)
							m_rb.velocity = Vector2.right * dashSpeed;
						else
							m_rb.velocity = Vector2.left * dashSpeed;
					}
				}

				// wall grab
				if(m_onWall && !isGrounded && m_rb.velocity.y <= 0f && m_playerSide == m_onWallSide)
				{
					actuallyWallGrabbing = true;    // for animation
					m_wallGrabbing = true;
					m_rb.velocity = new Vector2(moveInput * speed, -slideSpeed);
					m_wallStick = m_wallStickTime;
				} else
				{
					m_wallStick -= Time.deltaTime;
					actuallyWallGrabbing = false;
					if (m_wallStick <= 0f)
						m_wallGrabbing = false;
				}
				if (m_wallGrabbing && isGrounded)
					m_wallGrabbing = false;

				// Sliding
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
				
				// enable/disable dust particles
				float playerVelocityMag = m_rb.velocity.sqrMagnitude;
				if (m_dustParticle.isPlaying && playerVelocityMag == 0f)
				{
					m_dustParticle.Stop();
				}
				else if (!m_dustParticle.isPlaying && playerVelocityMag > 0f)
				{
					m_dustParticle.Play();
				}

			}
		}

		private void Update()
		{

			// horizontal input
			moveInput = InputSystem.HorizontalRaw();

			if (isGrounded)
			{
				m_extraJumps = extraJumpCount;
			}

			// grounded remember offset (for more responsive jump)
			m_groundedRemember -= Time.deltaTime;
			if (isGrounded)
				m_groundedRemember = m_groundedRememberTime;


			if (!isCurrentlyPlayable) return;
			

			if (sprintEnabled) isSprinting = sprintEnabled;
			else isSprinting = false;

			// if not currently dashing and hasn't already dashed in air once
			if (!isDashing && !m_hasDashedInAir && m_dashCooldown <= 0f)
			{
				// dash input (left shift)
				if (InputSystem.Dash())
				{
					isDashing = true;
					// dash effect
					PoolManager.instance.ReuseObject(dashEffect, transform.position, Quaternion.identity);
					// if player in air while dashing
					if(!isGrounded)
					{
						m_hasDashedInAir = true;
					}
					// dash logic is in FixedUpdate
				}
			}
			m_dashCooldown -= Time.deltaTime;
			_flashCooldownTimer -= Time.deltaTime;
			
			// if has dashed in air once but now grounded
			if (m_hasDashedInAir && isGrounded)
				m_hasDashedInAir = false;

			// Jumping
			if (InputSystem.Jump() && m_extraJumps > 0 && !isGrounded && !m_wallGrabbing)   // extra jumping
			{
				m_rb.velocity = new Vector2(m_rb.velocity.x, m_extraJumpForce);
				m_extraJumps--;
				// jumpEffect
				PoolManager.instance.ReuseObject(jumpEffect, groundCheck.position, Quaternion.identity);
			}
			else if (InputSystem.Jump() && (isGrounded || m_groundedRemember > 0f)) // normal single jumping
			{
				m_rb.velocity = new Vector2(m_rb.velocity.x, jumpForce);
				// jumpEffect
				PoolManager.instance.ReuseObject(jumpEffect, groundCheck.position, Quaternion.identity);
			}
			else if (InputSystem.Jump() && m_wallGrabbing && moveInput != m_onWallSide)     // wall jumping off the wall
			{
				m_wallGrabbing = false;
				m_wallJumping = true;
				Debug.Log("Wall jumped");
				if (m_playerSide == m_onWallSide)
					Flip();
				m_rb.AddForce(new Vector2(-m_onWallSide * wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
			}
			else if (InputSystem.Jump() && m_wallGrabbing && moveInput != 0 && (moveInput == m_onWallSide))      // wall climbing jump
			{
				m_wallGrabbing = false;
				m_wallJumping = true;
				Debug.Log("Wall climbed");
				if (m_playerSide == m_onWallSide)
					Flip();
				m_rb.AddForce(new Vector2(-m_onWallSide * wallClimbForce.x, wallClimbForce.y), ForceMode2D.Impulse);
			}

		if (InputSystem.SlidePressed()
		&& isGrounded
		&& !isSliding
		&& isSprinting                      // must be in run-mode
		&& Mathf.Abs(moveInput) > 0.1f)     // actually moving
		{
			isSliding = true;
			isCrouching = false;
			slideTimer = slideDuration;
			float dir = m_facingRight ? 1f : -1f;
			m_rb.velocity = new Vector2(dir * crouch_slideSpeed, 0f);
		}
			if (!isSliding)
			{
				isCrouching = InputSystem.CrouchHeld();

				// try to stand up
				if (!isCrouching && HeadClear())
					ResizeColliderHeight(standColliderSize.y);
				else if (isCrouching)
					ResizeColliderHeight(crouchColliderSize.y);  // shrink collider while crouched
			}

		if (InputSystem.FlashLantern()
	    && _flashCooldownTimer <= 0f
	    && lantern != null
	    && (GameController.I == null || GameController.I.State != PlayState.Hiding))
		{
			lantern.Flash(flashBonusIntensity, flashDuration);
			_flashCooldownTimer = flashCooldown;
		}

		}

		void Flip()
		{
			m_facingRight = !m_facingRight;
			Vector3 scale = transform.localScale;
			scale.x *= -1;
			transform.localScale = scale;
		}

		void CalculateSides()
		{
			if (m_onRightWall)
				m_onWallSide = 1;
			else if (m_onLeftWall)
				m_onWallSide = -1;
			else
				m_onWallSide = 0;

			if (m_facingRight)
				m_playerSide = 1;
			else
				m_playerSide = -1;
		}

		bool HeadClear()
		{
			Vector2 origin = (Vector2)transform.position + Vector2.up * crouchColliderSize.y * .5f;
			float extra = .05f;
			return !Physics2D.BoxCast(origin, standColliderSize, 0f, Vector2.up, (standColliderSize.y - crouchColliderSize.y) + extra, whatIsGround);
		}

		public void SetPlayable(bool on)
		{
			isCurrentlyPlayable = on;
			if(on == false)
			{
				m_rb.bodyType = RigidbodyType2D.Kinematic;
				m_rb.velocity = Vector2.zero;
			}
			else
			{
				m_rb.bodyType = RigidbodyType2D.Dynamic;
			}
		}
		public void SetVisible(bool visible)
        {
            if (_spriteRenderers == null) return;

            foreach (var sr in _spriteRenderers)
            {
                sr.enabled = visible;
            }
        }
				
		void ResizeColliderHeight(float newHeight)
		{
			float oldHeight = m_col.size.y;
			if (Mathf.Approximately(oldHeight, newHeight)) return;

			Vector2 size = m_col.size;
			size.y = newHeight;
			m_col.size = size;

			// shift the offset so the bottom stays flush with the ground
			Vector2 off = m_col.offset;
			off.y -= (oldHeight - newHeight) * 1f;   // move centre DOWN by half the shrink
			m_col.offset = off;
		}

	}
}
