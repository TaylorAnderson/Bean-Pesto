using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : PhysicsObject {
	[HideInInspector]
	public bool selectable = true;
	public virtual void OnInteract(Player player) {

	}
}
