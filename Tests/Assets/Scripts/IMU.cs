using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IMU : MonoBehaviour {

	[Range(0,1)]
	public float a;
	public static Vector3 angularVelocity,accel;
	public static double Yaw, Roll, Pitch;

	public double aRoll, aPitch, magX, magY, time;
	private UDPClient server;
	private Vector3 eA;//SHORT FOR EULERANGLES, UT SINCE IT'S CALLED A LOT, MIGHT AS WELL ABREVIATE THE NAME
	private AHRS ahrs;

	// Use this for initialization
	void Start () {

		ahrs = new AHRS (0.001f);
		server = FindObjectOfType<UDPClient> ();
		angularVelocity = accel = Vector3.zero;
		Yaw = Roll = Pitch = 1;
		aRoll = aPitch = 0;
		magX = magY = 0;
		time = 0;
	}

	//Equations taken from: https://theccontinuum.com/2012/09/24/arduino-imu-pitch-roll-from-accelerometer/
	Vector3 ComputeRotation(){

		double t = Time.time;
		if(server.IsActive()){
			
			if (double.IsNaN(Yaw) || double.IsNaN(Roll)) {
				Yaw = 1;
				Roll = 1;
			} 
			else {
				Roll = Math.Atan (-Transmitter.Gaccel.x/Transmitter.Gaccel.z) * Mathf.Rad2Deg;
				Yaw = Math.Atan (Transmitter.Gaccel.y/Math.Sqrt(Math.Pow(Transmitter.Gaccel.x,2) + Math.Pow(Transmitter.Gaccel.z,2))) * Mathf.Rad2Deg;
			}

			Pitch = Math.Atan(-Transmitter.magnetometer.y/Transmitter.magnetometer.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler (new Vector3((float) Roll,(float)Pitch, (float) Yaw));
		}
		time = Time.time - t;

		//DONT KNOW WHY, BUT  PITCH->X, YAW
		return new Vector3((float) Roll,(float)Pitch, (float) Yaw);
	}

	void Update () {

//		print (AHRS.Quaternion[1] + " " + AHRS.Quaternion[2] + " " + AHRS.Quaternion[3] + " " + AHRS.Quaternion[0]);
		transform.rotation = new Quaternion (AHRS.Quaternion[1],AHRS.Quaternion[2],AHRS.Quaternion[3],AHRS.Quaternion[0]);
//		CalculateAngles ();
//		print(Transmitter.magnetometer + "    " + Transmitter.accelerometer);
	}

	void CalculateAngles(){

		eA = ComputeRotation ();

//		double w, x, y, z;
//		w = Math.Cos (eA.y / 2) * Math.Cos(eA.z/2) * Math.Cos(eA.x/2) + Math.Sin(eA.y/2) * Math.Sin(eA.z/2) * Math.Sin(eA.x/2);
//		x = Math.Sin (eA.y / 2) * Math.Cos(eA.z/2) * Math.Cos(eA.x/2) - Math.Cos(eA.y/2) * Math.Sin(eA.z/2) * Math.Sin(eA.x/2);
//		y = Math.Cos (eA.y / 2) * Math.Sin(eA.z/2) * Math.Cos(eA.x/2) + Math.Sin(eA.y/2) * Math.Cos(eA.z/2) * Math.Sin(eA.x/2);
//		z = Math.Cos (eA.y / 2) * Math.Cos(eA.z/2) * Math.Cos(eA.x/2) - Math.Sin(eA.y/2) * Math.Sin(eA.z/2) * Math.Cos(eA.x/2);
//		transform.rotation = new Quaternion((float)x,(float)y,(float)z,(float)w);
	}
}
