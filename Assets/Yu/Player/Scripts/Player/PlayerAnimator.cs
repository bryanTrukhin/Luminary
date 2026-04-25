using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
	private Rigidbody2D m_rb;
	private PlayerController m_controller;
	private Animator m_anim;
	private static readonly int Move = Animator.StringToHash("Move");
	private static readonly int JumpState = Animator.StringToHash("JumpState");
	private static readonly int IsJumping = Animator.StringToHash("IsJumping");
	private static readonly int WallGrabbing = Animator.StringToHash("WallGrabbing");
	private static readonly int IsDashing = Animator.StringToHash("IsDashing");
	private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");
	private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
	private static readonly int IsSliding = Animator.StringToHash("IsSliding");

	

	private void Start()
	{	
		m_anim = GetComponentInChildren<Animator>();
		m_controller = GetComponent<PlayerController>();
		m_rb = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{

		float maxSpeed = Mathf.Max(m_controller.speed, m_controller.sprintSpeed);
		float move01 = Mathf.Clamp01(Mathf.Abs(m_rb.velocity.x) / maxSpeed);

		var speedForAnim = m_controller.isDashing ? 0f : move01;
		// Idle & Running animation
		m_anim.SetFloat(Move, speedForAnim);

		m_anim.SetBool(IsSprinting, m_controller.isSprinting);

		// Jump state (handles transitions to falling/jumping)
		float verticalVelocity = m_rb.velocity.y;
		m_anim.SetFloat(JumpState, verticalVelocity);

		// Jump animation
		if (!m_controller.isGrounded && !m_controller.actuallyWallGrabbing)
		{
			m_anim.SetBool(IsJumping, true);
		}
		else
		{
			m_anim.SetBool(IsJumping, false);
		}

		if (!m_controller.isGrounded && m_controller.actuallyWallGrabbing)
		{
			m_anim.SetBool(WallGrabbing, true);
		}
		else
		{
			m_anim.SetBool(WallGrabbing, false);
		}


		// dash animation
		m_anim.SetBool(IsDashing, m_controller.isDashing);
		
		// Crouch + Slide
		m_anim.SetBool(IsCrouching, m_controller.isCrouching);
		m_anim.SetBool(IsSliding,  m_controller.isSliding);
	}
}