using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

[System.Serializable]
public class BlankGoal
{
    public int numberNeeded;
    public int numberCollected;
    public Sprite goalSprite;
    public string matchvalue;
}


public class GoalManager : MonoBehaviour
{

    public BlankGoal[] levelGoals;
    public List<GoalPanel> currentGoals= new List<GoalPanel>();
    public GameObject goalPrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;
    private Board _board;
    private EndGameManager _endGame;

    // Start is called before the first frame update
    void Start()
    {
        _board = FindObjectOfType<Board>();
        _endGame = FindObjectOfType<EndGameManager>();
        GetGoals();
        SetupGoals();
    }

    void GetGoals()
    {
        if (_board != null)
        {
            if (_board.world != null)
            {
                if (_board.level < _board.world.levels.Length)
                {
                    if (_board.world.levels[_board.level] != null)
                    {
                        levelGoals = _board.world.levels[_board.level].levelGoals;
                        for (int i = 0; i < levelGoals.Length; i++)
                        {
                            levelGoals[i].numberCollected = 0;
                        }
                    }
                }
            }
        }
    }

    void SetupGoals()
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            // create a new goal at the goalIntoParent position
            GameObject goal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform);
            
            //Set the image and text of the goal..
            GoalPanel panel = goal.GetComponent<GoalPanel>();
            panel.thisSprite = levelGoals[i].goalSprite;
            panel.thisString = "0 /" + levelGoals[i].numberNeeded;
            
            //create a new goal panel at the goalGameParent position
            GameObject gameGoal =  Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            gameGoal.transform.SetParent(goalGameParent.transform);
            panel = gameGoal.GetComponent<GoalPanel>();
            currentGoals.Add(panel); 
            panel.thisSprite = levelGoals[i].goalSprite;
            panel.thisString = "0 /" + levelGoals[i].numberNeeded;
            
        }
    }

    public void UpdateGoals()
    {
        int goalsCompleted=0;
        for (int i = 0; i < levelGoals.Length; i++)
        {
            currentGoals[i].thisText.text = "" + levelGoals[i].numberCollected + " / " + levelGoals[i].numberNeeded;
            if (levelGoals[i].numberCollected >= levelGoals[i].numberNeeded)
            {
                goalsCompleted++;
                currentGoals[i].thisText.text= "" + levelGoals[i].numberNeeded + " / " + levelGoals[i].numberNeeded;
            }
        }

        if (goalsCompleted >= levelGoals.Length)
        {
            if (_endGame != null)
            {
                _endGame.WinGame();
                Debug.Log("WIN!");

            }
            
        }
    }

    public void CompareGoal(string goalToCompare)
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            if (goalToCompare == levelGoals[i].matchvalue)
            {
                levelGoals[i].numberCollected++;
            }
        }
    }

}
