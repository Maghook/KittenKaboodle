using UnityEngine;
using System.Collections;

public class PlayerController : RaycastController {
	
	public float maxClimbAngle = 90; //prevent the player from climbing slopes any higher than this angle in degrees
	public float maxDescendAngle = 90; //player falls when descending an angle higher than this

	public CollisionInfo collisions;

	public override void Start() {
		base.Start ();
		collisions.faceDir = 1; //faceDir starting value is facing right
	}

	//method to detect collisions
	public void Move(Vector3 velocity, bool standingOnPlatform = false){
		UpdateRaycastOrigins ();
		collisions.Reset (); //creates a blank slate for collision detection each time
		collisions.velocityOld = velocity;

		//update the faceDir value
		if (velocity.x != 0) {
			collisions.faceDir = (int)Mathf.Sign (velocity.x);
		}

		if (velocity.y < 0) {
			DescendSlope (ref velocity);
		}

		//if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		//}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity);

		//allow jumping on upward moving platforms
		if (standingOnPlatform) {
			collisions.below = true;
		}
	}

	//method to detect horizontal collisions
	void HorizontalCollisions(ref Vector3 velocity){
		float directionX = collisions.faceDir; //get the direction of the X velocity - left = -1, right = +1
		float rayLength = Mathf.Abs (velocity.x) + skinWidth; //get the length of the ray, forcing it to be positive (Abs)

		//second skinWidth used to detect collisions with walls
		if (Mathf.Abs(velocity.x) < skinWidth) {
			rayLength = 2 * skinWidth;
		}

		for (int i = 0; i < horizontalRaycount; i++) {
			//determine which direction the player is moving
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
				Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red); //test that the rays are being cast

			//if the raycast hits something then set vertical velocity equal to movement amount
			if (hit) {

				//allow player to have unrestrained movement as a platform passes through them
				//causes the ray to skip ahead to the next ray and use that to determine collisions
				if (hit.distance == 0) {
					continue;
				}

				//set movement speed when ascending or descending slopes
				//determine the angle between the objects 'normal' angle and the global up 'normal' angle
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (i == 0 && slopeAngle <= maxClimbAngle) {
						//Debug.Log (slopeAngle);
					//statement to check that a player is between slope angles, if true then prevent slowing horizontal velocity
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					//allow player bounds to reach the edge of slope before climbing
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance -= skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				//only check the rays when not climbing a slope
				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance; //stop player from clipping through objects when the individual raycast distances change

					//prevent bouncing behaviour when colliding with the side of objects on a slope
					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
					}

					collisions.left = directionX == -1; //if player has hit a left-side object while moving left then set collision boolean to true
					collisions.right = directionX == 1; //if player has hit a right-side object while moving right then set collision boolean to true
				}
			}
		}
	}

	//method to detect vertical collisions
	void VerticalCollisions(ref Vector3 velocity){
		float directionY = Mathf.Sign (velocity.y); //get the direction of the Y velocity - down = -1, up = +1
		float rayLength = Mathf.Abs (velocity.y) + skinWidth; //get the length of the ray, forcing it to be positive (Abs)
		Vector2 rayOrigin;
		RaycastHit2D hit;

		for (int i = 0; i < verticalRaycount; i++) {
			//determine which direction the player is moving
			rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
				Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red); //test that the rays are being cast

			//if the raycast hits something then set y velocity equal to movement amount
			if (hit) {

				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance; //stop player from clipping through objects when the individual raycast distances change

				//prevent bouncing behaviour when colliding with the underside of objects on a slope
				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}

				collisions.below = directionY == -1; //if player has hit a bottom-side object while moving down then set collision boolean to true
				collisions.above = directionY == 1; //if player has hit a top-side object while moving up then set collision boolean to true
			}

				//prevent player from being stuck in the corner of slopes
				if (collisions.climbingSlope) {
				float directionX = Mathf.Sign (velocity.x);
				rayLength = Mathf.Abs (velocity.x) + skinWidth;
				//Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
				rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
				//RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
				hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

				if (hit) {
					float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
					if (slopeAngle != collisions.slopeAngle) {
						velocity.x = (hit.distance - skinWidth) * directionX;
						collisions.slopeAngle = slopeAngle;
					}
				}
			}
		}
	}

	//method for climbing slopes
	void ClimbSlope(ref Vector3 velocity, float slopeAngle){
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y > climbVelocityY) {
			//Debug.Log ("Jumping on slope");
		} else {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
			collisions.below = true; //assume that player is grounded when climbing slopes, fix bug that prevents player from jumping on slopes
			collisions.climbingSlope = true; //set struct CollisionInfo boolean to true
			collisions.slopeAngle = slopeAngle;
		}
	}

	//method for descending slopes
	void DescendSlope (ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		//cast a ray downwards and if the player is moving left then start at the bottom right, and vice versa
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		//determine slope angle when ray hits an object and when surface isn't flat
		if (hit) {
			float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign (hit.normal.x) == directionX) {
					//check that the player is cose enough to the slope before taking effect
					if (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) + Mathf.Abs (velocity.x)) {
						float moveDistance = Mathf.Abs (velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
						velocity.y -= descendVelocityY;

						//update collisions
						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

	//detect collisions above, below, left, right of player
	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public bool descendingSlope;
		public float slopeAngle, slopeAngleOld; //store current and previous slope angles
		public Vector3 velocityOld; //store previous velocity to prevent slowing down when immediately descending and ascending slopes
		public int faceDir; //determine which direction the character is facing, +1 for right and -1 for left

		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0; //reset the slope angle
		}
	}
}