// Draw lines to the connected game objects that a script has.
// if the target object doesnt have any game objects attached
// then it draws a line from the object to 0,0,0.

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Laser))]
class ConnectLineHandleExampleScript : Editor {

}