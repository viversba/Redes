using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class Transmitter : MonoBehaviour {

	public static Vector3 accelerometer;
	public static Vector3 gyroscope;
	public static Vector3 magnetometer;

	private Thread client;
	private UDPClient server;
	private static object syncRoot = new Object();
	private static volatile Transmitter instance;


	private Transmitter(){}

	public static Transmitter Instance
	{
		get 
		{
			if (instance == null)
			{
				lock (syncRoot) 
				{
					if (instance == null) 
						instance = new Transmitter();
				}
			}
			return instance;
		}
	}

	void Start(){

		gyroscope = magnetometer = accelerometer = Vector3.zero;
		instance = new Transmitter ();
		server = FindObjectOfType<UDPClient> ();
	}

	void Update(){

		if (server.IsActive ()) {
			gyroscope = UDPClient.gyroscope;
			accelerometer = UDPClient.accelerometer;
			magnetometer = UDPClient.magnetometer;
		} 
		else {
			gyroscope = accelerometer = magnetometer = Vector3.one;
		}
//		print (gyroscope + " " + accelerometer + " " + magnetometer);
	}


}
