using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public Transform centerobj;
	public float camspeed = 10f;
	bool maxOut = false;
	bool maxIn = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		//Rotate Camera Around World
		transform.LookAt (centerobj);

		if (Input.GetKey (KeyCode.W) && transform.position.y < 12.5f) {
			transform.Translate(Vector3.up * camspeed * Time.deltaTime);
		}
		if (Input.GetKey (KeyCode.A)) {
			transform.Translate(Vector3.left * camspeed * Time.deltaTime);
		}
		if (Input.GetKey (KeyCode.S) && transform.position.y > -12.5f) {
			transform.Translate(Vector3.down * camspeed * Time.deltaTime);
		}
		if (Input.GetKey (KeyCode.D)) {
			transform.Translate(Vector3.right * camspeed * Time.deltaTime);
		}

		//Zoom in/out
		if (Vector3.Distance(this.transform.position, centerobj.transform.position) < 10){
			maxIn = true;
		} else {
			maxIn = false;
		}

		if (Vector3.Distance(this.transform.position, centerobj.transform.position) > 20){
			maxOut = true;
		} else {
			maxOut = false;
		}

		if (maxIn == false && Input.GetAxis("Mouse ScrollWheel") > 0){
			transform.position = Vector3.MoveTowards(transform.position, centerobj.transform.position, Input.GetAxis("Mouse ScrollWheel") * 150 * Time.deltaTime);
		}

		if (maxOut == false && Input.GetAxis("Mouse ScrollWheel") < 0){
			transform.position = Vector3.MoveTowards(transform.position, centerobj.transform.position, Input.GetAxis("Mouse ScrollWheel") * 150 * Time.deltaTime);
		}

		transform.LookAt (centerobj);

	}


}
