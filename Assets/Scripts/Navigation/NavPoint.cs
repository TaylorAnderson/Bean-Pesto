using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPoint : MonoBehaviour {


	public bool isJump = false;
	public Sprite moveSprite;
	public Sprite jumpSprite;
	private SpriteRenderer sprRenderer;
	// Use this for initialization
	void Start () {
		sprRenderer = GetComponent<SpriteRenderer>();
		if (isJump) sprRenderer.sprite = jumpSprite;
		else sprRenderer.sprite = moveSprite;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void UpdateSprite() {

	}
}
