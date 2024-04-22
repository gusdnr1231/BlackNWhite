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
	#endregion

	#region Server Variables
	private NetworkManager network = null;
    private ClientType local;
    private ClientType remote;
    private ClientType turn;
	private ClientType before;
	#endregion

	// 진행 상황.
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
		// 시합 시작 신호 표시를 기다립니다.
		currentTime += Time.deltaTime;
		ProgressText.text = "Waiting to Start";

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

		PlayerSetCardImage.sprite = OCardImages[pBeforeColor];
		OtherSetCard.sprite = OCardImages[oBeforeColor];

		RemainTime = TurnTime;
		ProgressText.text = $"Start Round : {RoundCount}";
		Debug.Log($"{RoundCount}라운드 시작 :{turn}");
		bool SetClient = false;
		if(TurnCount == 0)
		{
			isFirst = true;
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
			isFirst = false;
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
			// 카드가 놓이는 사운드 효과를 냅니다. 
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
		if (!isFirst)
		{
			index = PlayerManager.Instance.ReturnCard().Color;
			OtherSetCard.sprite = BCardImages[index];
			Debug.Log($"DoOppnentTurn, index:{index}");

			if (index <= 0)
			{
				Debug.Log($"수신된 값 : {index}");
				return false;
			}
		}

		RemainTime -= Time.deltaTime;
		if (RemainTime <= 0.0f)
		{
			// 타임오버.
			RemainTime = 0.0f;
			// 현재 클라이언트의 카드 중 랜덤하게 1장 버리기
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

	// 상대의 턴일 때의 처리.
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
				Debug.Log($"수신된 값 : {index}");
				return false;
			}
		}

		ClientType thisClient = (network.IsServer() == true) ? ClientType.Remote : ClientType.Local;
		Debug.Log("수신");

		// 수신한 정보를 선택된 칸으로 변환합니다. 
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