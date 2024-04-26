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
	[SerializeField] private Image OtherSetCard;
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
	private const float WaitTime = 50.0f;
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
		turn = ClientType.Local;
		before = ClientType.Local;

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

		pBeforeColor = 0;
		oBeforeColor = 0;

		Debug.Log($"This Client Type : {local.ToString()}");

		isGameOver = false;
		winner = Winner.None;
	}

	private void UpdateReady()
	{
		// ���� ���� ��ȣ ǥ�ø� ��ٸ��ϴ�.
		currentTime += Time.deltaTime;
		ProgressText.text = "Waiting to Start";

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
		turn = before;

		PlayerSetCardImage.sprite = OCardImages[pBeforeColor];
		OtherSetCard.sprite = OCardImages[oBeforeColor];

		RemainTime = TurnTime;
		ProgressText.text = $"Start Round : {RoundCount}";
		Debug.Log($"{RoundCount}���� ���� :{turn}");
		bool SetClient = false;
		if (TurnCount == 0)
		{
			isFirst = true;
			if (before == local)
			{
				SetClient = DoOwnTurn();  // ���� �Է�
			}
			else if (before == remote)
			{
				SetClient = DoOppnentTurn();  // ����� �Է�
			}
		}
		else if (TurnCount == 1)
		{
			isFirst = false;
			turn = (turn == ClientType.Local) ? ClientType.Remote : ClientType.Local;
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
		if (ClientNum > RemoteNum || (ClientNum == 0 && RemoteNum == 7))
		{
			localWin = localWin + 1;
			LocalWinCount.text = localWin.ToString();
		}
		if(RemoteNum > ClientNum || (ClientNum == 7 && RemoteNum == 0))
		{
			remoteWin = remoteWin + 1;
			RemoteWinCount.text = remoteWin.ToString();
		}

		if (winner != Winner.None)
		{
			//�¸��� ���� ����ȿ���� ���ϴ�.
			if ((winner == Winner.Own && local == ClientType.Local)
				|| (winner == Winner.Opponent && local == ClientType.Remote))
			{
				//�¸��� Ŭ���̾�Ʈ�� ���� �߰��ϴ� �Լ� ����

			}
			progress = GameProgress.Result;
		}

		// brfore�� �ش� ���� �¸��ڷ� �ٲ��� ��
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
			OtherSetCard.sprite = BCardImages[index];
			Debug.Log($"DoOppnentTurn, index:{index}");
		}
		else if (index == -1)
		{
			Debug.Log($"���ŵ� �� : {index}");
			return false;
		}

		if(PlayerManager.Instance._myPlayer.IsShowingHand == false) PlayerManager.Instance._myPlayer.ShowHand();

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
		else;
			setPacket.destinationId = (int)ClientType.Remote + 1;

		network.Send(setPacket.Write());

		TurnCount = TurnCount + 1;
		return true;
	}

	// ����� ���� ���� ó��.
	public bool DoOppnentTurn()
	{
		int index = -1;
		if(isFirst == false) index = PlayerManager.Instance.ReturnCardColor();
		if(index != -1)
		{
			OtherSetCard.sprite = BCardImages[index];
			Debug.Log($"DoOppnentTurn, index:{index}");
		}
		else if (index == -1)
		{
			Debug.Log($"���ŵ� �� : {index}");
			return false;
		}

		PlayerManager.Instance._myPlayer.ShowHand(false);
		Debug.Log("����");
		Debug.Log("Recv:" + index + " [" + network.IsServer() + "]");

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
