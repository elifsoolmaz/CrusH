using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum GameType
{
    Moves,
    Time
}

[System.Serializable]
public class EndGameRequirements
{
    public GameType gameType;
    public int counterValue;
}

public class EndGameManager : MonoBehaviour
{
    public GameObject movesLabel;
    public GameObject timeLabel;
    public Text counter;
    public int currentCounterValue;
    
    private float timerSeconds;
    private Board _board;
    
    public GameObject youWinPanel;
    public GameObject tryAgainPanel;
    public EndGameRequirements requirements;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _board = FindObjectOfType<Board>();
        SetGameType();
        SetUpGame();
    }

    void SetGameType()
    {
        if (_board.world != null)
        {
            if (_board.level < _board.world.levels.Length)
            {
                if (_board.world.levels[_board.level] != null)
                {
                    requirements = _board.world.levels[_board.level].EndGameRequirements;
                }
            }
        }
    }
    
    void SetUpGame()
    {
        currentCounterValue = requirements.counterValue;
        if (requirements.gameType == GameType.Moves)
        {
            movesLabel.SetActive(true);
            timeLabel.SetActive(false);
        }
        else
        {
            timerSeconds = 1;
            movesLabel.SetActive(false);
            timeLabel.SetActive(true);
        }

        counter.text = "" + currentCounterValue;
    }

    //time counter
    public void DecreaseCounterValue()
    {
        if (_board.currentState != GameState.pause)
        {
            currentCounterValue--;
            counter.text = "" + currentCounterValue;
            if (currentCounterValue <= 0)
            {
                LoseGame(); 
            }
        }
    }

    public void WinGame()
    {
        youWinPanel.SetActive(true);
        _board.currentState = GameState.win;
        Debug.Log("WIN!");
        currentCounterValue = 0;
        counter.text = "" + currentCounterValue;
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();
    }

    public void LoseGame()
    {
        tryAgainPanel.SetActive(true);
        _board.currentState = GameState.lose;
        Debug.Log("LOSE!");
        currentCounterValue = 0;
        counter.text = "" + currentCounterValue;
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();
    }
    
    
    private void Update()
    {
        if (requirements.gameType == GameType.Time && currentCounterValue >0)
        {
            timerSeconds -= Time.deltaTime;
            if (timerSeconds <= 0)
            {
                DecreaseCounterValue();
                timerSeconds = 1;
            }
        }
    }
}
