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
	static readonly string FlashAreaInput = "FlashArea";
	static readonly string FlashLanternInput = "Flash";
	static readonly string HealInput = "Heal";

	static readonly string HorizontalLookInput = "HorizontalLook";
	static readonly string VerticalLookInput = "VerticalLook";
	
	static readonly string debugSwitchLanternInput = "debugSwitchLanternInput"; // right mouse for testing lantern break

	public static float HorizontalRaw()
	{
		return Input.GetAxisRaw(HorizontalInput);
	}
	public static float HorizontalLookRaw() // For arrow keys
	{
		return Input.GetAxisRaw(HorizontalLookInput);
	}
	public static float VerticalLookRaw() // For arrow keys
	{
		return Input.GetAxisRaw(VerticalLookInput);
	}

	public static bool Jump()
	{
		return Input.GetButtonDown(JumpInput); // Space
	}

	public static bool Dash()
	{
		return Input.GetButtonDown(DashInput); // J key
	}

	public static bool CrouchHeld()
	{
		return Input.GetButton(CrouchInput); // S key (unused)
	}
	
	public static bool SlidePressed()
	{
		return Input.GetButtonDown(SlideInput); // unused?
	}

	public static bool Interact()
	{
		return Input.GetButtonDown(InteractInput); // F key
	}
	
	public static bool FlashLantern()
	{
		return Input.GetButtonDown(FlashLanternInput); // left mouse
	}
	public static bool Heal()
	{
		return Input.GetButtonDown(HealInput); // I key
	}
	public static bool FlashArea()
	{
		return Input.GetButtonDown(FlashAreaInput); // m
	}
	public static bool ToggleBrokenState()
	{
		return Input.GetButtonDown(debugSwitchLanternInput);
	}
	
}
