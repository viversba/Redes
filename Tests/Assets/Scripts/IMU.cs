using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IMU : MonoBehaviour {

//	[Range(0,1)]
//	public float a;
	[Range(0,1)]
	public float SamplePeriod;
	public static Vector3 angularVelocity,accel;
	public static double Yaw, Roll, Pitch;

	private float[] quaternion;
	private float[] eInt;
//	public double aRoll, aPitch, magX, magY, time;
	private double time;
	private float Beta;
	private float Kp = 1, Ki = 0;
	private UDPClient server;
	private Vector3 eA;//SHORT FOR EULERANGLES, UT SINCE IT'S CALLED A LOT, MIGHT AS WELL ABREVIATE THE NAME
	private AHRS ahrs;
	private Vector3 actualPosition;

	// Use this for initialization
	void Start () {

		quaternion = new float[] { 1f, 0f, 0f, 0f };
		eInt = new float[] { 0f, 0f, 0f };
		actualPosition = transform.position;
//		SamplePeriod = 1f/131f;
		Beta = 0.1f;
	}

	void Update () {

//		UpdateRotation();
		UpdatePositionAndRotation();
	}

	void UpdatePositionAndRotation(){

		float gx, gy, gz, ax, ay, az, mx, my, mz;
		gx = Transmitter.gyroscope.x;	
		gy = Transmitter.gyroscope.y;
		gz = Transmitter.gyroscope.z;
		ax = Transmitter.accelerometer.x;	
		ay = Transmitter.accelerometer.y;
		az = Transmitter.accelerometer.z;
		mx = Transmitter.magnetometer.x;
		my = Transmitter.magnetometer.y;
		mz = Transmitter.magnetometer.z;

		float q1 = quaternion[0], q2 = quaternion[1], q3 = quaternion[2], q4 = quaternion[3];   // short name local variable for readability
		float norm;
		float hx, hy, bx, bz;
		float vx, vy, vz, wx, wy, wz;
		float ex, ey, ez;
		float pa, pb, pc;

		// Auxiliary variables to avoid repeated arithmetic
		float q1q1 = q1 * q1;
		float q1q2 = q1 * q2;
		float q1q3 = q1 * q3;
		float q1q4 = q1 * q4;
		float q2q2 = q2 * q2;
		float q2q3 = q2 * q3;
		float q2q4 = q2 * q4;
		float q3q3 = q3 * q3;
		float q3q4 = q3 * q4;
		float q4q4 = q4 * q4;   

		// Normalise accelerometer measurement
		norm = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
		if (norm == 0f) return; // handle NaN
		norm = 1 / norm;        // use reciprocal for division
		ax *= norm;
		ay *= norm;
		az *= norm;

		// Normalise magnetometer measurement
		norm = (float)Math.Sqrt(mx * mx + my * my + mz * mz);
		if (norm == 0f) return; // handle NaN
		norm = 1 / norm;        // use reciprocal for division
		mx *= norm;
		my *= norm;
		mz *= norm;

		// Reference direction of Earth's magnetic field
		hx = 2f * mx * (0.5f - q3q3 - q4q4) + 2f * my * (q2q3 - q1q4) + 2f * mz * (q2q4 + q1q3);
		hy = 2f * mx * (q2q3 + q1q4) + 2f * my * (0.5f - q2q2 - q4q4) + 2f * mz * (q3q4 - q1q2);
		bx = (float)Math.Sqrt((hx * hx) + (hy * hy));
		bz = 2f * mx * (q2q4 - q1q3) + 2f * my * (q3q4 + q1q2) + 2f * mz * (0.5f - q2q2 - q3q3);

		// Estimated direction of gravity and magnetic field
		vx = 2f * (q2q4 - q1q3);
		vy = 2f * (q1q2 + q3q4);
		vz = q1q1 - q2q2 - q3q3 + q4q4;
		wx = 2f * bx * (0.5f - q3q3 - q4q4) + 2f * bz * (q2q4 - q1q3);
		wy = 2f * bx * (q2q3 - q1q4) + 2f * bz * (q1q2 + q3q4);
		wz = 2f * bx * (q1q3 + q2q4) + 2f * bz * (0.5f - q2q2 - q3q3);  

		// Error is cross product between estimated direction and measured direction of gravity
		ex = (ay * vz - az * vy) + (my * wz - mz * wy);
		ey = (az * vx - ax * vz) + (mz * wx - mx * wz);
		ez = (ax * vy - ay * vx) + (mx * wy - my * wx);
		if (Ki > 0f)
		{
			eInt[0] += ex;      // accumulate integral error
			eInt[1] += ey;
			eInt[2] += ez;
		}
		else
		{
			eInt[0] = 0.0f;     // prevent integral wind up
			eInt[1] = 0.0f;
			eInt[2] = 0.0f;
		}

		// Apply feedback terms
		gx = gx + Kp * ex + Ki * eInt[0];
		gy = gy + Kp * ey + Ki * eInt[1];
		gz = gz + Kp * ez + Ki * eInt[2];

		// Integrate rate of change of quaternion
		pa = q2;
		pb = q3;
		pc = q4;
		q1 = q1 + (-q2 * gx - q3 * gy - q4 * gz) * (0.5f * SamplePeriod);
		q2 = pa + (q1 * gx + pb * gz - pc * gy) * (0.5f * SamplePeriod);
		q3 = pb + (q1 * gy - pa * gz + pc * gx) * (0.5f * SamplePeriod);
		q4 = pc + (q1 * gz + pa * gy - pb * gx) * (0.5f * SamplePeriod);

		// Normalise quaternion
		norm = (float)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);
		norm = 1.0f / norm;
		quaternion[0] = q1 * norm;
		quaternion[1] = q2 * norm;
		quaternion[2] = q3 * norm;
		quaternion[3] = q4 * norm;

//		print (pa*2 + " " + pb*100 + " " + pc*100);
		Quaternion rotation = new Quaternion (quaternion[2],quaternion[0],quaternion[1],quaternion[3]);
		transform.localRotation = rotation;
//		transform.localPosition = new Vector3(pa*10,pb*10,pc*10) + actualPosition;
	}

	//Code taken from: https://github.com/xioTechnologies/Open-Source-AHRS-With-x-IMU/blob/master/x-IMU%20IMU%20and%20AHRS%20Algorithms/x-IMU%20IMU%20and%20AHRS%20Algorithms/AHRS/MahonyAHRS.cs
	public void UpdateRotation()
	{
		float gx, gy, gz, ax, ay, az;
		gx = Transmitter.gyroscope.x;	
		gy = Transmitter.gyroscope.y;
		gz = Transmitter.gyroscope.z;
		ax = Transmitter.accelerometer.x;	
		ay = Transmitter.accelerometer.y;
		az = Transmitter.accelerometer.z;

		float q1 = quaternion[0], q2 = quaternion[1], q3 = quaternion[2], q4 = quaternion[3];   // short name local variable for readability
		float norm;
		float vx, vy, vz;
		float ex, ey, ez;
		float pa, pb, pc;

		// Normalise accelerometer measurement
		norm = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
		if (norm == 0f) return; // handle NaN
		norm = 1 / norm;        // use reciprocal for division
		ax *= norm;
		ay *= norm;
		az *= norm;

		// Estimated direction of gravity
		vx = 2.0f * (q2 * q4 - q1 * q3);
		vy = 2.0f * (q1 * q2 + q3 * q4);
		vz = q1 * q1 - q2 * q2 - q3 * q3 + q4 * q4;

		// Error is cross product between estimated direction and measured direction of gravity
		ex = (ay * vz - az * vy);
		ey = (az * vx - ax * vz);
		ez = (ax * vy - ay * vx);
		if (Ki > 0f)
		{
			eInt[0] += ex;      // accumulate integral error
			eInt[1] += ey;
			eInt[2] += ez;
		}
		else
		{
			eInt[0] = 0.0f;     // prevent integral wind up
			eInt[1] = 0.0f;
			eInt[2] = 0.0f;
		}

		// Apply feedback terms
		gx = gx + Kp * ex + Ki * eInt[0];
		gy = gy + Kp * ey + Ki * eInt[1];
		gz = gz + Kp * ez + Ki * eInt[2];

		// Integrate rate of change of quaternion
		pa = q2;
		pb = q3;
		pc = q4;
		q1 = q1 + (-q2 * gx - q3 * gy - q4 * gz) * (0.5f * SamplePeriod);
		q2 = pa + (q1 * gx + pb * gz - pc * gy) * (0.5f * SamplePeriod);
		q3 = pb + (q1 * gy - pa * gz + pc * gx) * (0.5f * SamplePeriod);
		q4 = pc + (q1 * gz + pa * gy - pb * gx) * (0.5f * SamplePeriod);

		// Normalise quaternion
		norm = (float)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);
		norm = 1.0f / norm;
		quaternion[0] = q1 * norm;
		quaternion[1] = q2 * norm;
		quaternion[2] = q3 * norm;
		quaternion[3] = q4 * norm;

		print (q2*100 + " " + q3*100 + " " + q4*100);
		Quaternion rotation = new Quaternion (quaternion[2],quaternion[0],quaternion[1],quaternion[3]);
		transform.localRotation = rotation;
//		transform.localPosition = new Vector3(gx*5,gy*5,gz*5) + actualPosition;
	}
}
