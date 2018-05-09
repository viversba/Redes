using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calibrator : MonoBehaviour {

	public GameObject nextButton;
	public static Vector3 gyroNoise;
	public static Vector3 accelNoise;
	public static Vector3 magnetNoise;

	private Vector3[] gyroSamples;
	private Vector3[] accelSamples;
	private Vector3[] magnetSamples;
	private UDPClient client;
	private int counter;
	// Use this for initialization
	void Start () {

		gyroNoise = Vector3.zero;
		accelNoise = Vector3.zero;
		magnetNoise = Vector3.zero;

		gyroSamples = new Vector3[1000];
		accelSamples = new Vector3[1000];
		magnetSamples = new Vector3[6000];
		counter = 0;
		client = FindObjectOfType<UDPClient> ();
		if (!client) {
			Debug.LogError ("Client not found in " + name + ", impossible to calibrate");
		}
	}

	public void Init(){

		client.StartTransmition ();
		StartCalibration (0);
	}

	private void StartCalibration(int parameter){

		switch (parameter) {
		case 0:
			StartCoroutine(CalibrateGyroscope());
			break;	
		case 1:
			StartCoroutine(CalibrateAccelerometer());
			break;	
		case 2:
			StartCoroutine (CalibrateMagnetometer());
			break;	
		case 3:
			
		default:
			break;	
		}

	}

	public void Finish(){

		client.StopTransmition ();
	}

	IEnumerator CalibrateGyroscope(){

		print ("GYRO CALIBRATION STARTED");
		for (int i = 0; i < 1000; i++) {
			gyroSamples [i] = Transmitter.gyroscope;
			yield return new WaitForSecondsRealtime(0.0001f);
			print ("IMPERSO " + gyroSamples[i]);
		}
		Vector3 mean = Vector3.zero;
		for(int i=0;i<1000;i++){
			mean += gyroSamples [i];
		}
		mean = mean / 1000;
		print ("NOISE OF GYRO: " + mean);
		StartCalibration(1);
//		yield return new WaitForSecondsRealtime(0.0001f);
	}

	IEnumerator CalibrateAccelerometer(){

		print ("ACCELEROMETER CALIBRATION STARTED");
		for (int i = 0; i < 1000; i++) {
			accelSamples [i] = Transmitter.accelerometer;
			yield return new WaitForSeconds (0.005f);
		}
		Vector3 mean = Vector3.zero;
		for(int i=0;i<1000;i++){
			mean += (accelSamples [i] - new Vector3(0f,0f,9.81f));
		}
		mean = mean / 1000;
		print ("NOISE OF ACCEL: " + mean);
		StartCalibration(2);
	}

	IEnumerator CalibrateMagnetometer(){

		Vector3 hardIron = Vector3.zero;
		Vector3 softIron = Vector3.zero;

//		float noiseX = noiseY = noiseZ = 0;

		for (int j = 0; j < 6; j++) {

			print ("TAKING SAMPLES");
			for (int i = j*1000; i < (j+1)*1000; i++) {
				magnetSamples [i] = Transmitter.magnetometer;
				yield return new WaitForSeconds (0.001f);
			}
			if (j % 2 == 0)
				print ("TURN THE DEVICE 180 DEGREES ON CURRENT AXIS");
			else
				print ("TURN DEVICE 90 DEGREES TO NEXT AXIS");
			yield return new WaitForSeconds (2f);
		}

		//CALCULATIONS OF HARD IRON NOISE
		float noiseX,noiseY,noiseZ;
		noiseX = noiseY = noiseZ = 0;
		for (int i = 0; i < 2000; i++) {
			noiseZ += magnetSamples [i].z;
		}
		noiseZ /= 1000;
		for (int i = 0; i < 2000; i++) {
			noiseX += magnetSamples [i].x;
		}
		noiseX /= 1000;
		for (int i = 0; i < 2000; i++) {
			noiseY += magnetSamples [i].y;
		}
		noiseY /= 1000;
		hardIron = new Vector3(noiseX,noiseY,noiseZ)/2;

		//CALCULATIONS OF SOFT IRON NOISE
		StartCalibration(3);
	}

	private void FinishCalibration(){

		nextButton.SetActive (true);

	}
}