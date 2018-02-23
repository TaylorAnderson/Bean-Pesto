using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
	public GameObject explosionPrefab;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Unlock() {
		var expl = Instantiate(explosionPrefab);
		expl.transform.position = transform.position;
		Destroy(GetComponent<SpriteRenderer>());
	}
	public void UnpauseGame() {
		PauseManager.Instance.UnpauseGame();
	}
}
