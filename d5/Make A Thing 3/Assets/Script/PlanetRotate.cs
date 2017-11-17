using UnityEngine;
using System.Collections;

public class PlanetRotate : MonoBehaviour {

	public float PlanetRotateSpeed = -25.0f;
	public Transform centerObj;

	void Update() {
		// planet to spin on it's own axis
		transform.Rotate(centerObj.up * PlanetRotateSpeed * Time.deltaTime);

	}

}
