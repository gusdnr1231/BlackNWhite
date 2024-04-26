using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : Player
{
	NetworkManager _network;

	public bool IsShowingHand { get; private set;}
	public GameObject CardPrefab;
	public RectTransform CardContainer;

	private void Awake()
	{
		CardContainer = GameObject.Find("PlayerCardContainer").GetComponent<RectTransform>();
	}

	private void Start()
	{
		SettingCardHand();
	}

	public void ShowHand(bool isActive = true)
	{
		GameObject playerHand = CardContainer.gameObject;
		playerHand.SetActive(isActive);
		IsShowingHand = isActive;
		if(isActive == true) IsSetCard = false;
	}

	public void SettingCardHand()
	{
		if (Cards != null)
		{
			foreach (var _card in Cards)
			{
				Cards.Remove(_card);
				Destroy(_card.gameObject);
			}
		}

		GameObject cardObj;
		RectTransform cardTrm;
		Card card;
		for (int i = 0; i < 7; i++)
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

		IsSetCard = false;
	}

	private int SetCardColor(int num)
	{
		return num % 2 == 0 ? 0 : 1;
	}
}
