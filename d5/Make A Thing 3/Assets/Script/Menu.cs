using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

    public GameObject playerObj;
    public PlayerMovement playerMainScript;
    public IntroGravity gravScript;
    public GameObject inGameUI;
    public GameObject menuUI;
    public GameObject explosions;
    public GameObject mainCam;
    Camera thisCam;
    public Camera cam2;
    public bool started = false;

    AudioSource thisAudio;
    public GameObject shipDisplay;
    public Text shipDisplayText;
    float letterPause = 0.05f;
    public AudioClip typeSound1;
    public AudioClip typeSound2;
    string message;
    int charCount = 0;

    public GameObject controlsPanel;
    public Text controlsText;
    string controlsMessage;
	bool canSkip = true;
	public AudioListener playerListener;

    void Start()
    {
        thisAudio = GetComponent<AudioSource>();
        thisCam = GetComponent<Camera>();
        Cursor.visible = true;
        message = "Engine state: critical...\nHull state: critical...\nLife support systems: critical...\n\n...\n...\n\nComet reached: Charlie Alpha Three Eight.\nLife support resource detected. Ice. Suit contact required...\nSuit regenerative power activated...\nRescue signal activated...\n...\n\nEngine state: critical... \nHull st";
        controlsMessage = "...\nSK-1 PC Suit... WASD Thrusters... SPACE Vertical booster...\n...\nGemini Xbox Suit... L Thumbstick Thrusters... A - Vertical booster...\n...\nPS-Mobility Suit... L Thumbstick Thrusters... X - Vertical booster...\n...";
    }

    void Controls()
    {
        if (!controlsPanel.activeSelf)
        {
            controlsPanel.SetActive(true);
            StartCoroutine(TypeControlsText());
        } else
        {
            StopAllCoroutines();
            controlsPanel.SetActive(false);
            controlsText.text = "";
        }
            
    }

	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (!started){
				Application.Quit();
			} else {
				Skip();
			}
		}

		if (Input.GetButton("Button0")){
			if (!started){
				StartGame ();
			} else if (charCount > 40 && canSkip){
				Skip ();
			}
		}

	}

	void QuitGame () {
		Application.Quit();
	}

	IEnumerator TypeText () {
        charCount = 0;
        char[] messageArray = message.ToCharArray ();
		foreach (char letter in messageArray) {
			charCount++;
			shipDisplayText.text += letter;
			if (letter != ' ' && letter != '\n' && letter != '.'){
				thisAudio.PlayOneShot(typeSound1, 0.6f);
			}
			if (charCount == messageArray.Length){
				thisAudio.clip = typeSound2;
				thisAudio.Play();
				Invoke("Explode", 1f);
			}
			yield return 0;
			yield return new WaitForSeconds (letterPause);
		}
	}

    IEnumerator TypeControlsText()
    {
        charCount = 0;
        char[] messageArray2 = controlsMessage.ToCharArray();
        foreach (char letter in messageArray2)
        {
            charCount++;
            controlsText.text += letter;
            if (letter != ' ' && letter != '\n' && letter != '.')
            {
                thisAudio.PlayOneShot(typeSound1, 0.6f);
            }
            yield return 0;
            yield return new WaitForSeconds(0.025f);
        }
    }

    void StartGame () {
        StopAllCoroutines();
		Cursor.visible = false;
        controlsPanel.SetActive(false);
        started = true;
		shipDisplay.SetActive (true);
		StartCoroutine(TypeText ());
	}

	void Explode(){
		canSkip = false;
		thisAudio.Stop();
		shipDisplay.SetActive (false);;
		menuUI.SetActive(false);
		explosions.SetActive(true);
		playerObj.SetActive(true);
		Invoke("Intro", 12f);
		Invoke("Cam2", 7.5f);
	}

	void Cam2(){
		cam2.enabled = true;
		thisCam.enabled = false;
	}

	void Intro(){
        mainCam.SetActive(true);
		playerListener.enabled = true;
		gameObject.GetComponent<AudioListener>().enabled = false;
		cam2.enabled = false;
        Invoke("EnableCharacter", 3f);
    }

    void EnableCharacter()
    {
        gravScript.enabled = false;
        playerMainScript.enabled = true;
        inGameUI.SetActive(true);
    }

	void Skip(){
		StopAllCoroutines ();
		Explode ();
    }

    void EnableControls()
    {
        gravScript.enabled = false;
        playerMainScript.enabled = true;
        inGameUI.SetActive(true);
		this.enabled = false;
    }
}
