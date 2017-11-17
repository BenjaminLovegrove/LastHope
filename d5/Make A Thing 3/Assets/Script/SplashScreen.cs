using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplashScreen : MonoBehaviour {

	public Image splashImg;
	float splashTimer = -1;
	public AudioSource as1;
	public AudioSource as2;
	public AudioSource as3;

	public Transform rocket;
	public Transform rocketTarg;
	Vector3 rocketStartPos;
	float rocketLerp = 0;


	void Start(){
		rocketStartPos = rocket.position;
	}

	void Update () {
		splashTimer += Time.deltaTime / 4.5f;
		rocketLerp += Time.deltaTime / 9;
		splashImg.color = new Color(splashImg.color.r, splashImg.color.g, splashImg.color.b, Mathf.Lerp (1.5f,0, splashTimer));
		if (splashTimer > 1.1f) {
			Application.LoadLevel(1);
		}

		as1.volume = Mathf.Lerp (0.2f, 0, Mathf.Abs (splashTimer));
		as2.volume = Mathf.Lerp (0.1f, 0, Mathf.Abs (splashTimer));
		as3.volume = Mathf.Lerp (0.2f, 0, Mathf.Abs (splashTimer));

		rocket.position = Vector3.Lerp (rocketStartPos, rocketTarg.position, rocketLerp);
	}
}
