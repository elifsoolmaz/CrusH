using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pasuePanel;
    private Board _board;
    public bool paused = false;
    public Image soundButton;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    private SoundManager _sound;
    
    
    void Start()
    {
        _sound = FindObjectOfType<SoundManager>();
        _board = FindObjectOfType<Board>();
        pasuePanel.SetActive(false);

        //In Player Prefs, the "Sound" key is for sound
        // If sound ==0, then mute, if sound ==1, then unmute
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                soundButton.sprite = musicOffSprite;
            }
            else
            {
                soundButton.sprite = musicOnSprite;
            }
        }
        else
        {
            soundButton.sprite = musicOnSprite;
        }
    }

    void Update()
    {
        if (paused && !pasuePanel.activeInHierarchy)
        {
            pasuePanel.SetActive(true);
            _board.currentState = GameState.pause;
        }

        if (!paused && pasuePanel.activeInHierarchy)
        {
            pasuePanel.SetActive(false);
            _board.currentState = GameState.move;
        }
    }

    public void Sound()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                PlayerPrefs.SetInt("Sound",1);
                soundButton.sprite = musicOnSprite;
                _sound.AdjustVolume();
            }
            else
            {
                PlayerPrefs.SetInt("Sound",0);
                soundButton.sprite = musicOffSprite;
                _sound.AdjustVolume();
            }
        }
        else
        {
            PlayerPrefs.SetInt("Sound",1);
            soundButton.sprite = musicOffSprite;
            _sound.AdjustVolume();

        }
    }

    public void PauseGame()
    {
        paused = !paused;
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Splash");
    }
}
