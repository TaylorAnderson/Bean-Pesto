using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using Uween;

public class Button : MonoBehaviour {

	public Sprite offFrame;
	public Sprite onFrame;
	[Header("Whatever's dropped here needs to implement IActivatable.")]
	public MonoBehaviour objectToActivate;
	[Space]
	public GameObject effectPrefab;
	private new SpriteRenderer renderer;
	[HideInInspector]
	public bool activated = false;
	// Use this for initialization
	void Start() {
		renderer = GetComponent<SpriteRenderer>();

	}

	// Update is called once per frame
	void Update() {

	}
	public IEnumerator Activate() {
		//StartCoroutine(PauseManager.Instance.PauseForSeconds(0.4f));
		this.activated = true;
		this.renderer.sprite = this.onFrame;
		var effect = Instantiate(effectPrefab);
		effect.transform.position = this.transform.position;

		TweenSX.Add(gameObject, 0.3f, 2f).EaseOutBack().Then(() => { TweenSX.Add(gameObject, 0.2f, 1).EaseOutBack().Delay(0.2f); });
		TweenSY.Add(gameObject, 0.3f, 0.3f).EaseOutBack().Then(() => { TweenSY.Add(gameObject, 0.2f, 1).EaseOutBack().Delay(0.2f); });
		this.GetComponent<BoxCollider2D>().enabled = false;

		yield return new WaitForSeconds(0.5f);

		var cinematics = Camera.main.GetComponent<ProCamera2DCinematics>();
		cinematics.AddCinematicTarget(objectToActivate.transform);
		PauseManager.Instance.PauseGame();
		var activatable = (IActivatable) objectToActivate;
		cinematics.OnCinematicTargetReached.AddListener((_) => { activatable.Activate(); });
		cinematics.OnCinematicFinished.AddListener(() => {
			PauseManager.Instance.UnpauseGame();
		});

		cinematics.Play();

	}
	public void Squash(float time = 0.1f) {
		TweenSX.Add(gameObject, time, 2f).EaseOutBack().Then(() => { TweenSX.Add(gameObject, time, 1).EaseOutBack().Delay(0.2f); });
		TweenSY.Add(gameObject, time, 0.4f).EaseOutBack().Then(() => { TweenSY.Add(gameObject, time, 1).EaseOutBack().Delay(0.2f); });
	}
	public void Stretch() {
		TweenSX.Add(gameObject, 0.1f, 0.7f).EaseOutBack().Then(() => { TweenSX.Add(gameObject, 0.1f, 1).EaseOutBack(); });
		TweenSY.Add(gameObject, 0.1f, 1.2f).EaseOutBack().Then(() => { TweenSY.Add(gameObject, 0.1f, 1).EaseOutBack(); });
	}

}