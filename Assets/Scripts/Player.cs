using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private float inputDirection; //stores value of movement on X axis for "Horizontal" input
	private float verticalVelocity; //stores value of velocity on Y axis for "Vertical" gravity

	public float speed = 5.0f; //directional speed of player movement on X axis
	public float gravity = 30.0f; //stores value for gravity of "verticalVelocity"
	public float jumpForce = 10.0f; //amount of velocity applied to jumping
	private bool doubleJump = false; //boolean to set how many consecutive double jumps allowed
	private bool wallConfirm; //boolean to allow wall jumping lock and freedom of movement
	public float groundedBuffer = 0.2f; //creates buffer zone underneath player to keep them grounded

	private Vector3 moveVector; //allows movement on X,Y,Z axes
	private Vector3 lastMotion; //copy previous movement for direction locking whilst jumping
	private CharacterController controller; //create a reference to the CharacterController to link player movement

	// Use this for initialization
	void Start () {
		Debug.Log ("Player Script Loaded");

		controller = GetComponent<CharacterController>(); //points the controller float to the player's CharacterController
	}
	
	// Update is called once per frame
	void Update () {

		IsControllerGrounded (); //causes ray to be cast every frame from private bool below

		moveVector = Vector3.zero;

		inputDirection = Input.GetAxis ("Horizontal") * speed; //sets inputDirection to horizontal movement on X axis multiplied by "speed" variable
			//Debug.Log(inputDirection);

		//boolean to check if player is grounded in order to determine verticalVelocity
		if (IsControllerGrounded()) {
			verticalVelocity = 0; //0 velocity when grounded = standing
			//make the player jump
			if (Input.GetKeyDown(KeyCode.Space)) {
				verticalVelocity = jumpForce; //velocity when grounded = jumping
				doubleJump = true; //if grounded, allow double jumping
					Debug.Log ("Player Has Jumped");
			}
			moveVector.x = inputDirection;
		} else {
			//make the player double jump
			if (Input.GetKeyDown(KeyCode.Space)) {
				if (doubleJump) {
					verticalVelocity = jumpForce; //velocity when not grounded = double jumping
					doubleJump = false; //if already jumping, disallow double jumping
						Debug.Log ("Player Has Double Jumped");
				}
			}
			verticalVelocity -= gravity * Time.deltaTime; //-1 velocity when not grounded = falling
			moveVector.x = lastMotion.x;
		}
		moveVector.y = verticalVelocity;
		//moveVector = new Vector3(inputDirection,verticalVelocity,0); //allow player movement every frame on X,Y,Z axes
		controller.Move(moveVector * Time.deltaTime); //movement tied to time and not framerate
		lastMotion = moveVector; //lock jump direction
			//Debug.Log(moveVector);
	}

	//cause the player to be grounded so that physics stop jittery movement, preventing consistent jumping
	private bool IsControllerGrounded() {
		//draws rays from left to right in the center of the player's body
		Vector3 leftRayStart;
		Vector3 rightRayStart;
		leftRayStart = controller.bounds.center;
		rightRayStart = controller.bounds.center;
		leftRayStart.x -= controller.bounds.extents.x;
		rightRayStart.x += controller.bounds.extents.x;
		Debug.DrawRay (leftRayStart,Vector3.down,Color.red);
		Debug.DrawRay (rightRayStart,Vector3.down,Color.green);

		if (Physics.Raycast (leftRayStart, Vector3.down, (controller.height / 2) + groundedBuffer))
			return true;
		if (Physics.Raycast (rightRayStart, Vector3.down, (controller.height / 2) + groundedBuffer))
			return true;
		
		return false;
	}

	//call and record collision data, between player and objects
	private void OnControllerColliderHit(ControllerColliderHit hit) {
		//Debug.Log ("Player Has Collided With " + hit.gameObject.name);
		if (controller.collisionFlags == CollisionFlags.Sides) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				Debug.DrawRay (hit.point, hit.normal,Color.red,2.0f); //draws gizmo rays that appear in debug when colliding with sides
				moveVector = hit.normal * speed;
				verticalVelocity = jumpForce;
				doubleJump = true;
			}
		}
		//create a switch statement so the player can collect coins
		switch (hit.gameObject.tag) {
		case "Coin":
			LevelManager.Instance.CollectPickUp (); //call CollectPickUp instance from LevelManager
			Destroy (hit.gameObject); //destroys coins that the player collides with
			Debug.Log("Coin Collected");
			break;
		case "JumpPad":
			verticalVelocity = jumpForce * 2; //cause jump velocity to multiply when player collides with JumpPad
			doubleJump = true;
			break;
		case "Teleporter":
			transform.position = hit.transform.GetChild(0).position; //teleports player to the teleporter's waypoint
			break;
		case "WinBox":
			LevelManager.Instance.Win(); //activate the Win insgance in the LevelManager script
			break;
		default:
			break;
		}
	}
}