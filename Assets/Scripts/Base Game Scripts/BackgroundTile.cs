using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public int hitPoints;
    private SpriteRenderer _sprite;
    private GoalManager _goalManager;

    public void Start()
    {
        _goalManager = FindObjectOfType<GoalManager>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (hitPoints <= 0)
        {
            if (_goalManager != null)
            {
                _goalManager.CompareGoal(this.gameObject.tag);
                _goalManager.UpdateGoals();
            }
            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;
        MakeLighter();
    }

    void MakeLighter()
    {
        // take the current color
        Color color = _sprite.color;
        
        //get the current color's alpha value
        float newAlpha = color.a * 0.5f;
        _sprite.color= new Color(color.r, color.g, color.b, newAlpha);
    }
}