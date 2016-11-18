using UnityEngine;
using System.Collections;

public class PlayerAnimation : PlayerManager {

	public Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		PlayerAnimate ();
	}

	//method for all of the player's animations
	void PlayerAnimate () {
		if (Input.GetKeyDown("1")) {

			Debug.Log ("Running Animation");
		}
		
	}
}