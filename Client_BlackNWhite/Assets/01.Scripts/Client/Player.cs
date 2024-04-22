using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public int PlayerID { get; set; }
	public static int CardNum { get; set; }
	public static int CardColor { get; set; }
	public static int WinCount { get; set; }

	[SerializeField] private GameObject CardPrefab;
	[SerializeField] private RectTransform CardContainer;
	public List<Card> Cards;

	private void Start()
	{
		CardNum = 0;
		CardColor = -1;
		WinCount = 0;

		SettingStartGame();
	}

	public void SetCard(CardData data)
	{
		CardNum = data.Number;
		CardColor = data.Color;

		Debug.Log($"Card Data: {CardNum} / {CardColor}");
	}

	public void SettingStartGame()
	{
		GameObject cardObj;
		RectTransform cardTrm;
		Card card;
		for(int i = 0; i < 8; i++)
		{
			cardObj = Instantiate(CardPrefab);
			cardObj.name = $"Card_{i}";
			cardTrm = cardObj.GetComponent<RectTransform>();
			cardTrm.SetParent(CardContainer);
			cardTrm.localScale = Vector3.one;
			card = cardObj.GetComponent<Card>();
			card.SettingOwner(this);
			card.SetData(i, SetCardColor(i));
			Cards.Add(card);
		}
	}

	private int SetCardColor(int num)
	{
		return num % 2 == 0 ? 0 : 1;
	}
}
