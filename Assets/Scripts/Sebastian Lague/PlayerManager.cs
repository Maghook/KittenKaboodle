using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PlayerController))]
public class PlayerManager : MonoBehaviour {

	//variables for movement
	float moveSpeed;
	public float walkSpeed = 8.0f;
	public float runSpeed = 16.0f;
	float accelerationTimeAirborne = 0.2f;
	float accelerationTimeGrounded = 0.1f;

	//assign values to jumpHeight and timeToJumpApex variables which control gravity and jumpVelocity variables
	public float maxJumpHeight = 4.0f; //maximum jump distance
	public float minJumpHeight = 1.0f; //minimum jump distance
	public float timeToJumpApex = 0.4f; //amount of time to reach the jump apex

	//variables for wall jumping / sliding
	public float wallSlideSpeedMax = 3.0f; //maximum speed to slide down walls
	public float wallStickTime = 0.25f; //amount of time to stick to the wall in place
	float timeToWallUnstick;
	public Vector2 wallJumpClimb; //stores force for hopping up the wall
	public Vector2 wallJumpOff; //stores force for jumping off the wall
	public Vector2 wallJumpLeap; //stores force for leaping away from wall

	//variables for special moves
	public float groundPoundVelocity = 50.0f;

	//variables for collision objects
	private bool jumpPadCollision = false;

	//variables for physics
	private float gravity;
	private float maxJumpVelocity;
	private float minJumpVelocity;

	//variable booleans
	private bool jump;
	private bool doubleJump;
	private bool airJump;
	private bool canAirJump;
	[HideInInspector]
	public bool isRunning;
	private bool groundPound = false;

	Vector3 velocity; //store the player's velocity

	float velocityXSmoothing; //create a reference to the smooth horizontal movement function

	PlayerController controller;
	Collision2D collision2d;

	// Use this for initialization
	void Start () {
		controller = GetComponent<PlayerController> (); //force script to require component

		//solving velocity of gravity based on kinematic equations
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex; //use Mathf.Abs to set the gravity to a positive value
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight); //calculate minimum jump velocity
			//Debug.Log ("Gravity: " + gravity + " Jump Velocity: " + jumpVelocity);
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical")); //allow detection of horizontal collisions
		int wallDirX = (controller.collisions.left) ? -1 : 1; //equal to -1 if colliding with wall to the left, +1 if to the right

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

		//function to allow wall jumping and sliding
		bool wallSliding = false; //true when collisions detected to the left or right and moving downward
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;
			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}
			//function for temporarily sticking to walls
			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;
				if (input.x != wallDirX && input.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				} else {
					timeToWallUnstick = wallStickTime;
				}
			} else {
				timeToWallUnstick = wallStickTime;
			}
		}

		//workaround to allow jump pad to activate before resetting vertical velocity, otherwise doesn't work
		//still allows air jumping somehow, I don't know, cool stuff
		if (jumpPadCollision) {
			velocity.y = maxJumpVelocity * 2;
			jumpPadCollision = false;
		} else {
			//prevent player's rays from accumulating gravity indefinitely
			if (controller.collisions.above || controller.collisions.below) {
				velocity.y = 0; //reset vertical velocity
			}
		}

		//ground pound when in air
		if (!controller.collisions.below && Input.GetKeyDown (KeyCode.Space) && Input.GetKey (KeyCode.S)) {
			groundPound = true;
			doubleJump = false;
			canAirJump = false;

			if (groundPound) {
				velocity.y = -groundPoundVelocity;
				groundPound = false;
					//Debug.Log ("Ground Pound");
			}
		}

		//ground jump function
		if (controller.collisions.below) {
			jump = true;
			doubleJump = false;
			canAirJump = true;

			if (Input.GetKeyDown (KeyCode.Space) && jump) {
				velocity.y = maxJumpVelocity;
				doubleJump = true;
				canAirJump = false;
					//Debug.Log ("Player has Jumped");
			}
		} else if (doubleJump) {
			jump = false;
			canAirJump = false;

			//allow player to double jump
			if (Input.GetKeyDown (KeyCode.Space) && doubleJump) {
				velocity.y = maxJumpVelocity;
				doubleJump = false;
					//Debug.Log ("Player has Double Jumped");
			} else {
				if (controller.collisions.left || controller.collisions.right) {
					doubleJump = false; //prevent player from double jumping when next to a wall
				}
			}
		} else {
			if (canAirJump) {
				airJump = true;

				//allow player to jump once after falling off an object, this is not the same as a double jump
				if (Input.GetKeyDown (KeyCode.Space) && airJump) {
					if (velocity.y <= 10) {
						velocity.y = maxJumpVelocity;
						canAirJump = false;
							//Debug.Log ("Player has Air Jumped");
					}
				}
			}
		}
			//Debug.Log("Player Can Air Jump: " + canAirJump);

		//wall jump function, causes player to jump slightly up and away from wall
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (wallSliding) {
				//move away in the same direction the wall is facing
				if (wallDirX == input.x) {
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
						//Debug.Log ("Wall Climb");
				//jump off the wall
				} else if (input.x == 0) {
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
						//Debug.Log ("Wall Jump");
				//when input is opposite to the wall's direction
				} else {
					velocity.x = -wallDirX * wallJumpLeap.x;
					velocity.y = wallJumpLeap.y;
					canAirJump = true; //alow player to double jump after wall leap
						//Debug.Log ("Wall Leap");
				}
			}
		}

		//determine variable jump velocity when jump key is released
		if (Input.GetKeyUp (KeyCode.Space)) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}

		//allow running when LeftShift is held down
		if (Input.GetKey (KeyCode.LeftShift)) {
			isRunning = true;
			if (isRunning && Input.GetKey (KeyCode.LeftShift)) {
				moveSpeed = runSpeed;
				//Debug.Log ("Player is running");
			}
		} else {
			isRunning = false;
			if (!isRunning) {
				moveSpeed = walkSpeed;
				//Debug.Log ("Player isn't running");
			}
		}

		//smooth horizontal movement
		velocity.y += gravity * Time.deltaTime; //apply gravity to velocity every frame
		controller.Move(velocity * Time.deltaTime); //cause controller to move the player
	}

	//method for detecting collisions with game objects and their effect
	void OnCollisionEnter2D (Collision2D collision2d) {
		switch (collision2d.gameObject.tag) {
		case "JumpPad":
			jumpPadCollision = true; //set collision with jump pad to true to enable function in Update method
			canAirJump = true;
			break;
		case "PickUp":
			LevelManager.Instance.CollectPickUp (); //add to score by calling CollectPickUp instance from LevelManager
			Destroy (collision2d.gameObject); //destroy the pickup
			break;
		case "Teleporter":
			transform.position = collision2d.transform.GetChild(0).position; //teleports player to the teleporter's waypoint
			break;
		case "WinBox":
			LevelManager.Instance.Win(); //activate the Win instance in the LevelManager script
			break;
		default:
			break;
		}
			//Debug.Log (collision2d.gameObject.tag);
	}

}