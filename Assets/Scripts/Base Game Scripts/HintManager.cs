using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    private Board _board;
    private float _hintDelaySeconds;

    public float hintDelay;
    public GameObject hintParticle;
    public GameObject currentHint;
    
    void Start()
    {
        _board = FindObjectOfType<Board>();
        _hintDelaySeconds = hintDelay;
    }

    void Update()
    {
        _hintDelaySeconds -= Time.deltaTime;
        if (_hintDelaySeconds <= 0 && currentHint == null)
        {
            MarkHint();
            _hintDelaySeconds = hintDelay;
        }
    }
    
    //First, I want to find all possible matches on the board
    private List<GameObject> FindAllMatches()
    {
        List<GameObject> possibleMoves= new List<GameObject>();
        
        for (int i = 0; i < _board.width; i++)
        {
            for (int j = 0; j < _board.height; j++)
            {
                if (_board.allDots[i, j] != null)
                {
                    if (i < _board.width - 1)
                    {
                        if (_board.SwitchAndCheck(i, j, Vector2.right))
                        {
                            possibleMoves.Add(_board.allDots[i,j]);
                        }
                    }
                    if (j < _board.height - 1)
                    {
                        if (_board.SwitchAndCheck(i, j, Vector2.up))
                        {
                            possibleMoves.Add(_board.allDots[i,j]);

                        } 
                    }
                }
            }
        }
        return possibleMoves;
    }
    
    
    //Pick one of those matches randomly
    GameObject PickOneRandomly()
    {
        List<GameObject> possibleMoves= new List<GameObject>();
        possibleMoves = FindAllMatches();
        if (possibleMoves.Count > 0)
        {
            int pieceToUse = Random.Range(0, possibleMoves.Count);
            return possibleMoves[pieceToUse];
        }
        return null;
    }
    
    
    //Create the hint behind the chosen match
    private void MarkHint()
    {
        GameObject move = PickOneRandomly();
        if (move != null)
        {
            currentHint = Instantiate(hintParticle, move.transform.position, Quaternion.identity);
        }
    }
    
    //Destroy the hint..
    public void DestroyHint()
    {
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
            _hintDelaySeconds = hintDelay;
        }
    }
    
    
}
