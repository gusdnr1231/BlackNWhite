using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public int PlayerID { get; set; }
	public static int CardNum { get; set; }
	public static int CardColor { get; set; }
	public static int WinCount { get; set; }

	private void Start()
	{
		CardNum = 0;
		CardColor = -1;
		WinCount = 0;
	}
}
