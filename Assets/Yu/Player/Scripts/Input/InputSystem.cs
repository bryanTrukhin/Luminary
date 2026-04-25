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
	static readonly int FlashLanternInput = 0;
	
	static readonly string HorizontalLookInput = "HorizontalLook";
	static readonly string VerticalLookInput = "VerticalLook";
	
	static readonly string debugSwitchLanternInput = "debugSwitchLanternInput"; // right mouse for testing lantern break

	public static float HorizontalRaw()
	{
		return Input.GetAxisRaw(HorizontalInput);
	}
	public static float HorizontalLookRaw()
	{
		return Input.GetAxisRaw(HorizontalLookInput);
	}
	public static float VerticalLookRaw()
	{
		return Input.GetAxisRaw(VerticalLookInput);
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
	
	public static bool FlashLantern()
	{
		return Input.GetMouseButtonDown(FlashLanternInput); // left mouse
	}
	public static bool ToggleBrokenState()
	{
		return Input.GetButtonDown(debugSwitchLanternInput);
	}
	
}
