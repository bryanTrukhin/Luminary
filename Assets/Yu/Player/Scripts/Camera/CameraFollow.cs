using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollow : MonoBehaviour
{
	[SerializeField]
	private Transform target;
	[SerializeField]
	private float smoothSpeed = 0.125f;
	public Vector3 originalPos;
	public float verticalOffset;
	public float horizontalOffset;
	[Header("Camera bounds")]
	public Vector3 minCamerabounds;
	public Vector3 maxCamerabounds;

	public float xInput;
	public float yInput;

	private Vector3 _origOffset;

	private void Awake()
	{
		_origOffset = originalPos;
	}

	private void FixedUpdate()
	{
		xInput = InputSystem.HorizontalLookRaw();
		yInput = InputSystem.VerticalLookRaw();

		if (InputSystem.HorizontalRaw() == 0)
		{
			if (xInput == 1)
			{
				originalPos = new Vector3(_origOffset.x + horizontalOffset, _origOffset.y, _origOffset.z);
			}
			else if (xInput == -1)
			{
				originalPos = new Vector3(_origOffset.x - horizontalOffset, _origOffset.y, _origOffset.z);
			}
			else if (yInput == 1)
			{
				originalPos = new Vector3(_origOffset.x, _origOffset.y + verticalOffset, _origOffset.z);
			}
			else if (yInput == -1)
			{
				originalPos = new Vector3(_origOffset.x, _origOffset.y - verticalOffset, _origOffset.z);
			}
		}
		else
		{
			originalPos = new Vector3(_origOffset.x, _origOffset.y, _origOffset.z);
		}
		
		Vector3 desiredPosition = target.position + originalPos;
		Vector3 currentPosition = transform.position;

		Vector3 smoothedPosition = Vector3.Lerp(currentPosition, desiredPosition, smoothSpeed);

		// clamp using world coords
		smoothedPosition = new Vector3(
			Mathf.Clamp(smoothedPosition.x, minCamerabounds.x, maxCamerabounds.x),
			Mathf.Clamp(smoothedPosition.y, minCamerabounds.y, maxCamerabounds.y),
			Mathf.Clamp(smoothedPosition.z, minCamerabounds.z, maxCamerabounds.z)
		);

		transform.position = smoothedPosition;
	}

	public void SetTarget(Transform targetToSet)
	{
		target = targetToSet;
	}
}