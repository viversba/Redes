using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorSprite : MonoBehaviour {

	Calibrator calibrator;

	void Start(){

		calibrator = FindObjectOfType<Calibrator> ();
		if (!calibrator) {
			Debug.LogError ("Calibrator not found in " + name);
		}
	}

	void StartCalibration(){

		calibrator.Init ();
	}
}
