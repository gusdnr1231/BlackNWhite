using DummyClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DummyClient.S_PlayerList;

public class PlayerManager
{

	public static PlayerManager Instance { get; } = new PlayerManager();

	Player _myPlayer;
    CardData SelectCardData;
    int CardColor;

    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public int ReturnCardColor()
    {
        int returnColor = CardColor;
        CardColor = -1;
        return returnColor;
    }

    public void BroadcastCard()
    {

    }

    public CardData ReturnCard()
    {
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
				Player myPlayer = go.AddComponent<Player>();
				myPlayer.PlayerID = p.playerId;
				myPlayer.transform.position = new Vector3(0, 5, 0);
				_myPlayer = myPlayer;
			}
			else
			{
				Player player = go.AddComponent<Player>();
				player.PlayerID = p.playerId;
				player.transform.position = new Vector3(0, 0, 0);
				_players.Add(p.playerId, player);
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
