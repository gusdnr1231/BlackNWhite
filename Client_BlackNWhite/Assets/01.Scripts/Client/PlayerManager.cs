using DummyClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DummyClient.S_PlayerList;

public class PlayerManager
{
	public static PlayerManager Instance { get; } = new PlayerManager();
	public MyPlayer _myPlayer;
	public int OtherCardColor { get; set; }
	public int OtherCardNumber { get; set; }

	public Dictionary<int, Player> _players = new Dictionary<int, Player>();

	public void SetCard(S_SetOtherCard cardData)
	{
		OtherCardNumber = cardData.selectNum;
		OtherCardColor = cardData.selectCol;
	}

	public int ReturnCardColor()
	{
		int CardColor = OtherCardColor;
		OtherCardColor = -1;
		return CardColor;
	}

	public int ReturnCardNumber()
	{
		int CardNumber = OtherCardNumber;
		OtherCardNumber = -1;
		return CardNumber;
	}

	public CardData ReturnPlayerCard()
	{
		CardData SelectCardData = new CardData();
		_myPlayer.CardNum = -1;
		_myPlayer.CardColor = -1;
		return SelectCardData;
	}

	// �÷��̾� ����Ʈ ����&����
	public void Add(S_PlayerList packet)
	{
		Object obj = Resources.Load("Player");

		foreach (S_PlayerList.Player p in packet.players)
		{
			GameObject go = Object.Instantiate(obj) as GameObject;

			if (p.isSelf)
			{
				go.AddComponent<MyPlayer>();
				_myPlayer = go.GetComponent<MyPlayer>();
				_myPlayer.PlayerID = p.playerId;
				_myPlayer.CardNum = -1;
				_myPlayer.CardColor = -1;
				_myPlayer.transform.position = new Vector3(0, 0, 0);
				_myPlayer.CardPrefab = Resources.Load("Card_UI") as GameObject;
				_myPlayer.CardContainer = GameObject.Find("PlayerCardContainer").GetComponent<RectTransform>();
			}
			else
			{
				Player player = go.AddComponent<Player>();
				player.PlayerID = p.playerId;
				player.CardNum = -1;
				player.CardColor = -1;
				player.IsSetCard = false;
				player.Cards = new List<Card>();
				player.transform.position = new Vector3(0, 1, 0);
				_players.Add(p.playerId, player);
			}
		}
	}

	// �� Ȥ�� �������� ���� �������� ��
	public void EnterGame(S_BroadcastEnterGame packet)
	{
		if (packet.playerId == _myPlayer.PlayerID)
		{
			return;
		}
		else
		{
			Object obj = Resources.Load("Player");
			GameObject go = Object.Instantiate(obj) as GameObject;
			Player player = go.AddComponent<Player>();
			_players.Add(packet.playerId, player);
		}
	}

	// �� Ȥ�� �������� ������ ������ ��
	public void LeaveGame(S_BroadcastLeaveGame packet)
	{
		if (_myPlayer.PlayerID == packet.playerId)
		{
			GameObject.Destroy(_myPlayer.gameObject);
			_myPlayer = null;
		}
		else
		{
			Player player = null;
			if (_players.TryGetValue(packet.playerId, out player))
			{
				GameObject.Destroy(player.gameObject);
				_players.Remove(packet.playerId);
			}
		}
	}

}
