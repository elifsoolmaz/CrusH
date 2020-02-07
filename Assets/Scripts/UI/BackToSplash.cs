using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToSplash : MonoBehaviour
{
    public string sceneToLoad;
    private GameData _gameData;
    private Board _board;

    
    public void WinOK()
    {
        if (_gameData != null)
        {
            _gameData.saveData.isActive[_board.level+1]= true;
            _gameData.Save();
        }
        SceneManager.LoadScene(sceneToLoad);
    }
    
    public void LoseOK()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
    
    void Start()
    {
        _gameData = FindObjectOfType<GameData>();
        _board = FindObjectOfType<Board>();
        
    }

    void Update()
    {
        
    }
}
