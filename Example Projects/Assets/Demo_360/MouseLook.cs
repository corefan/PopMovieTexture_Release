﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public bool		UseGyroOnMobile = true;
	private Quaternion?	mInitialGyro = null;
	
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;
	
	public float minimumX = -360F;
	public float maximumX = 360F;
	
	public float minimumY = -60F;
	public float maximumY = 60F;
	
	float rotationX = 0F;
	float rotationY = 0F;
	
	Quaternion originalRotation;


	public void ResetOrientation()
	{
		mInitialGyro = null;
	}

	static public bool UsingVr()
	{
		//	vr exists from 5.1 onwards
#if UNITY_5_1
		if (UnityEngine.VR.VRSettings.loadedDevice == UnityEngine.VR.VRDeviceType.None)
			return false;


		if (!UnityEngine.VR.VRDevice.isPresent)
			return false;

		return true;
#else
		return false;
#endif
	}


	void Update ()
	{
		//	in VR mode, no mouse/gyro at all!
		if (UsingVr() )
			return;

		bool UseGyro = UseGyroOnMobile;
		{
			if (UseGyro)
				Input.gyro.enabled = true;
			UseGyro &= Input.gyro.enabled;
		}
	

		if (UseGyro) 
		{
			bool GyroValid = Input.gyro.enabled;
			{
				//	first usage of the attituide is all zeros, ignore this
				Vector4 Gyro4 =  new Vector4( Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w );
				GyroValid &= Gyro4.SqrMagnitude() > 0;
			}

			if ( GyroValid )
			{
				//	correction stolen from google cardboad SDK
				var att = Input.gyro.attitude;
				att = new Quaternion(att.x, att.y, -att.z, -att.w);
				att = Quaternion.Euler(0, 0, 0) * att;

				if ( mInitialGyro == null )
				{
					mInitialGyro = att;
				}

				transform.localRotation = Quaternion.Inverse(mInitialGyro.Value) * att;
			}
		}
		else if (axes == RotationAxes.MouseXAndY)
		{
			// Read the mouse input axis
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			
			rotationX = ClampAngle (rotationX, minimumX, maximumX);
			rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);
			
			transform.localRotation = originalRotation * xQuaternion * yQuaternion;
		}
		else if (axes == RotationAxes.MouseX)
		{
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationX = ClampAngle (rotationX, minimumX, maximumX);
			
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			transform.localRotation = originalRotation * xQuaternion;
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion yQuaternion = Quaternion.AngleAxis (-rotationY, Vector3.right);
			transform.localRotation = originalRotation * yQuaternion;
		}
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
		originalRotation = transform.localRotation;
	}
	
	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}
}