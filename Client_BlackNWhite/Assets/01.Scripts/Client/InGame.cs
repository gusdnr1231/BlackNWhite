using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame : MonoBehaviour
{
    private enum GameProgress
    {
        None = 0,
        Ready = 1,
        Turn = 2,
        Result = 3,
        RoundWin = 4,
        RoundLose = 5,
        GameWin = 4,
        GameLose = 5,
        Disconnect = 8
    }
}
