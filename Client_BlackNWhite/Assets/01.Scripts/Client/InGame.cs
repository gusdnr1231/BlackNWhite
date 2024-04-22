using DummyClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.VersionControl.Asset;

public class InGame : MonoBehaviour
{
    private enum GameProgress
    {
        None = 0,
        Ready = 1,
		StartRound = 2,
        Turn = 3,
        EndRound = 4,
        Result = 5,
		GameOver = 6,
        Disconnect = 7
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

    private NetworkManager network = null;
    private ClientType local;
    private ClientType remote;
    private ClientType turn;
	private ClientType before;

	// ���� ��Ȳ.
	private GameProgress progress;
	private Winner winner;

	private Coroutine TimerCoroutine;
	private float currentTime;
	private bool isGameOver;

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

        Debug.Log($"This Client Type : {local.ToString()}");

        isGameOver = false;
        winner = Winner.None;
    }

	private void UpdateReady()
	{
		// ���� ���� ��ȣ ǥ�ø� ��ٸ��ϴ�.
		currentTime += Time.deltaTime;
		//Debug.Log("UpdateReady");

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
		RemainTime = TurnTime;
		Debug.Log($"{RoundCount}���� ���� :{turn}");
		bool SetClient = false;
		if(TurnCount == 0)
		{
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
			//��ȣ�� ���̴� ���� ȿ���� ���ϴ�. 
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

		RemainTime -= Time.deltaTime;
		if (RemainTime <= 0.0f)
		{
			// Ÿ�ӿ���.
			RemainTime = 0.0f;
			// ���� Ŭ���̾�Ʈ�� ī�� �� �����ϰ� 1�� ������
		}
		else
		{
			// ���콺�� ���� ��ư�� ���� ���¸� �����մϴ�.
			bool isClicked = Input.GetMouseButtonDown(0);
			if (isClicked == false)
			{
				// �������� �ʾ����Ƿ� �ƹ��͵� ���� ���� �ʽ��ϴ�.
				return false;
			}

			Vector3 pos = Input.mousePosition;
			Debug.Log("POS:" + pos.x + ", " + pos.y + ", " + pos.z);

			// ������ ������ �������� ���õ� ĭ���� ��ȯ�մϴ�.
			Debug.Log($"Ŭ�� ��ȯ�� : {index}");
			if (index < 0)
			{
				// ���� ���� ���õǾ����ϴ�.
				return false;
			}
		}

		// ĭ�� �Ӵϴ�.
		bool ret = SetMarkToSpace(index, localMark);
		if (ret == false)
		{
			// �� �� �����ϴ�.
			return false;
		}

		C_MoveStone movePacket = new C_MoveStone();
		movePacket.select = index;      // ���ð�
										// ������id ����
		if (local == ClientType.Local)    // �����ΰ��
			movePacket.destinationId = (int)Mark.Cross + 1;
		else
			movePacket.destinationId = (int)Mark.Circle + 1;

		network.Send(movePacket.Write());

		TurnCount = TurnCount + 1;
		return true;
	}

	// ����� ���� ���� ó��.
	public bool DoOppnentTurn()
	{
		// ����� ������ �����մϴ�.
		int index = PlayerManager.Instance.returnStone();
		Debug.Log($"DoOppnentTurn, index:{index}");

		if (index <= 0)
		{
			// ���� ���ŵ��� �ʾҽ��ϴ�.
			Debug.Log($"���ŵ� �� : {index}");
			return false;
		}

		// ������� �� Ŭ���̾�Ʈ��� ���� �����մϴ�.
		ClientType thisClient = (network.IsServer() == true) ? ClientType.Remote : ClientType.Local;
		Debug.Log("����");

		// ������ ������ ���õ� ĭ���� ��ȯ�մϴ�. 
		Debug.Log("Recv:" + index + " [" + network.IsServer() + "]");

		// ĭ�� �Ӵϴ�.
		bool ret = SetMarkToSpace(index, remoteMark);
		if (ret == false)
		{
			// �� �� ����.
			Debug.Log("�Ѽ�����.");
			return false;
		}

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

	private IEnumerator Timer(float TimerTime)
    {
        currentTime = TimerTime;
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime -= 1;
        }
        
        if(currentTime <= 0)
        {
            winner = Winner.Opponent;
            progress = GameProgress.Turn;
        }
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