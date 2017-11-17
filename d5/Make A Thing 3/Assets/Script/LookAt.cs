using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour {

	public Transform centerObj;

	// Update is called once per frame
	void Update () {
		//transform.LookAt (centerObj);
		transform.rotation = Quaternion.LookRotation ((centerObj.position - transform.position).normalized);
	}

	public void Rescue(Transform rescueObj){
		centerObj = rescueObj;
	}
}
