using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{
	enum Mode
	{
		SelectHost = 0,
		Connection,
		Game,
		Disconnection,
		Error,
	};

	enum HostType
	{
		None = 0,
		Server,
		Client,
	};

	private Mode m_mode;
	private IPAddress ipAddr;
	private HostType hostType;
	private const int m_port = 50765;
	private int m_counter = 0;

	public GameObject UI_MainMenu;
	public GameObject UI_Game;
	public NetworkManager network;

	public void OnClick_BtnMakeRoom()
	{
		hostType = HostType.Server;

		UI_MainMenu.SetActive(false);
		UI_Game.SetActive(true);
	}

	public void OnClick_BtnEnterRoom()
	{
		hostType = HostType.Client;

		UI_MainMenu.SetActive(false);
		UI_Game.SetActive(true);
	}
	public void OnClick_Exit()
	{
		hostType = HostType.None;
		m_mode = Mode.SelectHost;

		UI_MainMenu.SetActive(true);
		UI_Game.SetActive(false);
	}


	private void Awake()
	{
		m_mode = Mode.SelectHost;
		hostType = HostType.None;

		//GameObject obj = new GameObject("Network");
		//network = obj.AddComponent<NetworkManager>();
		//DontDestroyOnLoad(obj);

		// 호스트명을 가져옵니다.
		string hostname = Dns.GetHostName();
		// 호스트명에서 IP주소를 가져옵니다.
		IPHostEntry iphost = Dns.GetHostEntry(Dns.GetHostName());
		ipAddr = iphost.AddressList[1];
	}

	void Update()
	{
		switch (m_mode)
		{
			case Mode.SelectHost:
				OnUpdateSelectHost();
				break;

			case Mode.Connection:
				OnUpdateConnection();
				break;

			case Mode.Game:
				OnUpdateGame();
				break;

			case Mode.Disconnection:
				//OnUpdateDisconnection();
				break;
		}

		++m_counter;
	}

	// Sever 또는 Client 선택화면
	void OnUpdateSelectHost()
	{
		switch (hostType)
		{
			case HostType.Server:   // 플레이어1
				{
					bool ret = network.ConnectToServer(ipAddr, m_port);
					m_mode = ret ? Mode.Connection : Mode.Error;
					Debug.Log("HostType Server");
					network.m_isServer = true;
				}
				break;

			case HostType.Client:   // 플레이어2
				{
					bool ret = network.ConnectToServer(ipAddr, m_port);
					m_mode = ret ? Mode.Connection : Mode.Error;
					Debug.Log("HostType Client");

				}
				break;

			default:
				break;
		}
	}

	void OnUpdateConnection()
	{
		if (network.IsConnected() == true)
		{
			m_mode = Mode.Game;

			GameObject game = GameObject.Find("InGame");
			game.GetComponent<InGame>().GameStart();
		}
	}

	void OnUpdateGame()
	{
		GameObject game = GameObject.Find("InGame");
		if (game.GetComponent<InGame> ().IsGameOver() == true)
		{
			m_mode = Mode.Disconnection;
		}
	}



}

