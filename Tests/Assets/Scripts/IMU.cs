using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IMU : MonoBehaviour {

	[Range(0,1)]
	public float a;
	public static Vector3 angularVelocity,accel;
	public static double Roll, Pitch, Yaw;

	private float LSB = 131;//LEAST SIGNIFICATN BIT
	public double aRoll, aPitch, magX, magY, time;
	private UDPClient server;
	private static Vector3 transition;

	// Use this for initialization
	void Start () {

		server = FindObjectOfType<UDPClient> ();
		transition  = angularVelocity = accel = Vector3.zero;
		Roll = Pitch = Yaw = 1;
		aRoll = aPitch = 0;
		magX = magY = 0;
		time = 0;
	}
	
	// Update is called once per frame
	void Update () {

		double t = Time.time;
		if(server.IsActive()){

			Vector3 m = Transmitter.magnetometer.normalized;
			angularVelocity = Transmitter.gyroscope/131;
			accel = Transmitter.accelerometer;


			aRoll = Math.Atan2 (accel.x,accel.z);
			aPitch = Math.Atan2 (accel.y,accel.z);
//			aRoll = Math.Atan(accel.y/(Math.Sqrt(accel.x*accel.x + accel.z*accel.z)));
//			aPitch = Math.Atan(-accel.x/(Math.Sqrt(accel.y*accel.y + accel.z*accel.z)));
//			print (aRoll + "  " + aPitch);

			if (double.IsNaN(Roll) || double.IsNaN(Pitch)) {
				Roll = 1;
				Pitch = 1;
			} 
			else {
				Roll = a * (angularVelocity.x * time + Roll) + (1 - a) * aRoll * Mathf.Rad2Deg;
				Pitch = a * (angularVelocity.y * time + Pitch) + (1 - a) * aPitch * Mathf.Rad2Deg;
			}

			magX = ( m.z * Math.Sin(Pitch) ) - ( m.y * Math.Cos(Pitch) );
			magY = ( m.x * Math.Cos (Roll) ) + ( m.y*Math.Sin(Roll) * Math.Sin(Pitch) ) + ( m.z * Math.Sin(Roll) * Math.Cos(Pitch) );


			Yaw = (Math.Atan ( magX/magY )) * Mathf.Rad2Deg;
			print(Yaw);

//			print (Roll.ToString("N4") + " " + Pitch.ToString("N4") + " " + Yaw.ToString("N4"));

			transform.rotation = Quaternion.Euler(new Vector3((float) Pitch,(float) Yaw, (float) Roll));

//			transform.Rotate (new Vector3((float)Roll,(float)Pitch,(float)Yaw));
//			transform.rotation.eulerAngles = new Vector3 ((float)Roll, (float)Pitch, (float)Yaw);
//			transition = Vector3.Lerp (transition,new Vector3((float)Roll,(float)Pitch,(float)Yaw),Time.deltaTime*1);
//			transform.eulerAngles = new Vector3 ((float)Roll, (float)Pitch, (float)Yaw);
//			transform.rotation = Quaternion.Euler (new Vector3((float)Roll,(float)Pitch,(float)Yaw));
//			print(Quaternion.Euler(new Vector3 ((float)Roll, (float)Pitch, (float)Yaw)));
		}
		time = Time.time - t;
	}
}
