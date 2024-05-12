using DummyClient;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGame : MonoBehaviour
{
	private enum GameProgress
	{
		None = 0,
		Ready = 1,
		StartRound = 2,
		EndRound = 3,
		Result = 4,
		GameOver = 5,
		Disconnect = 6
	}

	private enum ClientType
	{
		Local = 0,
		Remote = 1,
	}

	private enum Turn
	{
		Own = 0,
		Opponent = 1,
	}

	private enum Colors
	{
		Black = 0,
		White = 1,
	}

	private enum Winner
	{
		None = 0,
		Own = 1,
		Opponent = 2,
		Tie = 3,
	}

	#region UI Variables
	[SerializeField] private TextMeshProUGUI ProgressText;
	[SerializeField] private Image OtherSetCardImage;
	[SerializeField] private Image PlayerSetCardImage;
	[SerializeField] private TextMeshProUGUI PlayerSetCardText;
	[SerializeField] private TextMeshProUGUI LocalWinCount;
	[SerializeField] private TextMeshProUGUI RemoteWinCount;

	public List<Sprite> FCardImages;
	public List<Sprite> BCardImages;
	public List<Sprite> OCardImages;

	private int pBeforeColor = -1;
	private int oBeforeColor = -1;
	#endregion

	#region Unity Variables
	// �� ����� ����
	private int RoundCount;
	private int TurnCount;
	private float StepCount = 0;
	// �� ����� ��� �ð� �Ҵ�� ����
	private float RemainTime = 0f;
	// ���� ���� ���� ��ȣǥ�� �ð�.
	private const float WaitTime = 10.0f;
	// ��� �ð�.
	private const float TurnTime = 30.0f;
	#endregion

	#region Server Variables
	private NetworkManager network = null;
	private ClientType local;
	private ClientType remote;
	private ClientType turn;
	private ClientType before;

	private int localWin = 0;
	private int remoteWin = 0;
	#endregion

	// ���� ��Ȳ.
	private GameProgress progress;
	private Winner winner;

	private float currentTime;
	private bool isGameOver;
	private bool isFirst;

	private static InGame inGame;
	public static InGame _InGame
	{
		get
		{
			if (inGame == null)
			{
				inGame = FindObjectOfType<InGame>();
			}
			return inGame;
		}
	}

	private void Start()
	{
		GameObject _network = GameObject.Find("NetworkManager");
		network = _network.GetComponent<NetworkManager>();
		if (network != null)
		{
			network.RegisterEventHandler(EventCallback);
		}

		ResetGame();
		isGameOver = false;
		RemainTime = TurnTime;
	}

	private void Update()
	{
		switch (progress)
		{
			case GameProgress.Ready:
				UpdateReady();
				break;
			case GameProgress.StartRound:
				UpdateRound();
				break;
			case GameProgress.EndRound:
				UpdateData();
				break;
			case GameProgress.GameOver:
				UpdateGameOver();
				break;
		}
	}

	public void GameStart()
	{
		progress = GameProgress.Ready;

		if (network.IsServer() == true)
		{
			local = ClientType.Local;
			remote = ClientType.Remote;
		}
		else
		{
			local = ClientType.Remote;
			remote = ClientType.Local;
		}

		turn = ClientType.Local;
		before = ClientType.Local;

		pBeforeColor = 0;
		oBeforeColor = 0;
		
		localWin = 0;
		remoteWin = 0;

		isGameOver = false;
		winner = Winner.None;

	}

	private void UpdateReady()
	{
		// ���� ���� ��ȣ ǥ�ø� ��ٸ��ϴ�.
		ProgressText.text = "Waiting to Start";
		//PlayerManager.Instance._myPlayer.SettingHandInteraction(false);
		if(PlayerManager.Instance._players.Count != 0) currentTime += Time.deltaTime;
		
		if (currentTime > WaitTime)
		{
			// ���� �����Դϴ�.
			TurnCount = 0;
			progress = GameProgress.StartRound;
		}

	}

	private void UpdateRound()
	{
		RoundCount = RoundCount + 1;
		

		PlayerSetCardImage.sprite = OCardImages[pBeforeColor];
		OtherSetCardImage.sprite = OCardImages[oBeforeColor];

		RemainTime = TurnTime;
		ProgressText.text = $"Start Round : {RoundCount}";
		bool SetClient = false;
		if (TurnCount == 0)
		{
			turn = before;
			Debug.Log($"{RoundCount}���� ���� :{turn}");
			isFirst = true;
			if (turn == local)
			{
				SetClient = DoOwnTurn();  // ���� �Է�
			}
			if (turn == remote)
			{
				SetClient = DoOppnentTurn();  // ����� �Է�
			}
		}
		else if (TurnCount == 1)
		{
			isFirst = false;
			turn = (before == local) ? remote : local;
			if (turn == local)
			{
				SetClient = DoOwnTurn();  // ���� �Է�
			}
			else if (turn == remote)
			{
				SetClient = DoOppnentTurn();  // ����� �Է�
			}
		}
		else if (TurnCount == 2)
		{
			progress = GameProgress.EndRound;
		}

		if (SetClient == false)
		{
			return;
		}
		else
		{
			// ī�尡 ���̴� ���� ȿ���� ���ϴ�. 
		}

		// ���� �����մϴ�.
		Debug.Log($"���� ���� :{turn}");

	}

	private void UpdateData()
	{
		// �� Ŭ���̾�Ʈ �� ���� �ް�, ������ ���� �¸��� ��ȸ
		int ClientNum = PlayerManager.Instance.ReturnPlayerCard().Number;
		int RemoteNum = PlayerManager.Instance.ReturnCardNumber();
		if (ClientNum > RemoteNum || (ClientNum == 0 && RemoteNum == 8))
		{
			localWin = localWin + 1;
			before = local;
			LocalWinCount.text = localWin.ToString();
			ProgressText.text = "You Win";
		}
		else if(RemoteNum > ClientNum || (ClientNum == 8 && RemoteNum == 0))
		{
			remoteWin = remoteWin + 1;
			before = remote;
			RemoteWinCount.text = remoteWin.ToString();
			ProgressText.text = "Enemy Win";
		}
		else if(ClientNum == RemoteNum)
		{
			ProgressText.text = "Draw";
			remoteWin = remoteWin + 1;
			localWin = localWin + 1;
			LocalWinCount.text = localWin.ToString();
			RemoteWinCount.text = remoteWin.ToString();
			before = Random.Range(1, 10) / 5 == 1 ? local : remote;
		}

		if(localWin >= 5)
		{
			if(local == ClientType.Local) winner = Winner.Own;
			if(local == ClientType.Remote) winner = Winner.Opponent;
		}
		else if(remoteWin >= 5)
		{
			if(local == ClientType.Local) winner = Winner.Opponent;
			if(local == ClientType.Remote) winner = Winner.Own;
		}

		if (winner != Winner.None)
		{
			//�¸��� ���� ����ȿ���� ���ϴ�.
			if (winner == Winner.Own)
			{
				ProgressText.text = "You Win!!!";
			}
			else if(winner == Winner.Opponent)
			{
				ProgressText.text = "You Lose...";
			}
			progress = GameProgress.Result;
		}

		TurnCount = 0;
		progress = GameProgress.StartRound;
	}

	private void UpdateGameOver()
	{
		StepCount += Time.deltaTime;
		if (StepCount > 3.0f)
		{
			// ������ �����մϴ�.
			ResetGame();
			isGameOver = true;
		}
	}

	// �ڽ��� ���� ���� ó��
	public bool DoOwnTurn()
	{
		int index = -1;
		if (isFirst == false) index = PlayerManager.Instance.ReturnCardColor();

		if (index != -1)
		{
			OtherSetCardImage.sprite = BCardImages[index];
			Debug.Log($"DoOppnentTurn, index:{index}");
		}
		else if (index == -1)
		{
			Debug.Log($"�� �� ����");
		}
		ProgressText.text = "Your Turn";

		Debug.Assert(PlayerManager.Instance != null, "PlayerManager Is Null");
		Debug.Assert(PlayerManager.Instance._myPlayer != null, "PlayerManager's My Player is Null");
		if(PlayerManager.Instance._myPlayer.IsShowingHand == false)
		{
			PlayerManager.Instance._myPlayer.ShowHand();
			PlayerManager.Instance._myPlayer.SettingHandInteraction(true);
		}
		RemainTime -= Time.deltaTime;
		if (RemainTime <= 0.0f)
		{
			// Ÿ�ӿ���.
			RemainTime = 0.0f;
			// ���� Ŭ���̾�Ʈ�� ī�� �� �����ϰ� 1�� ������
		}
		else
		{
			bool isSetCard = PlayerManager.Instance._myPlayer.IsSetCard;
			if (isSetCard == false)
			{
				return false;
			}

			PlayerManager.Instance._myPlayer.ShowHand(false);
			pBeforeColor = PlayerManager.Instance.ReturnPlayerCard().Color;
			PlayerSetCardImage.sprite = FCardImages[pBeforeColor];
			PlayerSetCardText.text = $"{PlayerManager.Instance.ReturnPlayerCard().Number}";
			if (pBeforeColor == 1) PlayerSetCardText.color = new Vector4(0, 0, 0, 1);
			else if (pBeforeColor == 0) PlayerSetCardText.color = Vector4.one;
		}

		C_SetCard setPacket = new C_SetCard();
		if (local == ClientType.Local)
			setPacket.destinationId = (int)ClientType.Local + 1;
		else
			setPacket.destinationId = (int)ClientType.Remote + 1;
		setPacket.selectNum = PlayerManager.Instance.ReturnPlayerCard().Number;
		setPacket.selectCol = PlayerManager.Instance.ReturnPlayerCard().Color;
		network.Send(setPacket.Write());

		PlayerSetCardImage.sprite = FCardImages[PlayerManager.Instance.ReturnPlayerCard().Color];
		PlayerSetCardText.text = $"{PlayerManager.Instance.ReturnPlayerCard().Number}";

		TurnCount = TurnCount + 1;
		return true;
	}

	// ����� ���� ���� ó��.
	public bool DoOppnentTurn()
	{
		int index = -1;
		if(isFirst == false) index = PlayerManager.Instance.ReturnCardColor();

		if (index != -1)
		{
			OtherSetCardImage.sprite = BCardImages[index];
			Debug.Log($"DoOppnentTurn, index:{index}");
		}
		else if (index == -1)
		{
			Debug.Log($"���� ����");
		}
		ProgressText.text = "Enemy's Turn";

		if(PlayerManager.Instance.OtherCardColor == -1 && PlayerManager.Instance.OtherCardNumber == -1)
		{
			return false;
		}

		Debug.Log("����");
		Debug.Log("Recv:" + index + " [" + network.IsServer() + "]");
		Debug.Log($"Other Data: Color {PlayerManager.Instance.OtherCardColor} Number {PlayerManager.Instance.OtherCardNumber}");
		OtherSetCardImage.sprite = BCardImages[index];

		TurnCount = TurnCount + 1;
		return true;
	}

	private void ResetGame()
	{
		turn = ClientType.Local;
		progress = GameProgress.None;
		StepCount = 0f;
		RoundCount = 0;
		TurnCount = 0;
		currentTime = 0;
		localWin = 0;
		remoteWin = 0;
	}

	// ���� ���� üũ.
	public bool IsGameOver()
	{
		return isGameOver;
	}

	// �̺�Ʈ �߻� ���� �ݹ� �Լ�.
	public void EventCallback(NetEventState state)
	{
		switch (state.type)
		{
			case NetEventType.Disconnect:
				if (progress < GameProgress.Result && isGameOver == false)
				{
					progress = GameProgress.Disconnect;
				}
				break;
		}
	}
}
