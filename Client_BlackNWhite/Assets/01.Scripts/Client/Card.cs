using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct CardData
{
	public int Number;
	public int Color;
}

public class Card : MonoBehaviour
{
	private Player Owner;

    private Image CardImage;
    private TextMeshProUGUI CardText;

	public CardData Data;
	public Sprite[] CardImages;

	private void Awake()
	{
		CardImage = GetComponentInChildren<Image>();
		CardText = GetComponentInChildren<TextMeshProUGUI>();
	}

	public void SettingOwner(Player owner)
	{
		Owner = owner;
	}

	public void SetData(int number, int color)
	{
		Data.Number	= number;
		Data.Color = color;
		SetUI();
	}

	public void SetUI()
	{
		CardText.text = $"{Data.Number}";
		if(Data.Color == 1) CardText.color = new Vector4(0,0,0,1);
		else if(Data.Color == 0) CardText.color = Vector4.one;
		CardImage.sprite = CardImages[Data.Color];
	}

	public void SettingCard()
	{
		Owner.SetCard(Data);
		Owner.Cards.Remove(this);
		Destroy(gameObject);
	}
}
