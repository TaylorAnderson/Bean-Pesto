using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : PhysicsObject {
	public float friction = 0.8f;
	protected override void ComputeVelocity() {
		this.velocity.x *= friction;
	}
}
