using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RescueShip : MonoBehaviour {

	public float rescueTime;
	public float gameTime;
	public Transform rescueLocation;
	public Vector3 startLoc;
	bool startGame = false;
	public float rescueCounter;
	public Text rescueDisplay;

	void Start(){
		startLoc = transform.position;
	}

	void Update () {
		if (startGame) {
			gameTime += Time.deltaTime / rescueTime;
			transform.position = new Vector3 (Mathf.Lerp (startLoc.x, rescueLocation.position.x, gameTime), Mathf.Lerp (startLoc.y, rescueLocation.position.y, gameTime), Mathf.Lerp (startLoc.z, rescueLocation.position.z, gameTime));
		}

		rescueCounter = (rescueTime * 0.85f) - (gameTime * rescueTime);

		if (gameTime < 0.05f) {
			rescueDisplay.text = "Rescue: out of range..";
		} else if (gameTime < 0.85f) {
			rescueDisplay.text = "Rescue: " + Mathf.Clamp ((int)rescueCounter, 0, 99999) + "s";
		} else {
			rescueDisplay.text = "approaching..";
		}
	}

	void StartGame(){
		startGame = true;
	}
}
