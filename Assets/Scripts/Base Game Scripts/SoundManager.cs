using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public AudioSource[] destroyNoise;
    public AudioSource backgroundMusic;

    private void Start()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                backgroundMusic.Play();
                backgroundMusic.volume = 0;
            }
            else
            {
                backgroundMusic.Play();
                backgroundMusic.volume = 0.2f;
            }
        }
        else
        {
            backgroundMusic.Play();
            backgroundMusic.volume = 0.2f;

        }
    }

    public void AdjustVolume()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                backgroundMusic.volume = 0;
            }
            else
            {
                backgroundMusic.volume = 0.2f;
            }
        }
    }
    
    public void PlayRandomDestroyNoise()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 1)
            {
                //choose a random number
                int clipToPlay = Random.Range(0, destroyNoise.Length);
        
                //play that clip
                destroyNoise[clipToPlay].Play();
            }
        }
        else
        {
            //choose a random number
            int clipToPlay = Random.Range(0, destroyNoise.Length);
        
            //play that clip
            destroyNoise[clipToPlay].Play();
        }
    }
    
}
