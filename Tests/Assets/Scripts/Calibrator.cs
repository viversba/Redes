using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calibrator : MonoBehaviour {

	private Vector3[] gyroSamples;
	private Vector3[] accelSamples;
	private Vector3[] magnetSamples;
	private UDPClient client;
	private int counter;
	// Use this for initialization
	void Start () {

		gyroSamples = new Vector3[1000];
		accelSamples = new Vector3[1000];
		magnetSamples = new Vector3[1000];
		counter = 0;
		client = FindObjectOfType<UDPClient> ();
		if (!client) {
			Debug.LogError ("Client not found in " + name + ", impossible to calibrate");
		}
	}

	public void Init(){

		client.StartTransmition ();
		CalibrateGyroscope ();
		CalibrateAccelerometer ();
		CalibrateMagnetometer ();
	}

	private void CalibrateGyroscope(){

		for (int i=0;i<1000;i++) {
			gyroSamples[i] = client.GetGyroscope ();
		}
		print ("Gyroscope samples taken");
		print (gyroSamples[999]);
	}

	private void CalibrateAccelerometer(){

		for (int i=0;i<1000;i++) {
			accelSamples[i] = client.GetAccelerometer ();
		}
		print ("Accelerometer samples taken");
		print (accelSamples[999]);
	}

	private void CalibrateMagnetometer(){

		for (int i=0;i<1000;i++) {
			magnetSamples[i] = client.GetMagnetometer ();
		}
		print ("Magnetometer samples taken");
		print (magnetSamples[999]);
	}

	public void Finish(){

		client.StopTransmition ();
	}
}
