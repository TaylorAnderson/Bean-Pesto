using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Chiplet : MonoBehaviour {
	private Player player;
	private Vector2 velocity;
	public float speed = 0.05f;
	public float followWait = 1f;
	public float friction = 0.9f;
	public bool collidedWithPlayer = false;
	private new SpriteRenderer renderer;
	public Color c1;
	public Color c2;
	private int colorChangeTimer = 0;
	// Update is called once per frame
	void Update () {
		this.renderer = GetComponent<SpriteRenderer>();
		if (this.player != null) {
			followWait -= Time.deltaTime;
			if (followWait < 0 && !collidedWithPlayer) {
				this.velocity += (Vector2)(player.transform.position + Vector3.up * 0.5f - this.transform.position).normalized * this.speed;
			}
			this.transform.position += (Vector3)this.velocity;
			this.velocity *= this.friction;
		}
		colorChangeTimer++;
		if (colorChangeTimer%3 == 0 && !collidedWithPlayer) {
			if (renderer.color == c1) renderer.color = c2;
			else renderer.color = c1;
		}
		
	}
	public void Init(Vector2 velocity, Player player) {
		this.player = player;
		this.velocity = velocity;
	}
	private void OnCollisionStay2D(Collision2D collision) {
		if (collision.gameObject.GetComponent<Player>() != null && followWait < 0) {
			velocity = Vector2.zero;
			collidedWithPlayer = true;
			this.renderer.color = Color.white;
			LeanTween.scale(gameObject, new Vector3(3f, 0f, 0f), .3f).setOnComplete(() => { Destroy(gameObject); });
		}
	}
}
