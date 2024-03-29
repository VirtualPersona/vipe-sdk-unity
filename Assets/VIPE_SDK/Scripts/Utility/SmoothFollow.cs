﻿using UnityEngine;

#pragma warning disable 649
namespace UnityStandardAssets.Utility
{
	public class SmoothFollow : MonoBehaviour
	{

		// The target we are following
		[SerializeField]
		public Transform target;
		[SerializeField]
		public bool previewMode = false;
		[SerializeField]
		public bool gameMode = false;
		// The distance in the x-z plane to the target
		[SerializeField]
		private float distance = 10.0f;
		// the height we want the camera to be above the target
		[SerializeField]
		private float height = 5.0f;
		[SerializeField]
		private float maxCameraDistance = 20.0f;
		[SerializeField]
		private float minCameraDistance = 0.0f;
		[SerializeField]
		private float rotationDamping;
		[SerializeField]
		private float heightDamping; // 0.9

		private float currentHeight;
		private float wantedHeight;

        public float Height { get => height; set => height = value; }
        public float Distance { get => distance; set => distance = value; }

        // Use this for initialization
        void Start() { }
		private void Update()
		{
            if (previewMode)
			{
                //this.transform.position = this.transform.position + new Vector3(0, -Input.mouseScrollDelta.y, 0);
				this.transform.position = Vector3.Lerp(transform.position, this.transform.position + new Vector3(0,-Input.mouseScrollDelta.y, 0), Time.deltaTime * 35);

                this.transform.position = new Vector3(this.transform.position.x,Mathf.Clamp(this.transform.position.y, minCameraDistance, maxCameraDistance), this.transform.position.z);
            }
        }
		// Update is called once per frame
		void LateUpdate()
		{
			// Early out if we don't have a target
			if (!target)
				return;

			// Calculate the current rotation angles
			var wantedRotationAngle = target.eulerAngles.y;
			wantedHeight = target.position.y + Height;

			var currentRotationAngle = transform.eulerAngles.y;
			currentHeight = transform.position.y;

			// Damp the rotation around the y-axis
			currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

			// Damp the height
			currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

			// Convert the angle into a rotation
			var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

			// Set the position of the camera on the x-z plane to:
			// distance meters behind the target
			transform.position = target.position;
			transform.position -= currentRotation * Vector3.forward * Distance;

			// Set the height of the camera
			transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

			// Always look at the target
			transform.LookAt(target);
		}
	}
}