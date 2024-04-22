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


	// 턴 세기용 변수
	private int RoundCount;
	private int TurnCount;
	private float StepCount = 0;
	// 턴 진행시 대기 시간 할당용 변수
    private float RemainTime = 0f;
	// 시합 시작 전의 신호표시 시간.
	private const float WaitTime = 50.0f;
	// 대기 시간.
	private const float TurnTime = 30.0f;

    private NetworkManager network = null;
    private ClientType local;
    private ClientType remote;
    private ClientType turn;
	private ClientType before;

	// 진행 상황.
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
		// 시합 시작 신호 표시를 기다립니다.
		currentTime += Time.deltaTime;
		//Debug.Log("UpdateReady");

		if (currentTime > WaitTime)
		{
			// 게임 시작입니다.
			progress = GameProgress.StartRound;
		}
	}

	private void UpdateRound()
	{
		RoundCount = RoundCount + 1;
		TurnCount = 0;
		turn = before;
		RemainTime = TurnTime;
		Debug.Log($"{RoundCount}라운드 시작 :{turn}");
		bool SetClient = false;
		if(TurnCount == 0)
		{
			if (before == local)
			{
				SetClient = DoOwnTurn();  // 내턴 입력
			}
			else if (before == remote)
			{
				SetClient = DoOppnentTurn();  // 상대턴 입력
			}
		}
		else if (TurnCount == 1)
		{
			turn = (turn == ClientType.Local) ? ClientType.Remote : ClientType.Local;
			if (turn == local)
			{
				SetClient = DoOwnTurn();  // 내턴 입력
			}
			else if (turn == remote)
			{
				SetClient = DoOppnentTurn();  // 상대턴 입력
			}
		}

		if (SetClient == false)
		{
			return;
		}
		else
		{
			//기호가 놓이는 사운드 효과를 냅니다. 
		}

		if (winner != Winner.None)
		{
			//승리한 경우는 사운드효과를 냅니다.
			if ((winner == Winner.Own && local == ClientType.Local)
				|| (winner == Winner.Opponent && local == ClientType.Remote))
			{
				//승리한 클라이언트에 점수 추가하는 함수 실행
			}
			progress = GameProgress.Result;
		}

		// 턴을 갱신합니다.
		Debug.Log($"라운드 갱신 :{turn}");
		if(TurnCount == 2)
		{
			progress = GameProgress.EndRound;
		}
	}

	private void UpdateData()
	{
		// 각 클라이언트 별 숫자 받고, 대조해 라운드 승리자 조회
		// brfore을 해당 라운드 승리자로 바꿔줄 것
		progress = GameProgress.StartRound;
	}

	private void UpdateGameOver()
	{
		StepCount += Time.deltaTime;
		if (StepCount > 3.0f)
		{
			// 게임을 종료합니다.
			ResetGame();
			isGameOver = true;
		}
	}

	// 자신의 턴일 때의 처리
	public bool DoOwnTurn()
	{
		int index = 0;

		RemainTime -= Time.deltaTime;
		if (RemainTime <= 0.0f)
		{
			// 타임오버.
			RemainTime = 0.0f;
			// 현재 클라이언트의 카드 중 랜덤하게 1장 버리기
		}
		else
		{
			// 마우스의 왼쪽 버튼의 눌린 상태를 감시합니다.
			bool isClicked = Input.GetMouseButtonDown(0);
			if (isClicked == false)
			{
				// 눌려지지 않았으므로 아무것도 하지 않지 않습니다.
				return false;
			}

			Vector3 pos = Input.mousePosition;
			Debug.Log("POS:" + pos.x + ", " + pos.y + ", " + pos.z);

			// 수신한 정보를 바탕으로 선택된 칸으로 변환합니다.
			Debug.Log($"클릭 변환값 : {index}");
			if (index < 0)
			{
				// 범위 밖이 선택되었습니다.
				return false;
			}
		}

		// 칸에 둡니다.
		bool ret = SetMarkToSpace(index, localMark);
		if (ret == false)
		{
			// 둘 수 없습니다.
			return false;
		}

		C_MoveStone movePacket = new C_MoveStone();
		movePacket.select = index;      // 선택값
										// 목적지id 구분
		if (local == ClientType.Local)    // 서버인경우
			movePacket.destinationId = (int)Mark.Cross + 1;
		else
			movePacket.destinationId = (int)Mark.Circle + 1;

		network.Send(movePacket.Write());

		TurnCount = TurnCount + 1;
		return true;
	}

	// 상대의 턴일 때의 처리.
	public bool DoOppnentTurn()
	{
		// 상대의 정보를 수신합니다.
		int index = PlayerManager.Instance.returnStone();
		Debug.Log($"DoOppnentTurn, index:{index}");

		if (index <= 0)
		{
			// 아직 수신되지 않았습니다.
			Debug.Log($"수신된 값 : {index}");
			return false;
		}

		// 서버라면 ○ 클라이언트라면 ×를 지정합니다.
		ClientType thisClient = (network.IsServer() == true) ? ClientType.Remote : ClientType.Local;
		Debug.Log("수신");

		// 수신한 정보를 선택된 칸으로 변환합니다. 
		Debug.Log("Recv:" + index + " [" + network.IsServer() + "]");

		// 칸에 둡니다.
		bool ret = SetMarkToSpace(index, remoteMark);
		if (ret == false)
		{
			// 둘 수 없다.
			Debug.Log("둘수없다.");
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

	// 이벤트 발생 시의 콜백 함수.
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