using System;
using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public enum ComputerType {
	KEY = 1,
	COINS = 2
}
public class Computer : MonoBehaviour {

	private Animator animator;
	// Use this for initialization
	private const string HackingAnim = "ComputerHacking";
	private const string JumpAnim = "ComputerJump";
	private const string HackedAnim = "ComputerHacked";

	public bool finishedHacking = false;
	public GameObject explosion;
	[HideInInspector]
	public Door doorToUnlock;
	[HideInInspector]
	public ComputerType type;

	public SpriteRenderer iconSprite;

	public Sprite coinIcon;
	public Sprite keyIcon;

	public GameObject chipletPrefab;

	void Start() {
		animator = GetComponent<Animator>();

		if (this.type == ComputerType.COINS) {
			this.iconSprite.sprite = this.coinIcon;
		}
		if (this.type == ComputerType.KEY) {
			this.iconSprite.sprite = this.keyIcon;
		}

	}

	// Update is called once per frame
	void Update() {
		if (finishedHacking) {
			StartCoroutine("FinishHack");
		}

	}
	public void StartHacking() {
		iconSprite.enabled = false;
		animator.Play(JumpAnim);
	}

	//called with animation event
	private void JumpFinished() {

		animator.Play(HackingAnim);
		this.iconSprite.enabled = true;
		this.iconSprite.transform.position += Vector3.up * 0.1f;
	}
	private void HackingFinished() {
		Destroy(iconSprite.gameObject);
		animator.Play(HackedAnim);
	}
	private void StartCompletion() {
		finishedHacking = true;
	}
	private IEnumerator FinishHack() {
		if (GetComponent<SpriteRenderer>() != null) {
			var ex = Instantiate(explosion);
			ex.transform.position = transform.position;

		}
		Destroy(GetComponent<SpriteRenderer>());
		Destroy(GetComponent<BoxCollider2D>());

		if (this.type == ComputerType.KEY) {
			yield return new WaitForSeconds(1);

			Destroy(gameObject);
			var cinematics = Camera.main.GetComponent<ProCamera2DCinematics>();
			cinematics.AddCinematicTarget(doorToUnlock.transform);
			cinematics.OnCinematicTargetReached.AddListener((_) => { doorToUnlock.Unlock(); });
			cinematics.OnCinematicFinished.AddListener(() => {
				doorToUnlock.UnpauseGame();
				cinematics.OnCinematicTargetReached.RemoveAllListeners();
				cinematics.OnCinematicFinished.RemoveAllListeners();
				cinematics.RemoveCinematicTarget(doorToUnlock.transform);
			});
			cinematics.Play();

			PauseManager.Instance.PauseGame();
		} else {
			var chipAmt = 20;
			//yes i know this is inefficient, sue me
			var player = GameObject.FindWithTag("Player").GetComponent<Player>();
			for (int i = 0; i < chipAmt; i++) {
				var chiplet = Instantiate(chipletPrefab).GetComponent<Chiplet>();
				chiplet.transform.position = this.transform.position;
				float angle = (float) (i * 360 / chipAmt) * Mathf.Deg2Rad;

				chiplet.Init(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized * 0.2f, player);
			}
			Destroy(gameObject);
		}

	}
}