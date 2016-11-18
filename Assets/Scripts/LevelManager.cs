using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour {

	public static LevelManager Instance {
		set;
		get;
	}

	private int hitpoint = 3; //how many lives the character has
	private int score = 0; //set player's score to 0 upon loading level
	private int maxScore = 0; //maximum score per level

	public float fallLimit = 0.0f; //how far down the player falls on the Y-axis before losing a life

	public Transform spawnPosition; //location the character spawns when level is loaded or character dies
	public Transform playerTransform; //reference to the transform (location) of the player

	//create references to the text fields of the UI
	public Text scoreText;
	public Text hitpointText;
	public Text timerText;


	//timer variables
	private float timer; //set timer to 0 upon loading level
	private bool timerActive;

	//use Awake() to call functions before Start()
	private void Awake(){
		maxScore = GameObject.FindGameObjectsWithTag("PickUp").Length; //check how many objects are tagged "PickUp"

		Instance = this; //set Instance value to itself
		scoreText.text = "Current Score : " + score.ToString() + " of " + maxScore; //use ToString to convert int value to letters
		hitpointText.text = "Hitpoints : " + hitpoint.ToString();
		timerText.text = "Time : " + timer.ToString ();
	}

	// Use this for initialization
	void Start () {
		timerActive = true;
		timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		//if the player falls below -10 on Y axis then they lose a hitpoint and teleport to the spawnpoint location
		if (playerTransform.position.y < fallLimit) {
				Debug.Log ("Died");
			playerTransform.position = spawnPosition.position;
			hitpoint--; //hitpoints = hitpoints - 1;
			hitpointText.text = "Hitpoints : " + hitpoint.ToString(); //update HitpointText UI
			if (hitpoint <= 0) {
					Debug.Log ("You Lose");
				SceneManager.LoadScene ("00_MainMenu"); //load MainMenu scene upon loss
			}
		}

		//allow level timer to count upward, showing minutes/seconds/fraction of the current playtime
		if (timerActive) {
			timer += Time.deltaTime;

			//use Math.Floor to prevent display glitch of seconds = 60 for half a second before adding to minutes
			float minutes = Mathf.Floor (timer / 60);
			float seconds = Mathf.Floor (timer % 60);
			float fraction = Mathf.Floor ((timer * 100) % 100);

			timerText.text = string.Format ("Time : {0:00} : {1:00} : {2:00}", minutes, seconds, fraction);
		}
	}

	public void Win() {
		if (score > PlayerPrefs.GetInt ("PlayerScore")) {
			PlayerPrefs.SetInt ("PlayerScore", score); //only overrides highscore if score increased
		}
		if (timer > PlayerPrefs.GetFloat ("PlayerTime")) {
			PlayerPrefs.SetFloat ("PlayerTime", timer);
		}
			//Debug.Log ("You Win");
		SceneManager.LoadScene ("00_MainMenu"); //load MainMenu scene upon win
	}
	public void CollectPickUp() {
		score++;
		scoreText.text = "Current Score : " + score.ToString() + " of " + maxScore.ToString(); //update ScoreText UI
	}

	public void QuitToMenu() {
		SceneManager.LoadScene ("00_MainMenu");
	}
}
