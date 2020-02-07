using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    [Header("Level Information")]
    public string levelToLoad;
    public int level;
    private GameData _gameData;
    private int starsActive;
    private int highScore;
    
    [Header("UI Stuff")]
    public Image[] stars;

    public Text highScoreText;
    public Text starText;

    
    
    void OnEnable()
    {
        _gameData = FindObjectOfType<GameData>();
        LoadData();
        ActivateStars();
        SetText();
    }

    void LoadData()
    {
        if (_gameData != null)
        {
            starsActive = _gameData.saveData.stars[level - 1];
            highScore = _gameData.saveData.highScores[level - 1];
        }
    }

    void SetText()
    {
        highScoreText.text = "" + highScore;
        starText.text = "" + starsActive + "/3";
    }
    
    void ActivateStars()
    {
        //COME BACK TO THIS WHEN THE BINARY FILE IS DONE...
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = true;
        }
    }

    void Update()
    {
        
    }

   public void Cancel()
    {
        this.gameObject.SetActive(false);
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level" , level-1);
        SceneManager.LoadScene(levelToLoad);
    }
}
