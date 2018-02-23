using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActivatable {
	Transform transform { get; }
	void Activate();
}