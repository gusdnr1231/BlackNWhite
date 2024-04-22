using DummyClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
	public static PlayerManager Instance { get; } = new PlayerManager();
	public Player _myPlayer;
    int CardColor;

    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public void BroadcastCard()
    {

    }

    public CardData ReturnCard()
    {
		CardData SelectCardData = new CardData();
		SelectCardData.Number = _myPlayer.CardNum;
		SelectCardData.Color = _myPlayer.CardColor;
		return SelectCardData;
    }

	// 플레이어 리스트 생성&갱신
	public void Add(S_PlayerList packet)
	{
		Object obj = Resources.Load("Player");

		foreach (S_PlayerList.Player p in packet.players)
		{
			GameObject go = Object.Instantiate(obj) as GameObject;

			if (p.isSelf)
			{
				MyPlayer myPlayer = go.AddComponent<MyPlayer>();
				myPlayer.PlayerID = p.playerId;
				myPlayer.transform.position = new Vector3(0, -1, 0);
				_myPlayer = myPlayer;
				_myPlayer.ShowHand();
				_myPlayer.WinCount = 0;
			}
			else
			{
				Player player = go.AddComponent<Player>();
				player.PlayerID = p.playerId;
				player.transform.position = new Vector3(0, 1, 0);
				_players.Add(p.playerId, player);
				_players[p.playerId].ShowHand(false);
				_players[p.playerId].WinCount = 0;
			}
		}
	}

	// 나 혹은 누군가가 새로 접속했을 때
	public void EnterGame(S_BroadcastEnterGame packet)
	{
		if (packet.playerId == _myPlayer.PlayerID)
			return;

		Object obj = Resources.Load("Player");
		GameObject go = Object.Instantiate(obj) as GameObject;

		Player player = go.AddComponent<Player>();
		_players.Add(packet.playerId, player);
	}

	// 나 혹은 누군가가 게임을 떠났을 때
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
