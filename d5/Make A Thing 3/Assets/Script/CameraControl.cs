using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public float camSpeed = 0.2f;
	public Transform playerObj;
	Rigidbody rb;

	void Start(){
		rb = GetComponent<Rigidbody> ();
	}

	void FixedUpdate () {
		transform.LookAt (playerObj);

		rb.AddForce (Input.GetAxis ("Mouse X") * camSpeed, Input.GetAxis ("Mouse Y") * camSpeed, 0);
		//transform.position = Vector3.Lerp (transform.position, targPos, Time.deltaTime);
		//transform.Translate (Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y"), 0);
	}
}
