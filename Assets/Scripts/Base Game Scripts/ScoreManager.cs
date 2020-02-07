using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

    private Board board;
    public Text scoreText;
    public int score;
    public Image scoreBar;
    private GameData _gameData;
    private int numberStars;
    
    
    void Start () {
        board = FindObjectOfType<Board>();
        _gameData = FindObjectOfType<GameData>();
        UpdateBar();
    }
	
    void Update () {
        scoreText.text = "" + score;
    }

    public void IncreaseScore(int amountToIncrease)
    {
        score += amountToIncrease;
        
        //editing star number
        for (int i = 0; i < board.scoreGoals.Length; i++)
        {
            if (score > board.scoreGoals[i] && numberStars < i + 1)
            {
                numberStars++;
            }
        }
        
        if (_gameData != null)
        {
            int highScore = _gameData.saveData.highScores[board.level];
            if (score > highScore)
            {
                _gameData.saveData.highScores[board.level] = score;
            }
            
            int currentStarts=_gameData.saveData.stars[board.level];
            if (numberStars > currentStarts)
            {
                _gameData.saveData.stars[board.level] = numberStars;
            }

            _gameData.Save();
        }
        UpdateBar();
    }
    

    private void UpdateBar()
    {
        if (board != null && scoreBar != null)
        {
			
            int length = board.scoreGoals.Length;
          
            scoreBar.fillAmount = (float)score / (float)board.scoreGoals[length - 1];


        }
    }
}