using DummyClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{

	public static PlayerManager Instance { get; } = new PlayerManager();

	Player _myPlayer;
    CardData SelectCardData;
    int CardColor;

    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public void BroadcastCard()
    {

    }

    public CardData ReturnCard()
    {
        return SelectCardData;
    }

    public void AddPlayer()
    {

    }

    public void EnterPlayer()
    {

    }

    public void LeavePlayer()
    {

    }

}
