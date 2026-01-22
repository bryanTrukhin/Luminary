using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupanthaPaul
{
	public class CameraFollow : MonoBehaviour
	{
	    [SerializeField]
		private Transform target;
		[SerializeField]
		private float smoothSpeed = 0.125f;
		public Vector3 offset;
		[Header("Camera bounds")]
		public Vector3 minCamerabounds;
		public Vector3 maxCamerabounds;


		private void FixedUpdate()
		{
			Vector3 desiredPosition = target.position + offset;
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
}
