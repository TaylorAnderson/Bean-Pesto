using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PauseManager : MonoBehaviour {
	private static PauseManager _instance;

	public static PauseManager Instance { get { return _instance; } }
	public List<Behaviour> componentsToPause = new List<Behaviour>();

	private void Awake() {
		var temp = new List<Behaviour>();
		foreach (Behaviour component in componentsToPause) {
			var anim = component.GetComponent<Animator>();
			if (anim) {
				temp.Add(anim);
			}
		}

		componentsToPause.AddRange(temp);

		if (_instance != null && _instance != this) {
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}
	public void PauseGame() {
		foreach (Behaviour component in componentsToPause) {
			component.enabled = false;
		}

	}
	public IEnumerator PauseForSeconds(float seconds) {
		Instance.PauseGame();
		yield return new WaitForSeconds(seconds);
		Instance.UnpauseGame();
	}
	public void UnpauseGame() {
		foreach (Behaviour component in componentsToPause) {
			component.enabled = true;
		}
	}
}