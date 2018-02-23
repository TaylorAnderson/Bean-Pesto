using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
public class CharacterActions : PlayerActionSet {

	public PlayerAction Left;
	public PlayerAction Right;
	public PlayerAction Down;
	public PlayerAction Up;
	public PlayerAction Jump;
	public PlayerOneAxisAction Move;
	public PlayerAction Run;
	
	public CharacterActions() {
		Left = CreatePlayerAction("Move Left");
		Right = CreatePlayerAction("Move Right");
		Up = CreatePlayerAction("Point Up");
		Down = CreatePlayerAction("Point Down");
		Jump = CreatePlayerAction("Jump");
		Run = CreatePlayerAction("Run");
		Move = CreateOneAxisPlayerAction(Left, Right);
	}
}
