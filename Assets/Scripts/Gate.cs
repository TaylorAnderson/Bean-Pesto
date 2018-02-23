using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uween;

public class Gate : MonoBehaviour, IActivatable {
	private float velY = 0f;
	private float boundsY;
	private bool finished = false;
	private float intensity = 150;
	// Use this for initializations
	void Start() {
		boundsY = transform.position.y -2;
	}

	public void Activate() {
		TweenY.Add(gameObject, 0.5f, boundsY)
		.EaseOutBounce()
		.OnComplete += () => {finished = true; };
	}
}