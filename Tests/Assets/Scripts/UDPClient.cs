using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

public class UDPClient : MonoBehaviour {

	public bool active;
	public static Vector3 accelerometer;
	public static Vector3 gyroscope;
	public static Vector3 magnetometer;
	public static Semaphore semaphore;
	public static readonly object lockObject = new object();

	private Thread thread;
	private Transmitter transmitter;
	private bool processData = false;
	private string returnData = "";
	private string[] datos = new string[13];
	private static UdpClient UDP;

	void Start(){

		DontDestroyOnLoad (gameObject);
		active = true;
		semaphore = new Semaphore(0,1);
		thread = new Thread(new ThreadStart(Transmit));
		thread.Start();
		accelerometer = Vector3.zero; 
		gyroscope = Vector3.zero; 
		magnetometer = Vector3.zero;
	}

	public void StartTransmition(){

		if (!active) {
			active = true;
		}
		else {
			Debug.LogWarning ("Transmition already on course!");
		}
	}

	public void StopTransmition(){

		if (active) {
			active = false;
		}
		else {
			Debug.LogWarning ("Transmition already stopped!");
		}
	}

	void OnDestroy(){

		UDP.Close ();
		active = false;
	}

	public bool IsActive(){
		return active;
	}

	private void Transmit()
	{
		UDP = new UdpClient(5555);
		Debug.Log ("Transmiter started");
		while (true)
		{
			if (active && UDP.Available>0) {
				
//				semaphore.WaitOne ();
				//print ("ACTIVE");
				IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

				byte[] receiveBytes = UDP.Receive(ref RemoteIpEndPoint);

				/*lock object to make sure there data is 
        *not being accessed from multiple threads at thesame time*/
				lock (lockObject)
				{
					returnData = Encoding.ASCII.GetString(receiveBytes);
					//print (returnData);
					datos = returnData.Split (new string[] {","} , StringSplitOptions.None);
					accelerometer = new Vector3(float.Parse(datos[2]),float.Parse(datos[3]),float.Parse(datos[4]));
					gyroscope = new Vector3(float.Parse(datos[6]),float.Parse(datos[7]),float.Parse(datos[8]));
					magnetometer = new Vector3(float.Parse(datos[10]),float.Parse(datos[11]),float.Parse(datos[12]));
					//Debug.Log (accelerometer + " " + gyroscope + " " + magnetometer);
					//returnData = "";
					if (returnData != null){
						processData = true;
					}
				}

//				semaphore.Release ();
			}
		}
	}
}

