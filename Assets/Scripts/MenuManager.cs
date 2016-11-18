using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public Text scoreText;
	public Text timeText;
	public GameObject howToPlay;

	private int score;
	private float timer;

	bool reset;

	// Use this for initialization
	private void Start () {
		score = PlayerPrefs.GetInt ("PlayerScore");
		scoreText.text = "Highscore : " + score.ToString ();

		timer = PlayerPrefs.GetFloat ("PlayerTime");
		timeText.text = "Best Time : " + timer.ToString ();
	}

	// Update is called once per frame
	void Update () {
		if (reset) {
			score = PlayerPrefs.GetInt ("PlayerScore");
			scoreText.text = "Highscore : " + score.ToString ();

			timer = PlayerPrefs.GetFloat ("PlayerTime");
			timeText.text = "Best Time : " + timer.ToString ();
			reset = false;
		}
	}

	public void HowToPlay() {
		if (!howToPlay.activeInHierarchy) {
			howToPlay.SetActive (true);
		} else {
			howToPlay.SetActive (false);
		}
	}

	public void Level01(){
		SceneManager.LoadScene ("01_Level01");
	}

	public void Level02(){
		SceneManager.LoadScene ("01_Level02");
	}

	public void Level03(){
		SceneManager.LoadScene ("01_Level03");
	}

	public void ResetScore() {
		reset = true;
			//Debug.Log ("Mouse Click");
		if (score >= 0) {
			PlayerPrefs.SetInt ("PlayerScore", 0);
				//Debug.Log ("Reset Score");
		}

		if (timer >= 0) {
			PlayerPrefs.SetFloat ("PlayerTime", 0);
				//Debug.Log ("Reset Time");
		}
	}

	public void QuitGame() {
		Application.Quit();
	}
}
