using UnityEngine;


public class InputSystem : MonoBehaviour
{
	// input string caching
	static readonly string HorizontalInput = "Horizontal";
	static readonly string JumpInput = "Jump";
	static readonly string DashInput = "Dash";
	static readonly string CrouchInput = "Crouch";
	static readonly string SlideInput = "Slide";
	static readonly string InteractInput = "Interact";

	public static float HorizontalRaw()
	{
		return Input.GetAxisRaw(HorizontalInput);
	}

	public static bool Jump()
	{
		return Input.GetButtonDown(JumpInput);
	}

	public static bool Dash()
	{
		return Input.GetButtonDown(DashInput);
	}

	public static bool CrouchHeld()
	{
		return Input.GetButton(CrouchInput);
	}
	
	public static bool SlidePressed()
	{
		return Input.GetButtonDown(SlideInput);
	}

	public static bool Interact()
	{
		return Input.GetButtonDown(InteractInput);
	}

	// NEW: left-click flash
	public static bool FlashLantern()
	{
		return Input.GetMouseButtonDown(0); // left mouse
	}
}
