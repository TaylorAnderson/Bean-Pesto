using UnityEditor;

[CustomEditor(typeof(Computer))]
[System.Serializable]
public class ComputerScriptEditor : Editor {
	override public void OnInspectorGUI() {
		base.OnInspectorGUI();
		var compScript = target as Computer;
		compScript.type = (ComputerType) EditorGUILayout.EnumPopup("Computer type: ", compScript.type);

		if (compScript.type == ComputerType.KEY)
			compScript.doorToUnlock = EditorGUILayout.ObjectField("Door to unlock: ", compScript.doorToUnlock as UnityEngine.Object, typeof(Door), true) as Door;
		EditorUtility.SetDirty(compScript);
		
	}
}
