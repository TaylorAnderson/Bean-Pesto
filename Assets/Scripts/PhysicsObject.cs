using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

	public float minGroundNormalY = .65f;


	public float gravityModifier = 5f;
	protected float currentGravityModifier;
	protected bool grounded;
	protected float bounciness = 1;
	protected Vector2 groundNormal = Vector2.zero;
	protected Vector2 targetVelocity;
	protected Rigidbody2D rb2d;
	new protected BoxCollider2D collider;
	float radius = 0.5f;
	public Vector2 velocity;

	[HideInInspector]
	public bool useCollision = true;

	protected ContactFilter2D contactFilter;
	protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

	protected const float minMoveDistance = 0.0001f;
	protected const float shellRadius = 0.01f;

	public LayerMask layerMask;

	public List<Collider2D> ignoredColliders;

	

	void OnEnable() {
		rb2d = GetComponent<Rigidbody2D>();
		collider = GetComponent<BoxCollider2D>();
		this.currentGravityModifier = gravityModifier;
	}

	protected void Start() {
		contactFilter.useTriggers = false;
		contactFilter.SetLayerMask(layerMask);
		contactFilter.useLayerMask = true;
	}
	protected virtual void ComputeVelocity() {
		
	}

	protected virtual bool IsCollisionFit(Collider2D collider) {
		return true;
	}

	void FixedUpdate() {
		
		velocity += currentGravityModifier * Physics2D.gravity * Time.deltaTime;
		ComputeVelocity();
		grounded = false;

		Vector2 deltaPosition = velocity * Time.deltaTime;

		Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

		Vector2 move = moveAlongGround * deltaPosition.x;

		Movement(move, false);

		move = Vector2.up * deltaPosition.y;

		Movement(move, true);
	}

	void Movement(Vector2 move, bool yMovement) {
		float distance = move.magnitude;

		foreach (Collider2D collider in ignoredColliders) {
			collider.enabled = false;
		}

		if (distance > minMoveDistance) {
			int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);

			for (int i = 0; i < count; i++) {
				Vector2 currentNormal = hitBuffer[i].normal;
				Vector2 force = Vector2.zero;
				PhysicsObject obj = hitBuffer[i].collider.GetComponent<PhysicsObject>();

				if (!IsCollisionFit(hitBuffer[i].collider)) {
					continue;
				}

				if (obj != null) {
					force = obj.velocity;
				}

				if (currentNormal.y > minGroundNormalY) {
					grounded = true;
					if (yMovement) {
						groundNormal = currentNormal;
						currentNormal.x = 0;
					}
				}

				float projection = Vector2.Dot(velocity, currentNormal);
				if (projection < 0) {
					velocity -= projection * currentNormal * bounciness;
				}

				float modifiedDistance = hitBuffer[i].distance - shellRadius;
				distance = modifiedDistance < distance ? modifiedDistance : distance;
			}
		}
		if (rb2d.bodyType == RigidbodyType2D.Kinematic) rb2d.position = rb2d.position + move.normalized * distance;

		foreach (Collider2D collider in ignoredColliders) {
			collider.enabled = true;
		}
	}
}