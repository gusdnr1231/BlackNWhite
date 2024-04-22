using DummyClient;
using System.Collections;
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
            if(inGame == null)
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
        if(network != null )
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

        if(network.IsServer() == true)
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
			progress = GameProgress.StartRound;
		}
	}

	private void UpdateRound()
	{
		RoundCount = RoundCount + 1;
		TurnCount = 0;
		turn = before;

		PlayerSetCardImage.sprite = OCardImages[pBeforeColor];
		OtherSetCard.sprite = OCardImages[oBeforeColor];

		RemainTime = TurnTime;
		ProgressText.text = $"Start Round : {RoundCount}";
		Debug.Log($"{RoundCount}���� ���� :{turn}");
		bool SetClient = false;
		if(TurnCount == 0)
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

		if (SetClient == false)
		{
			return;
		}
		else
		{
			// ī�尡 ���̴� ���� ȿ���� ���ϴ�. 
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

		// ���� �����մϴ�.
		Debug.Log($"���� ���� :{turn}");
		if(TurnCount == 2)
		{
			progress = GameProgress.EndRound;
		}
	}

	private void UpdateData()
	{
		// �� Ŭ���̾�Ʈ �� ���� �ް�, ������ ���� �¸��� ��ȸ
		// brfore�� �ش� ���� �¸��ڷ� �ٲ��� ��
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
		int index = 0;
		if (!isFirst)
		{
			index = PlayerManager.Instance.ReturnCard().Color;
			OtherSetCard.sprite = BCardImages[index];
			Debug.Log($"DoOppnentTurn, index:{index}");

			if (index <= 0)
			{
				Debug.Log($"���ŵ� �� : {index}");
				return false;
			}
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

			pBeforeColor = PlayerManager.Instance.ReturnCard().Color;

			PlayerSetCardImage.sprite = FCardImages[pBeforeColor];
			PlayerSetCardText.text = $"{PlayerManager.Instance.ReturnCard().Number}";
			if (pBeforeColor == 1) PlayerSetCardText.color = new Vector4(0, 0, 0, 1);
			else if (pBeforeColor == 0) PlayerSetCardText.color = Vector4.one;
		}

		C_MoveStone movePacket = new C_MoveStone();
		if (local == ClientType.Local);
		else;

		network.Send(movePacket.Write());

		TurnCount = TurnCount + 1;
		return true;
	}

	// ����� ���� ���� ó��.
	public bool DoOppnentTurn()
	{
		int index = 0;
		if (!isFirst)
		{
			index = PlayerManager.Instance.ReturnCard().Color;
			OtherSetCard.sprite = BCardImages[index];
			Debug.Log($"DoOppnentTurn, index:{index}");

			if (index <= 0)
			{
				Debug.Log($"���ŵ� �� : {index}");
				return false;
			}
		}

		ClientType thisClient = (network.IsServer() == true) ? ClientType.Remote : ClientType.Local;
		Debug.Log("����");

		// ������ ������ ���õ� ĭ���� ��ȯ�մϴ�. 
		Debug.Log("Recv:" + index + " [" + network.IsServer() + "]");

		TurnCount = TurnCount + 1;
		return true;
	}

	private void ResetGame()
	{
		//turn = Turn.Own;
		turn = ClientType.Local;
		progress = GameProgress.None;
		StepCount = 0f;
		RoundCount = 0;
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