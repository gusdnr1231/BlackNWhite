using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public int PlayerID { get; set; }
	public int WinCount { get; set; }
	public bool IsSetCard { get; set; }
	public int CardNum { get; set; }
	public int CardColor { get; set; }

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

	public bool SetCard(CardData data)
	{
		CardNum = data.Number;
		CardColor = data.Color;

		Debug.Log($"Card Data: {CardNum} / {CardColor}");
		return IsSetCard = true;
	}

	public void ShowHand(bool isActive = true)
	{
		GameObject playerHand = CardContainer.gameObject;
		playerHand.SetActive(isActive);
		IsSetCard = false;
	}

	public void SettingStartGame()
	{
		if(Cards != null)
		{
			foreach(var _card in Cards)
			{
				Cards.Remove(_card );
				Destroy(_card.gameObject);
			}
		}

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
