using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {
	public Transform start;
	public Transform end;

	public Color c1;
	public Color c2;

	private float colorCounter = 0f;

	private LineRenderer line;
	// Use this for initialization
	void Start() {
		this.line = GetComponent<LineRenderer>();
		this.line.SetPosition(0, this.start.position);
		this.line.SetPosition(1, this.end.position);
	}

	// Update is called once per frame
	void Update() {
		colorCounter += Time.deltaTime;
		if (colorCounter > 0.05f) {
			if (line.startColor == c1) line.startColor = line.endColor = c2;
			else line.startColor = line.endColor = c1;
			colorCounter = 0;
		}
	}
	void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawLine(start.position, end.position);
	}
}