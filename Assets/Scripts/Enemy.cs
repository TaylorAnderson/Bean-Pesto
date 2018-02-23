using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uween;

public enum EnemyState {
	Normal,
	Alert,
	Bonked,
	LookingAround,
	Dead
}
public class Enemy : PhysicsObject {


	private const string IdleAnim = "EnemyIdle";
	private const string BonkAnim = "EnemyBonk";
	private const string WalkAnim = "EnemyWalk";
	private const string DeadAnim = "EnemyDead";
	private const string AlarmWalkAnim = "EnemyAlarmedWalk";
	private const string LookingAroundAnim = "EnemyLookingAround";
	private const string SurprisedAnim = "EnemySurprised";

	new private SpriteRenderer renderer;
	private Animator animator;
	private int currentMarker = 0;
	private EnemyState preBonkState;
	private EnemyState state;
	private Player player;
	private Vector2 suspicionPoint;
	
	private float bonkDelayTimer = 1.2f;

	private Transform objectToInvestigate;

	private float alertPersistenceTimer = 0;
	public StateMachine<EnemyState> fsm;
	[HideInInspector]
	public int facingDirection = 1;
	public float speed = 1f;
	public float runSpeed = 3f;
	public float walkDistance = 3f;
	public EnemySight sight;
	public GameObject lastPlayerSightingPrefab;
	public Dictionary<string, float> animLengths = new Dictionary<string, float>();
	[Tooltip("The minimum distance between the player and the enemy before the enemy becomes ALERT.")]
	public int playerDistanceThreshold = 5;
	[Tooltip("How long the player has to be out of sight before the enemy stops being alert")]
	public float alertPersistenceTime = 1f;
	[Tooltip("How long the enemy will wait when they hit a waypoint")]
	public float waypointWaitTime = 0.2f;

	public GameObject helmetPrefab;

	private List<Vector2> waypoints = new List<Vector2>();
	private float suspiciousWaitTimer = 0f;
	private bool waiting = false;
	void Awake() {

		this.player = GameObject.Find("Player").GetComponent<Player>();
		currentMarker = 0;
		alertPersistenceTimer = alertPersistenceTime;
		var sprite = this.transform.GetChild(0).gameObject;
		this.renderer = sprite.GetComponent<SpriteRenderer>();
		this.animator = GetComponent<Animator>();

		foreach (AnimationClip ac in animator.runtimeAnimatorController.animationClips) {
			animLengths[ac.name] = ac.length;
		}

		fsm = StateMachine<EnemyState>.Initialize(this, EnemyState.Normal);
		fsm.Changed += OnStateChanged;
		
		this.sight.OnPlayerSeen += () => fsm.ChangeState(EnemyState.Alert);
	}

	private void OnStateChanged(EnemyState state) {

	}

	protected override void ComputeVelocity() {
		renderer.flipX = velocity.x != 0 ? velocity.x < 0 : renderer.flipX;
	}

	void Normal_Enter() {

		animator.Play(WalkAnim);
		waypoints.Clear();
		waypoints.Add(new Vector2(this.transform.position.x - this.walkDistance, transform.position.y));
		waypoints.Add(new Vector2(this.transform.position.x + this.walkDistance, transform.position.y));
		
	}

	void Normal_Update() {
		if (Mathf.Abs(velocity.x) > 0) SetFacingDirection((int) Mathf.Sign(velocity.x));

		var markerPos = waypoints[currentMarker];
		velocity.x = markerPos.x - transform.position.x;
		velocity = velocity.normalized * speed;

		var start = (Vector2) transform.position + Vector2.up*0.5f;
		var facingDir = Vector2.right*Mathf.Sign(velocity.x);

		var blockInFront = Physics2D.Raycast(start, facingDir, 1f, layerMask);
		var blockToWalkOn = Physics2D.Raycast(start, facingDir + Vector2.down, 1f, layerMask);

		if (isCloseTo(this.transform.position.x, markerPos.x, speed / 100) || blockInFront || !blockToWalkOn) {
			velocity.x = 0;
			if (! waiting) {
				animator.Play(IdleAnim);
				waiting = true;
				TweenNull.Add(gameObject, this.waypointWaitTime).OnComplete += () => {
					this.currentMarker++;
					this.currentMarker %= waypoints.Count;
					velocity.x = markerPos.x - transform.position.x;	
					waiting = false;
					animator.Play(WalkAnim);
				};
			}
		}
	}

	IEnumerator Alert_Enter() {
		animator.Play(SurprisedAnim);
		velocity.x = 0;

		yield return new WaitForSeconds(animLengths[SurprisedAnim]);
		animator.Play(AlarmWalkAnim);
	}

	void Alert_Update() {
		SetFacingDirection((int) Mathf.Sign(velocity.x));
		velocity.x = player.transform.position.x - transform.position.x;
		velocity = velocity.normalized * runSpeed;

		
		if (Mathf.Abs(this.player.transform.position.y - this.transform.position.y) > 1 ||
			Physics2D.Linecast(transform.position, player.transform.position, layerMask)) {
			alertPersistenceTimer -= Time.deltaTime;

			if (alertPersistenceTimer <= 0) {
				fsm.ChangeState(EnemyState.Normal);
			}

		} 
		else {
			alertPersistenceTimer = alertPersistenceTime;
		}
	}

	IEnumerator Bonked_Enter() {
		animator.Play(BonkAnim, -1, 0);
		velocity.x = 0;
		yield return new WaitForSeconds(animLengths[BonkAnim]);
		if (preBonkState != EnemyState.Alert) fsm.ChangeState(EnemyState.LookingAround);
		else fsm.ChangeState(EnemyState.Alert);
	}

	void Bonked_Update() { }

	IEnumerator LookingAround_Enter() {
		animator.Play(LookingAroundAnim, -1, 0f);
		velocity.x = 0;
		yield return new WaitForSeconds(animLengths[LookingAroundAnim]);
		//we know if we get here then we aren't alert or anything
		fsm.ChangeState(EnemyState.Normal);
	}

	void Dead_Enter() {
		animator.Play(DeadAnim);
		Destroy(collider);
		this.velocity.y += 10;
		var helmet = Instantiate(helmetPrefab);
		helmet.transform.position = (Vector2) transform.position + Vector2.up * 0.3f;
		helmet.GetComponent<PhysicsObject>().velocity = Vector2.down*100f;
	}

	void Dead_Update() {
		this.transform.Rotate(Vector3.forward * 5);
		return;
	}
	
	public void Bonk() {
		preBonkState = fsm.State;
		fsm.ChangeState(EnemyState.Bonked, StateTransition.ForceOverwrite);
	}

	public void Die() {
		fsm.ChangeState(EnemyState.Dead, StateTransition.ForceOverwrite);
	}

	public void SetFacingDirection(int newFacingDirection) {
		this.facingDirection = newFacingDirection;
		sight.DrawFieldOfView();
	}

	//this ones just used to play nice with animator events
	public void SetFacingDirectionRelativeToSpriteFlip(int newFacingDirection) {
		this.facingDirection = newFacingDirection;
		if (this.renderer.flipX) facingDirection *= -1;
		sight.DrawFieldOfView();
	}

	bool isCloseTo(float val1, float val2, float marginOfError) {
		return (val1 < val2 + marginOfError && val1 > val2 - marginOfError);
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		var obj = collision.gameObject.GetComponent<PhysicsObject>();
		if (obj != null) {
			//ie, the object is above us and moving down
			if (obj.transform.position.y > (transform.position + Vector3.up*0.9f).y && obj.velocity.y < 0) {
				Bonk();
				obj.velocity.y = 10f;
			}
		}

	}

	void OnDrawGizmos() {
		waypoints.Clear();
		waypoints.Add(new Vector2(this.transform.position.x - this.walkDistance, transform.position.y));
		waypoints.Add(new Vector2(this.transform.position.x + this.walkDistance, transform.position.y));
		Gizmos.color = Color.white;
				
		Gizmos.DrawLine(waypoints[0] + Vector2.up, waypoints[0]);
		Gizmos.DrawLine(waypoints[1] + Vector2.up, waypoints[1]);
	}
}