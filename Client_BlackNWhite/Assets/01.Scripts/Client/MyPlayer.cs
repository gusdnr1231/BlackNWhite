using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : Player
{
	NetworkManager _network;

	public bool IsShowingHand { get; private set;}
	public bool IsCardInteraction { get; private set; }
	public GameObject CardPrefab;
	public RectTransform CardContainer;

	private void Start()
	{
		SettingCardHand();
	}

	public void ShowHand(bool isActive = true)
	{
		Debug.Log($"Player Hand : {isActive}");
		foreach(var card in Cards)
		{
			card.gameObject.SetActive(isActive);
		}
		IsShowingHand = isActive;
		if(isActive == true) IsSetCard = false;
	}

	public void SettingCardHand()
	{
		IsCardInteraction = true;
		Cards = new List<Card>(9);

		GameObject cardObj;
		RectTransform cardTrm;
		Card card;
		for (int i = 0; i < 9; i++)
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

		SettingHandInteraction(true);
		IsSetCard = false;
	}

	public void SettingHandInteraction(bool isInteraction)
	{
		IsCardInteraction = isInteraction;
		for (int c = 0; c < Cards.Count; c++)
		{
			Cards[c].SettingInteraction(isInteraction);
		}
	}

	public void ResetCard()
	{
		Cards.ForEach(c =>
		{
			Destroy(c.gameObject);
		});
		Cards.Clear();
	}

	private int SetCardColor(int num)
	{
		return num % 2 == 0 ? 0 : 1;
	}
}
