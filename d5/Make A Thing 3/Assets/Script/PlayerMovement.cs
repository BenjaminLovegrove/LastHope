using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour {

    [Header("Public Variables")]
    public bool playerDead = false;
    public float playTime;
	public Transform centerObj;
	public Transform camCenterObj;
	public float speed = 7.5f;
	public float jumpForce = 0.005f;
	public float controlSpeed = 12f;
	public float jetpackSpeed = 5f;
	public float airLossTime = 60;
	public float powerLossMod = 10;
	public float powerRegenMod = 5;
	public float airGain = 0.2f;

    [Header("Resources/UI")]
    public float airLeft = 1f;
	public float powerLeft = 1f;
	public GameObject airBar;
	public GameObject powerBar;
	Image powerBarIMG;
	float timeSinceBoost = 2f;
	public GameObject UIcanvas;
	public Image fadeToBlack;
	float timeSinceAir = 1;
	float airGainTimer = 1f;
	public Text timerDisplay;
	public Text clickToContinue;
	bool powerWarningTrig = false;
	bool powerOutTrig = false;
	bool powerRegenTrig = true;
	bool depleted = false;
	public Image warningEnergy;
	public Image warningLife;
	bool warningEnergyOn;
	bool warningLifeOn;
	bool warningLifeUp = true;
	bool warningEnergyUp = true;
	public Image energyDrain;
    public Image airGainGlow;

    [Header("Components")]
    Rigidbody rb;
	Animation animator;
	public GameObject rocketPart1;
	public GameObject rocketPart2;
	public ParticleSystem smokeTrail;
    public ParticleSystem smokeTrailLeft;
    public ParticleSystem smokeTrailRight;

    [Header("Audio")]
    public AudioSource SFX2d;
    public AudioSource SFX3d;
    public AudioSource BGM;
    float BGMMaxVol;
    public AudioSource breathing;
//    float breathingMaxVol;

    [Header("SFX")]
    public AudioClip jumpGrass;
    public AudioClip jumpGem;
    public AudioClip airGained;
    public AudioClip shieldWarning;
    public AudioClip shieldDepleted;
    public AudioClip shieldRegen;
    public AudioSource movementJet;
    public AudioSource movementRocket;
    public AudioClip startShield;
    
    //Local variables
	float deathLerpTimer = 1f;
	bool restartGame = false;
	bool jumped = false;
	bool landed = false;
	bool falling = true;
	float prevDist = 999;
	float slowDown = 1;
	float forwardSpeed = 1;

    [Header("Camera Movement")]
    public Transform cameraPosUp;
	public Transform cameraPosDown;
	Transform mainCam;
	float camLerpTimer = 0;

	[Header("Rescue Script")]
	public RescueShip rescueScipt;
	public LookAt lookAtScr;
	public bool rescued = false;
	Vector3 camEndStartPos;
	float endLerp = 0;
	float startFade;

	[Header("Difficulty Scaling")]
	bool reach60 = false;
	bool toggle60 = false;
	bool reach40 = false;
	bool toggle40 = false;
	bool reach20 = false;
	bool toggle20 = false;
	bool easy1 = false;
	bool easy2 = false;
	bool firstTouch = false;

	float startEndLerp;
	
	void Start () {
		rb = GetComponent<Rigidbody> ();
		animator = GetComponentInChildren<Animation> ();
		mainCam = Camera.main.transform;
//        breathingMaxVol = breathing.volume;
		BGMMaxVol = BGM.volume;
		UIcanvas.SetActive (true);
		powerBarIMG = powerBar.GetComponent<Image> ();
        timerDisplay.color = new Color(timerDisplay.color.r, timerDisplay.color.g, timerDisplay.color.b, 0);
        clickToContinue.color = new Color(clickToContinue.color.r, clickToContinue.color.g, clickToContinue.color.b, 0);
        SFX2d.PlayOneShot(startShield);
        SFX2d.PlayOneShot(airGained, 1);
		lookAtScr = GetComponent<LookAt> ();
		rescueScipt.SendMessage ("StartGame");
    }

	void Update () {

		//Cheat for presentation
		if ((Input.GetKey (KeyCode.P)) && (airLeft < 0.95f)) {
			airLeft += Time.deltaTime;
		}

		if (!playerDead) {
			if (!rescued){
				playTime += Time.deltaTime;
				CameraMovement ();
				UpdateResources ();
				ResourceSFX();
				DifficultyScaling();
			}

			if (rescueScipt.gameTime > 0.85f && transform.position.z < 0) {
				Rescue ();

				if (!rescued){
					//camEndStartPos = mainCam.transform.localPosition;
					startFade = fadeToBlack.color.a;
					mainCam.transform.parent = null;
					Invoke ("CharacterRescue", 2f);
					rescued = true;
				}
			}


		} else {
			Death ();
		}

		if (!playerDead && !rescued) {
			//Death feedback
			airBar.GetComponent<Image> ().color = new Color (Mathf.Abs (Mathf.Lerp (1.5f, 0, airLeft)), Mathf.Abs (Mathf.Lerp (0, 1, airLeft)), Mathf.Abs (Mathf.Lerp (0, 1, airLeft)));
			//lerp in beeping
			BGM.volume = Mathf.Lerp (0f, BGMMaxVol, airLeft);
            breathing.volume = Mathf.Lerp(0.6f, -0.3f, airLeft);
		}

		if (!rescued) {
			fadeToBlack.color = new Color (fadeToBlack.color.r, fadeToBlack.color.g, fadeToBlack.color.b, Mathf.Lerp (1.15f, 0, airLeft * 2));
		}

		if (airLeft < 0.05f && !playerDead) {
            breathing.Stop();
			playerDead = true;
			deathLerpTimer = 0f;
		}

		//CD on air gain to avoid multiple raycast hits
		timeSinceAir += Time.deltaTime;

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.LoadLevel(Application.loadedLevel);
		}
	}

	void UpdateResources(){
		//Remove air
		airLeft -= Time.deltaTime / airLossTime;

		//Add power
		if (powerLeft < 1 && timeSinceBoost > 1f) {
			powerLeft += Time.deltaTime / powerRegenMod;
			powerWarningTrig = false;
		}
		timeSinceBoost += Time.deltaTime / 2.5f;

		if (powerLeft < 0.05f) {
			depleted = true;
		}
		if (powerLeft > 0.5f) {
			depleted = false;
		}

		//Set bars
		if (airLeft > 0.05f) {
			airBar.transform.localScale = new Vector3 (airBar.transform.localScale.x, Mathf.Clamp(airLeft,0,1), airBar.transform.localScale.z);
		}
		powerBar.transform.localScale = new Vector3 (powerBar.transform.localScale.x, powerLeft, powerBar.transform.localScale.z);

		if (airLeft > 1) {
			airLeft = 1;
		}

		if (airGainTimer < 1) {
			airGainTimer += Time.deltaTime;
			airLeft += Time.deltaTime * 0.2f;
		}

        //Lerp out airgainglow
        if (timeSinceAir * 3 < 1)
        {
            airGainGlow.color = new Color(airGainGlow.color.r, airGainGlow.color.g, airGainGlow.color.b, Mathf.Lerp(airGainGlow.color.a, 1, Time.deltaTime * 3));
        }
        else
        {
            airGainGlow.color = new Color(airGainGlow.color.r, airGainGlow.color.g, airGainGlow.color.b, Mathf.Lerp(airGainGlow.color.a, 0, Time.deltaTime));
        }
	}

	void ResourceSFX(){
		if (powerLeft < 0.3f && timeSinceBoost < 0.1f && powerWarningTrig == false) {
			powerWarningTrig = true;
			SFX2d.PlayOneShot(shieldWarning);
		}
		if (powerLeft < 0.1 && !powerOutTrig) {
			powerOutTrig = true;
			SFX2d.PlayOneShot(shieldDepleted);
		}
		if (powerLeft > 0.15f) {
			powerOutTrig = false;
		}
		if (timeSinceBoost > 1f && !powerRegenTrig) {
			powerRegenTrig = true;
			SFX2d.PlayOneShot(shieldRegen, 0.85f);
		}
		if (timeSinceBoost < 1) {
			powerRegenTrig = false;
		}

		//WARNING LIGHTS ON
		if (airLeft < 0.4f) {
			warningLifeOn = true;
		} else {
			warningEnergyOn = false;
		}

		if (powerLeft < 0.3f) {
			warningEnergyOn = true;
		} else {
			warningEnergyOn = false;
		}

		//WARNING LIGHTS LERP
		if (warningLifeOn) {
			if (warningLifeUp) {
				warningLife.color = new Color (warningLife.color.r, warningLife.color.g, warningLife.color.b, Mathf.Lerp (warningLife.color.a, 1, Time.deltaTime));
				if (warningLife.color.a > 0.9f) {
					warningLifeUp = false;
				}
			} else {
				warningLife.color = new Color (warningLife.color.r, warningLife.color.g, warningLife.color.b, Mathf.Lerp (warningLife.color.a, 0.1f, Time.deltaTime));
				if (warningLife.color.a < 0.2f) {
					warningLifeUp = true;
				}
			}
		} else {
			warningLife.color = new Color (warningLife.color.r, warningLife.color.g, warningLife.color.b, Mathf.Lerp (warningLife.color.a, 0, Time.deltaTime));
			warningLifeUp = true;
		}

		if (warningEnergyOn) {
			if (warningEnergyUp) {
				warningEnergy.color = new Color (warningEnergy.color.r, warningEnergy.color.g, warningEnergy.color.b, Mathf.Lerp (warningEnergy.color.a, 1, Time.deltaTime));
				if (warningEnergy.color.a > 0.9f) {
					warningEnergyUp = false;
				}
			} else {
				warningEnergy.color = new Color (warningEnergy.color.r, warningEnergy.color.g, warningEnergy.color.b, Mathf.Lerp (warningEnergy.color.a, 0.1f, Time.deltaTime));
				if (warningEnergy.color.a < 0.2f) {
					warningEnergyUp = true;
				}
			}
		} else {
			warningEnergy.color = new Color (warningEnergy.color.r, warningEnergy.color.g, warningEnergy.color.b, Mathf.Lerp (warningEnergy.color.a, 0, Time.deltaTime));
			warningEnergyUp = true;
		}
	}

	void FixedUpdate(){
		Gravity ();
		if (!playerDead && !rescued) {
			GroundJump ();
			if (!depleted){
				Controls ();
				powerBarIMG.color = new Color(255,210,0);
			} else {
				rocketPart1.SetActive (false);
				rocketPart2.SetActive (false);
				movementRocket.volume = 0;
				smokeTrail.Stop();
				movementJet.volume = 0;
				powerBarIMG.color = new Color(255,0,0);
			}
		}
	}

	void Controls(){
		if (Input.GetKey (KeyCode.Space) || Input.GetButton("Button0")) {
			timeSinceBoost = 0f;
			if (powerLeft > 0.05f){
				powerLeft -= Time.deltaTime / (powerLossMod / 3.5f);
			} 
			if (powerLeft > 0.1f) {
				forwardSpeed = 2;
				rb.AddForce ((transform.position + (transform.up * jetpackSpeed)), ForceMode.Acceleration);

				movementRocket.volume = Mathf.Lerp(movementRocket.volume, 0.8f, Time.deltaTime * 2);
                if (!movementRocket.isPlaying)
                {
                    movementRocket.Play();
                }
				if (!rocketPart1.activeSelf) {
					rocketPart1.SetActive (true);
					rocketPart2.SetActive (true);
				}
			} else {
				rocketPart1.SetActive (false);
				rocketPart2.SetActive (false);
				movementRocket.volume = 0;
                forwardSpeed = 1;
			}
		} else {
			rocketPart1.SetActive (false);
			rocketPart2.SetActive (false);
			movementRocket.volume = 0;
            forwardSpeed = 1;
		}

		if (powerLeft > 0.1f){
			if (Input.GetKey (KeyCode.W) || (Input.GetAxis("Vertical") > 0.15f)) {
				if (!firstTouch){
					slowDown = 1f;
				} else {
					slowDown = 2.5f;
				}
				if (Input.GetAxis("Vertical") > 0.15f){
					rb.AddForce ((transform.up * controlSpeed) * Input.GetAxis("Vertical"), ForceMode.Acceleration);
				} else {
					rb.AddForce ((transform.up * controlSpeed), ForceMode.Acceleration);
				}

			} else if (Input.GetKey (KeyCode.S) || (Input.GetAxis("Vertical") < -0.15f)) {
				if (!firstTouch){
					slowDown = 1f;
				} else {
					slowDown = 2.5f;
				}
				if (Input.GetAxis("Vertical") < -0.15f){
					rb.AddForce ((-transform.up * (controlSpeed * 1.5f)) * Mathf.Abs(Input.GetAxis("Vertical")), ForceMode.Acceleration);
				} else {
					rb.AddForce ((-transform.up * (controlSpeed * 1.5f)), ForceMode.Acceleration);
				}
			} else {
				if (!firstTouch){
					slowDown = 1f;
				}
			}
			if (Input.GetKey (KeyCode.A) || (Input.GetAxis("Horizontal") < -0.15f)) {
				if (Input.GetAxis("Horizontal") < -0.15f){
					rb.AddForce ((-transform.right * controlSpeed) * Mathf.Abs(Input.GetAxis("Horizontal")), ForceMode.Acceleration);
				} else {
					rb.AddForce ((-transform.right * controlSpeed), ForceMode.Acceleration);
				}
			}
			if (Input.GetKey (KeyCode.D) || (Input.GetAxis("Horizontal") > 0.15f)) {
				if (Input.GetAxis("Horizontal") > 0.15f){
					rb.AddForce ((transform.right * controlSpeed) * (Input.GetAxis("Horizontal")), ForceMode.Acceleration);
				} else {
					rb.AddForce ((transform.right * controlSpeed), ForceMode.Acceleration);
				}
			}
		}

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0.15f || Input.GetAxis("Horizontal") < -0.15f || Input.GetAxis("Vertical") > 0.15f || Input.GetAxis("Vertical") < -0.15f){
				timeSinceBoost = 0f;
			if (powerLeft > 0.1f){
				smokeTrail.Play();
                if (!movementJet.isPlaying)
                {
                    movementJet.Play();
                }
				movementJet.volume = Mathf.Lerp(movementJet.volume,1,Time.deltaTime * 2);
			} else
            {
				movementJet.volume = 0;
            }
			if (powerLeft > 0.05f) {
				powerLeft -= Time.deltaTime / powerLossMod;
            }
		} else {
			smokeTrail.Stop();
			movementJet.volume = 0;
        }

        if ((Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0.15f) && (powerLeft > 0.1f))
        {
            smokeTrailLeft.Play();   
        } else
        {
            smokeTrailLeft.Stop();
        }
		if ((Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < -0.15f) && (powerLeft > 0.1f))
        {
            smokeTrailRight.Play();
        }
        else
        {
            smokeTrailRight.Stop();
        }

        if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.Space) || depleted) {
			energyDrain.color = new Color(energyDrain.color.r, energyDrain.color.g, energyDrain.color.b, Mathf.Lerp (energyDrain.color.a, 1, Time.deltaTime));
		} else if (Input.GetAxis("Horizontal") > 0.15f || Input.GetAxis("Horizontal") < -0.15f || Input.GetAxis("Vertical") > 0.15f || Input.GetAxis("Vertical") < -0.15f){
			energyDrain.color = new Color(energyDrain.color.r, energyDrain.color.g, energyDrain.color.b, Mathf.Lerp (energyDrain.color.a, 1, Time.deltaTime));
		} else {
			energyDrain.color = new Color(energyDrain.color.r, energyDrain.color.g, energyDrain.color.b, Mathf.Lerp (energyDrain.color.a, 0, Time.deltaTime));
		}
	}

	void Gravity(){
		Vector3 dir = centerObj.transform.position - transform.position;
		dir = dir.normalized;
		if (!rescued) {
			rb.AddForce (dir * ((speed * forwardSpeed) * slowDown));
		} else {
			rb.AddForce ((dir * ((speed * forwardSpeed) * slowDown)) / 2.5f);
		}
		float dist = Vector3.Distance (transform.position, centerObj.position);

		//Increase drag to maximise jump distance and reset land/jump
		if (!firstTouch) {
			slowDown = 0.25f;
			rb.drag = 0.5f;
			rb.angularDrag = 1f;
		}
		if (!falling) {
			if (dist > 120) {
				jumped = false;
				landed = false;
				rb.drag = Mathf.Lerp (0, 1, ((dist - 140) / (160 - 140)));
			}
		} else {
			rb.drag = Mathf.Lerp (0, 1, Time.deltaTime * 2);
		}

		
		//Check if falling by checking current distance against previous update
		if (dist < prevDist) {
			if (falling == false){
				animator.PlayQueued("jumpfall");
			}
			falling = true;
		} else {
			falling = false;
            animator.PlayQueued("jumpfall");
        }
		prevDist = dist;
	}

	void GroundJump(){
		if (!landed) {
			if (Physics.Raycast (transform.position, transform.forward, 7)) {
				animator.Stop();
				animator.Play ("jumpland");
				landed = true;
			}
		}
		if (!jumped) {
            RaycastHit hit;
            if (Physics.Raycast (transform.position, transform.forward, out hit, 4f)) {
				Invoke ("AddForce", 0.05f);
				jumped = true;
                if (hit.collider.gameObject.tag == "Sapphire")
                {
                    AddAir();
                    SFX3d.PlayOneShot(jumpGem);
                } else
                {
                    SFX3d.PlayOneShot(jumpGrass);
                }
            }

		}

	}

	void Death(){
		if (!restartGame) {
			if (deathLerpTimer < 1){
				deathLerpTimer += Time.deltaTime / 12;
			}
			//Lerp out beeping
		} else {
			deathLerpTimer -= Time.deltaTime / 10;
			if (deathLerpTimer < 0.25f || Input.GetMouseButtonDown(0) || Input.GetButtonDown("Button0")){
				Application.LoadLevel(Application.loadedLevel);
			}
		}

		BGM.volume = Mathf.Lerp (0, BGMMaxVol * 0.8f, deathLerpTimer);
		timerDisplay.text = "You survived " + (int)playTime + "s in space...";
		clickToContinue.text = "\n\n\n\nclick to continue...";

		timerDisplay.color = new Color (timerDisplay.color.r, timerDisplay.color.g, timerDisplay.color.b, Mathf.Lerp(-0.25f,1, deathLerpTimer));
		clickToContinue.color = new Color (clickToContinue.color.r, clickToContinue.color.g, clickToContinue.color.b, Mathf.Lerp(-1.5f,1, deathLerpTimer));

		if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Button0")){
			restartGame = true;
		}
	}


	void LevelRestart(){
		Application.LoadLevel (Application.loadedLevel);
	}

	void AddAir(){
		if (timeSinceAir > 1) {
            Invoke("AirSFX", 0.25f);
            airGainTimer = 0f;
			timeSinceAir = 0;
            airGainGlow.color = new Color(airGainGlow.color.r, airGainGlow.color.g, airGainGlow.color.b, 1);

        }
    }

    void AirSFX()
    {
        SFX2d.PlayOneShot(airGained, Mathf.Lerp(1.2f,0,airLeft));
    }

	void AddForce(){
		rb.AddForce ((transform.position + -transform.forward * 3), ForceMode.Impulse);
		animator.Play ("jump");
		animator.PlayQueued("jumpfall");	
	}

	void CameraMovement(){
		if (!rescued) {
			if (!falling) {
				mainCam.position = Vector3.Lerp (mainCam.position, cameraPosUp.position, Time.deltaTime);
				mainCam.rotation = Quaternion.Lerp (mainCam.rotation, cameraPosUp.rotation, Time.deltaTime);
				camLerpTimer = 0;
			} else {
				mainCam.position = Vector3.Lerp (mainCam.position, cameraPosDown.position, Time.deltaTime / 2);
				mainCam.rotation = Quaternion.Lerp (mainCam.rotation, cameraPosDown.rotation, Time.deltaTime / 2);
				camLerpTimer += Time.deltaTime;
			}
		}
	}

	void Rescue(){
		if (!restartGame) {
			BGM.volume = Mathf.Lerp (BGM.volume, BGMMaxVol, Time.deltaTime / 2);
			endLerp += Time.deltaTime;
			if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Button0")){
				startEndLerp = endLerp;
				restartGame = true;
			//	endLerp = 1;
			}
		} else {
			BGM.volume -= Time.deltaTime / 4;
		//	endLerp -= Time.deltaTime;
		//	BGM.volume = Mathf.Lerp (BGM.volume, 0, Time.deltaTime / 2);
		//	endLerp = Mathf.Lerp (startEndLerp, -1f, Time.deltaTime);
			if (endLerp < 0 || Input.GetMouseButtonDown(0) || Input.GetButtonDown("Button0") || BGM.volume < 0.1f){
				Application.LoadLevel(Application.loadedLevel);
			}
		}
		mainCam.transform.rotation = Quaternion.Lerp (mainCam.transform.rotation, (Quaternion.LookRotation ((camCenterObj.position - mainCam.transform.position).normalized)), endLerp * 0.025f);
		rb.angularDrag = 5f;

		fadeToBlack.color = new Color (fadeToBlack.color.r, fadeToBlack.color.g, fadeToBlack.color.b, Mathf.Lerp (startFade, 1, endLerp / 9));
		timerDisplay.text = "You were rescued after " + (int)playTime + "s.";
		clickToContinue.text = "\n\n\n\nclick to continue...";
		timerDisplay.color = new Color (timerDisplay.color.r, timerDisplay.color.g, timerDisplay.color.b, Mathf.Lerp(-0.25f,1, endLerp / 13));
		clickToContinue.color = new Color (clickToContinue.color.r, clickToContinue.color.g, clickToContinue.color.b, Mathf.Lerp(-1.5f,1, endLerp / 13));
		//mainCam.transform.localPosition = new Vector3 (mainCam.transform.localPosition.x, Mathf.Lerp (camEndStartPos.y, camEndStartPos.y - 9, endLerp / 2), mainCam.transform.localPosition.z);
	}

	void CharacterRescue(){
		rb.angularDrag = 0.05f;
		centerObj = rescueScipt.gameObject.transform;
		lookAtScr.Rescue(rescueScipt.gameObject.transform);
	}

	void DifficultyScaling(){
		if (airLeft < 0.5f && !easy1) {
			airGain = 0.225f;
			powerLossMod = powerLossMod * 0.9f;
			easy1 = true;
			airLossTime = airLossTime*1.03f;
		}
		if (airLeft < 0.22f && !easy2) {
			powerLossMod = powerLossMod * 0.75f;
			easy2 = true;
			airLossTime = airLossTime*1.03f;
		}

		if (airLeft < 60){
			reach60 = true;
		}
		if (airLeft < 40){
			reach40 = true;
		}
		if (airLeft < 20){
			reach20 = true;
		}

		if (rescueScipt.gameTime > 0.25f && !reach60){
			toggle60 = true;
			powerLossMod = powerLossMod * 1.1f;
			airLossTime = airLossTime * 0.9f;
		}
		if (rescueScipt.gameTime > 0.6f && !reach40){
			toggle60 = true;
			powerLossMod = powerLossMod * 1.1f;
			airLossTime = airLossTime * 0.9f;
		}
		if (rescueScipt.gameTime > 0.8f && !reach20){
			toggle60 = true;
			powerLossMod = powerLossMod * 1.15f;
			airLossTime = airLossTime * 0.9f;
		}
	}

}
