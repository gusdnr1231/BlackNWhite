using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public int PlayerID { get; set; }
	public bool IsSetCard { get; set; }
	public int CardNum { get; set; }
	public int CardColor { get; set; }

	public List<Card> Cards;


	private void Start()
	{
		CardNum = 0;
		CardColor = -1;
	}

	public void SetCard(CardData data)
	{
		CardNum = data.Number;
		CardColor = data.Color;

		Debug.Log($"Card Data: {CardNum} / {CardColor}");
	}
}
