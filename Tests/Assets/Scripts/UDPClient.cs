using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class UDPClient : MonoBehaviour {

	static UdpClient UDP;
	Thread thread;

	static readonly object lockObject = new object();
	string returnData = "";
	bool processData = false;
	string[] datos = new string[13];

	public static Vector3 accelerometer;
	public static Vector3 gyroscope;
	public static Vector3 magnetometer;

	private bool active;

	void Start(){

		active = false;
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
		//thread.Abort ();
	}

	void Update(){
		
		//Debug.Log(active);
		if (processData) {

			//Debug.Log ("ADENTRO");
			lock(lockObject){

				processData = false;
				//Process received data
				datos = returnData.Split (new string[] {","}, StringSplitOptions.None);
				accelerometer = new Vector3(float.Parse(datos[2]),float.Parse(datos[3]),float.Parse(datos[4]));
				gyroscope = new Vector3(float.Parse(datos[6]),float.Parse(datos[7]),float.Parse(datos[8]));
				magnetometer = new Vector3(float.Parse(datos[10]),float.Parse(datos[11]),float.Parse(datos[12]));
				Debug.Log (accelerometer + " " + gyroscope + " " + magnetometer);
				returnData = "";
			}
		}
	}

	private void Transmit()
	{
		UDP = new UdpClient(5555);
		while (true)
		{
			if (active) {

				//print ("ACTIVE");
				IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

				byte[] receiveBytes = UDP.Receive(ref RemoteIpEndPoint);

				/*lock object to make sure there data is 
        *not being accessed from multiple threads at thesame time*/
				lock (lockObject)
				{
					returnData = Encoding.ASCII.GetString(receiveBytes);

					datos = returnData.Split (new string[] {","}, StringSplitOptions.None);
					if (returnData != null){
						processData = true;
					}
				}
			}
		}
		UDP.Close ();
	}
}
