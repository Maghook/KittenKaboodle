using UnityEngine;
using System.Collections;
using System.Collections.Generic; //create HashSet to store all passengers on a single platform

public class PlatformController : RaycastController {

	public LayerMask passengerMask;

	public Vector3[] localWaypoints; //array that keep reference to multiple waypoints
	Vector3[] globalWaypoints; //array of global waypoints

	public float speed; //speed of the platform
	public bool cyclic; //allows a platform to cycle through its waypoint system
	public float waitTime; //amount of time to pause at each waypoint
	[Range(0,2)] //clamp ease range between 0 and 2 (+ 1), ultimately between 1 and 3
	public float easeAmount; //amount to ease in/out platforms between waypoints

	int fromWaypointIndex; //index of the gobal waypoint to move away from
	float percentBetweenWaypoints; //percent moved between the two waypoints, between 0 and 1
	float nextMoveTime; //

	List<PassengerMovement> passengerMovement; //list to store PassengerMovement struct
	Dictionary<Transform,PlayerController> passengerDictionary = new Dictionary<Transform,PlayerController>(); //create dictionary to reduce GetComponent calls to help with optimisation

	// Use this for initialization
	public override void Start () {
		base.Start ();
	
		//keep platform waypoint still so that the platform can reach the waypoint, rather than the waypoint moving with the platform
		globalWaypoints = new Vector3[localWaypoints.Length];
		for (int i = 0; i < localWaypoints.Length; i++) {
			globalWaypoints[i] = localWaypoints[i] + transform.position; //waypoints to move between
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateRaycastOrigins ();

		Vector3 velocity = CalculatePlatformMovement();

		CalculatePassengerMovement (velocity);

		//move passenger before or after platform, depending on platform direction
		MovePassengers (true);
		transform.Translate (velocity);
		MovePassengers (false);
	}

	//platform movement smoothing, ease in/out
	float Ease(float x) {
		float a = easeAmount + 1; //forces ease value of 0 to be equal to 1
		return Mathf.Pow (x, a) / (Mathf.Pow (x, a) + Mathf.Pow (1 - x, a));
	}

	//method to move platform between waypoints - which waypoint we are moving away/toward, and the percentage of distance between the two
	Vector3 CalculatePlatformMovement() {
		if (Time.time < nextMoveTime) {
			return Vector3.zero;
		}

		fromWaypointIndex %= globalWaypoints.Length; //reset to 0 as to not go out of bounds
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance (globalWaypoints [fromWaypointIndex], globalWaypoints [toWaypointIndex]); //distance between two waypoints
		percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints; //increase the percentage more slowly each frame, depending on distance of waypoints, by a constant rate
		percentBetweenWaypoints = Mathf.Clamp01 (percentBetweenWaypoints); //make sure to clamp ease function value between 0 and 1
		float easedPercentBetweenWaypoints = Ease (percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp (globalWaypoints [fromWaypointIndex], globalWaypoints [toWaypointIndex], easedPercentBetweenWaypoints); //find the point between from/to waypoints based on percentage of distance
	
		//what happens when the next waypoint is reached
		if (percentBetweenWaypoints >= 1) {
			percentBetweenWaypoints = 0; //reset percentage
			fromWaypointIndex++; //continue to the next set of waypoint

			//check that the next waypoint is not outside of the array and determine the end of the array
			//only allowed if the waypoint system does not cycle
			if (!cyclic) {
				if (fromWaypointIndex >= globalWaypoints.Length - 1) {
					fromWaypointIndex = 0; //start again at the beginning
					System.Array.Reverse(globalWaypoints); //reverse the array of waypoints to retrace its steps
				}
			}

			nextMoveTime = Time.time + waitTime;

		}

		return newPos - transform.position; //return the amount of distance to move each frame
	}

	//method to allow passengers to move on platforms
	void MovePassengers(bool beforeMovePlatform){
		foreach (PassengerMovement passenger in passengerMovement) {
			//check if passenger is not contained in the dictionary
			if (!passengerDictionary.ContainsKey (passenger.transform)) {
				passengerDictionary.Add (passenger.transform, passenger.transform.GetComponent<PlayerController> ()); //if not, add passenger to the dictionary
			}

			if (passenger.moveBeforePlatform == beforeMovePlatform) {
				passengerDictionary[passenger.transform].Move (passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}

	//anything moved by a platform is called a 'Passenger'
	void CalculatePassengerMovement(Vector3 velocity) {
		//allow multiple passengers to move on a platform
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		passengerMovement = new List<PassengerMovement> ();

		float directionX = Mathf.Sign (velocity.x);
		float directionY = Mathf.Sign (velocity.y);

		//vertically moving platform
		if (velocity.y != 0) {
			float rayLength = Mathf.Abs (velocity.y) + skinWidth; //get the length of the ray, forcing it to be positive (Abs)

			for (int i = 0; i < verticalRaycount; i++) {
				//determine which direction the platform is moving
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
					Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.green); //test that the rays are being cast

				//figure out how far to move the passenger
				if (hit) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform); //each passenger only moves one time per frame

						float pushX = (directionY == 1) ? velocity.x : 0; //only move horizontally if grounded on platform
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

						//if platform moves up then passenger moves first
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX,pushY), directionY == 1, true));
					}
				}
			}
		}

		//horizontally moving platform
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinWidth; //get the length of the ray, forcing it to be positive (Abs)

			for (int i = 0; i < horizontalRaycount; i++) {
				//determine which direction the platform is moving
				Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
					Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.green); //test that the rays are being cast

				//figure out how far to move the passenger
				if (hit) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform); //each passenger only moves one time per frame

						float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
						float pushY = -skinWidth; //add a small downward force to allow player to jump while being pushed from the side 


						//if platform moves horizontally then passenger moves first
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX,pushY), false, true));
					}
				}			
			}
		}

		//horizontally or downward moving platform
		//if a passenger is on top of an x/y moving platform then cast a ray upwards
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
			float rayLength = skinWidth * 2;

			for (int i = 0; i < verticalRaycount; i++) {

				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up, rayLength, passengerMask);
				Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.green); //test that the rays are being cast

				//figure out how far to move the passenger
				if (hit) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform); //each passenger only moves one time per frame

						float pushX = velocity.x;
						float pushY = velocity.y;


						//if platform moves horizontally then either moves first, if downward then platform moves first
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX,pushY), true, false));
					}
				}
			}
		}
	}

	//store passenger movement to use in MovePassengers method
	struct PassengerMovement {
		public Transform transform; //transform of the passenger
		public Vector3 velocity; //desired velocity of the passenger
		public bool standingOnPlatform; //whether or not passenger is standing on platform
		public bool moveBeforePlatform; //whether or not to move passenger before platform is moved

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform) {
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

	//visualise the platform waypoints with gizmos
	void OnDrawGizmos() {
		if (localWaypoints != null) {
			Gizmos.color = Color.red;
			float size = 0.3f;

			//convert the local position into a global position to draw the gizmos
			for (int i = 0; i < localWaypoints.Length; i++) {
				Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints [i] + transform.position; //allow waypoint position to move relative with platform position when the game is not playing
				Gizmos.DrawLine (globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size); //draws a vertical line for the waypoint gizmos
				Gizmos.DrawLine (globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size); //draws a horizontal line for the waypoint gizmos
			}
		}
	}

}
