using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal,
    Lock,
    Concrete,
    Slime
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}


public class Board : MonoBehaviour
{
    [Header("Scriptable Object Stuff")]
    public World world;
    public int level;
    public GameState currentState = GameState.move;
    
    [Header("Board Dimensions")]
    public int width;
    public int height;
    public int offSet;
    
    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject destroyEffect;
    public GameObject[] dots;
    public GameObject[,] allDots;
    public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject slimePiecePrefab;
    
    [Header("Layout")]
    public TileType[] boardLayout;

    private bool[,] _blankSpaces;
    private BackgroundTile[,] _breakableTiles;
    public BackgroundTile[,] lockTiles;
    private BackgroundTile[,] concreteTiles;
    private BackgroundTile[,] slimeTiles;
    public Dot currentDot;
    public int basePieceValue=20;
    private int _streakValue = 1;
    

    private FindMatches _findMatches;
    private ScoreManager _scoreManager;
    private SoundManager _soundManager;
    private GoalManager _goalManager;
    
    public float refillDelay = 0.5f;
    public int[] scoreGoals;
    private bool makeSlime=true;


    public void Awake()
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if (world != null)
        {
            if (level< world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
        }
    }

    void Start()
    {
        _goalManager = FindObjectOfType<GoalManager>();
        _soundManager = FindObjectOfType<SoundManager>();
        _scoreManager = FindObjectOfType<ScoreManager>();
        _breakableTiles= new BackgroundTile[width,height];
        lockTiles= new BackgroundTile[width,height];
        concreteTiles= new BackgroundTile[width,height];
        slimeTiles= new BackgroundTile[width,height];
        _findMatches = FindObjectOfType<FindMatches>();
        _blankSpaces = new bool[width, height];
        allDots= new GameObject[width,height];
        SetUp();
        currentState = GameState.pause;
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
            {
                //create a "jelly" tile at that position
                _blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;

            }
        }
    }

    public void GenerateBreakableTiles()
    {       
        // look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if tile is a "jelly" tile

            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                //create a "jelly" tile at that position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                _breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateLockTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if tile is a "lock" tile
            if (boardLayout[i].tileKind == TileKind.Lock)
            {
                //create a "lock" tile at that position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }
    
    private void GenerateConcreteTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if tile is a "concrete" tile
            if (boardLayout[i].tileKind == TileKind.Concrete)
            {
                //create a "concrete" tile at that position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }
    
    private void GenerateSlimeTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if tile is a "slime" tile
            if (boardLayout[i].tileKind == TileKind.Slime)
            {
                //create a "slime" tile at that position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(slimePiecePrefab, tempPosition, Quaternion.identity);
                slimeTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }
    
    
    private void SetUp()
    {
        GenerateBreakableTiles();
        GenerateBlankSpaces();
        GenerateLockTiles();
        GenerateConcreteTiles();
        GenerateSlimeTiles();
        
        for (int i = 0; i < width; i++)
        { 
            for (int j = 0; j < height; j++)
            { 
                if(!_blankSpaces[i,j] && !concreteTiles[i,j] && !slimeTiles[i,j])
                {
                    Vector2 tempPosition = new Vector2(i,j +offSet);
                    Vector2 tilePosition = new Vector2(i,j);
                    GameObject backgroundTile = Instantiate(tilePrefab,tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( " + i + " , " + j + " )";
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations <100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                    }
                    maxIterations = 0;
                    
                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;

                    dot.transform.parent = this.transform;
                    dot.name ="( " + i + " , " + j + " )";
                    allDots[i, j] = dot; 
                } 
            } 
        } 
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            { 
                if (allDots[column - 1, row].tag == piece.tag && allDots[column-2,row].tag == piece.tag) 
                    return true;
            }
            if (allDots[column , row-1] != null && allDots[column , row-2] != null)
            {
                if (allDots[column, row-1].tag == piece.tag && allDots[column,row-2].tag == piece.tag) 
                    return true;
            
            }
        }
        else if (column <= 1 || row <= 1) {
            if (row > 1) {
                if (allDots[column, row-1] != null && allDots[column , row-2] != null){
                    if(allDots[column,row-1].tag == piece.tag && allDots[column, row-2].tag == piece.tag){ 
                        return true; 
                    }
                }
            }
            if (column > 1) {
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
                { 
                    if(allDots[column-1,row].tag == piece.tag  && allDots[column-2, row].tag == piece.tag){ 
                        return true; 
                    }
                }
            }
        }
        return false;
    }

    /*
    private bool ColumnOrRow()
    {
        
        int numberHorizontal = 0;
        int numberVertical = 0;
        Dot firstPiece = _findMatches._currentMatches[0].GetComponent<Dot>();
        if (firstPiece != null)
        {
            foreach (GameObject currentPiece in _findMatches._currentMatches)
            {
                Dot dot = currentPiece.GetComponent<Dot>();
                if (dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if (dot.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }
        return (numberVertical == 5 || numberHorizontal == 5);
    }*/

    private int ColumnOrRow()
    {
        //Make a copy of the current marches
        List<GameObject> matchCopy = _findMatches._currentMatches as List<GameObject>;
        
        //Cycle through all of match Copy and decide if a bomb needs to be made
        for (int i = 0; i < matchCopy.Count; i++)
        {
            // Store this dot
            Dot thisDot = matchCopy[i].GetComponent<Dot>();

            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;
            
            //Cycle through the rest of the piece and compare
            for (int j = 0; j < matchCopy.Count; j++)
            {
                //Store the next dot
                Dot nextDot = matchCopy[j].GetComponent<Dot>();

                if (nextDot == thisDot)
                {
                    continue;
                }
                if (nextDot.column == thisDot.column && nextDot.CompareTag(thisDot.tag))
                {
                    columnMatch++;
                }
                if (nextDot.row == thisDot.row && nextDot.CompareTag(thisDot.tag))
                {
                    rowMatch++;
                }
                
            }
            
            //return 1 if it's a color bomb
            if (columnMatch == 4 || rowMatch == 4)
            {
                return 1;
            }
            
            //return 2 if adjacent
            if (columnMatch == 2 && rowMatch == 2)
            {
                return 2;
            }
            
            //return 3 if column or row match
            if (columnMatch == 3 || rowMatch == 3)
            {
                return 3;
            }
            
        }
        return 0;
    }

    private void CheckToMakeBombs()
    {
        /*
        if (_findMatches._currentMatches.Count == 4 || _findMatches._currentMatches.Count == 7) {
            _findMatches.CheckBombs();
        }

        if (_findMatches._currentMatches.Count == 5 || _findMatches._currentMatches.Count == 8) {
            if (ColumnOrRow())
            {
                //make a color bomb
                //is the current dot matched?
                if (currentDot != null)
                {
                    if (!currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currentDot._otherDots != null)
                        {
                            Dot otherDot = currentDot._otherDots.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
                Debug.Log("COLOR BOMB");
            }
            
            else
            {
                //make adjacent bomb
                
                //is the current dot matched?
                if (currentDot != null)
                {
                    if (!currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot._otherDots != null)
                        {
                            Dot otherDot = currentDot._otherDots.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }
                Debug.Log("ADJACENT BOMB");
            }
            
        }
        */
        if (_findMatches._currentMatches.Count > 3)
        {
            //how many objects are in findMatches currentMatches?
            int typeOfMatch = ColumnOrRow();
            if (typeOfMatch == 1)
            {
                //color bomb making
                if (currentDot != null)
                {
                    if (!currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currentDot._otherDots != null)
                        {
                            Dot otherDot = currentDot._otherDots.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            }
            
            //make adjacent bomb
            else if (typeOfMatch == 2)
            {
                //is the current dot matched?
                if (currentDot != null)
                {
                    if (!currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot._otherDots != null)
                        {
                            Dot otherDot = currentDot._otherDots.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }
            }
            
            //column or row bomb
            else if (typeOfMatch == 3)
            {
                _findMatches.CheckBombs();
            }
        }
        
    }

    public void BombRow(int row)
    {
        for (int i = 0; i < width; i++)
        {
            if (concreteTiles[i, row])
                {
                    concreteTiles[i, row].TakeDamage(1);
                    if (concreteTiles[i, row].hitPoints <= 0)
                    {
                        concreteTiles[i, row] = null;
                    }
                }
        }
    }
    
    public void BombColumn(int column)
    {
        for (int i = 0; i < width; i++)
        {
            if (concreteTiles[column,i])
                {
                    concreteTiles[column, i].TakeDamage(1);
                    if (concreteTiles[column, i].hitPoints <= 0)
                    {
                        concreteTiles[column, i] = null;
                    }
                
            }
        }
    }
    
    private void DestroyMatchesAt( int column , int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            //how many elements are in the matched pieces list from _findMatches?
            if (_findMatches._currentMatches.Count >= 4)
            { 
                CheckToMakeBombs();
            }
            
            //does a take need to break
            if (_breakableTiles[column, row] != null)
            {
                //if it does give one damage
                _breakableTiles[column, row].TakeDamage(1);
                if (_breakableTiles[column, row].hitPoints <= 0)
                {
                    _breakableTiles[column, row] = null;
                }
            }
            //lockedTiles
            if (lockTiles[column, row] != null)
            {
                //if it does give one damage
                lockTiles[column, row].TakeDamage(1);
                if (lockTiles[column, row].hitPoints <= 0)
                {
                    lockTiles[column, row] = null;
                }
            }
            DamageConcrete(column,row);
            DamageSlime(column,row);
            
            //goal
            if (_goalManager != null)
            {
                _goalManager.CompareGoal(allDots[column, row].tag.ToString());
                _goalManager.UpdateGoals();
            }
            
            
            
            //does the sound manager is exist?
            if (_soundManager != null)
            {
                _soundManager.PlayRandomDestroyNoise();
            }
            
            GameObject particle= Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            
            Destroy(allDots[column,row]);
            _scoreManager.IncreaseScore(basePieceValue * _streakValue);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i,j);
                }
            }
        }
        _findMatches._currentMatches.Clear();
        StartCoroutine(DecreaseRowCo2());
    }

    private void DamageConcrete(int column, int row)
    {
        if (column > 0)
        {
            if (concreteTiles[column - 1, row] != null)
            {
                concreteTiles[column-1,row].TakeDamage(1);
                if (concreteTiles[column-1, row].hitPoints <= 0)
                {
                    concreteTiles[column-1, row] = null;
                }
            }
        }
        
        if (column < width-1)
        {
            if (concreteTiles[column + 1, row] != null)
            {
                concreteTiles[column+1,row].TakeDamage(1);

                if (concreteTiles[column+1, row].hitPoints <= 0)
                {
                    concreteTiles[column+1, row] = null;
                }
            }
        }
        
        if (row >0)
        {
            if (concreteTiles[column, row-1] != null)
            {
                concreteTiles[column,row-1].TakeDamage(1);

                if (concreteTiles[column, row-1].hitPoints <= 0)
                {
                    concreteTiles[column, row-1] = null;
                }
            }
        }
        
        if (row <height-1)
        {
            if (concreteTiles[column, row+1] != null)
            {
                concreteTiles[column,row+1].TakeDamage(1);

                if (concreteTiles[column, row+1].hitPoints <= 0)
                {
                    concreteTiles[column, row+1] = null;
                }
            }
        }
    }

    private void DamageSlime(int column, int row)
    {
        if (column > 0)
        {
            if (slimeTiles[column - 1, row] != null)
            {
                slimeTiles[column-1,row].TakeDamage(1);
                if (slimeTiles[column-1, row].hitPoints <= 0)
                {
                    slimeTiles[column-1, row] = null;
                }

                makeSlime = false;
            }
        }
        
        if (column < width-1)
        {
            if (slimeTiles[column + 1, row] != null)
            {
                slimeTiles[column+1,row].TakeDamage(1);

                if (slimeTiles[column+1, row].hitPoints <= 0)
                {
                    slimeTiles[column+1, row] = null;
                }
                makeSlime = false;

            }
        }
        
        if (row >0)
        {
            if (slimeTiles[column, row-1] != null)
            {
                slimeTiles[column,row-1].TakeDamage(1);

                if (slimeTiles[column, row-1].hitPoints <= 0)
                {
                    slimeTiles[column, row-1] = null;
                }
                makeSlime = false;

            }
        }
        
        if (row <height-1)
        {
            if (slimeTiles[column, row+1] != null)
            {
                slimeTiles[column,row+1].TakeDamage(1);

                if (slimeTiles[column, row+1].hitPoints <= 0)
                {
                    slimeTiles[column, row+1] = null;
                }
                makeSlime = false;

            }
        }
    }


    private IEnumerator DecreaseRowCo2()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if the current spot isn't blank and is empty..
                if (_blankSpaces[i, j] && allDots[i, j] == null && !concreteTiles[i,j] && !slimeTiles[i,j])
                {
                    //loop from the space above to the top of the column.
                    for (int k = j + 1; k < height; k++)
                    {
                        //if a dot is found..
                        if (allDots[i, j] != null)
                        {
                            //move that fot to this empty space.
                            allDots[i, k].GetComponent<Dot>().row = j;
                            
                            //set that spot to be null
                            allDots[i, k] = null;
                            
                            //break out of the loop..
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f*refillDelay);
        StartCoroutine(FillBoardCo());
    }
    
    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(0.5f*refillDelay);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !_blankSpaces[i,j] && !concreteTiles[i,j] && !slimeTiles[i,j])
                {
                    Vector2 tempPosition= new Vector2(i,j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    
                    int maxIteration = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIteration <100)
                    {
                        maxIteration++;
                        dotToUse = Random.Range(0, dots.Length);
                    }
                    maxIteration = 0;
                    
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(refillDelay);
        RefillBoard();

        while (MatchesOnBoard())
        {
            _streakValue++;
            DestroyMatches();
            yield return new WaitForSeconds(2* refillDelay);

        }
        _findMatches._currentMatches.Clear();
        currentDot = null;
        CheckToMakeSlime();
        
        yield return  new WaitForSeconds(refillDelay);

        if (IsDeadlock())
        {
            ShuffleBoard();
            Debug.Log("DEADLOCKED!!!");
        }

        if (currentState != GameState.pause)
            currentState = GameState.move;
        currentState = GameState.move;
        makeSlime = true;
        _streakValue = 1;
    }

    private void CheckToMakeSlime()
    {
        //check the slime tiles array
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (slimeTiles[i, j] != null && makeSlime)
                {
                    //Call another method to make a new slime
                    MakeNewSlime();
                }
            }
        }
    }

    private Vector2 CheckForAdjacent(int column, int row)
    {
        if (column < width - 1)
        {
            if (allDots[column + 1, row])
            {
                return Vector2.right;
            }
        }
        if ( column > 0)
        {
            if (allDots[column - 1, row])
            {
                return Vector2.left;
            }
        }
        
        if (row < height - 1)
        {
            if (allDots[column, row + 1])
            {
                return Vector2.up;
            }
        }
        if ( row > 0)
        {
            if (allDots[column, row - 1])
            {
                return Vector2.down;
            }
        }
        return Vector2.zero;
    }

    private void MakeNewSlime()
    {
        bool slime = false;
        int loops = 0;
        while (!slime && loops <width*height)
        {
            int newX = Random.Range(0, width);
            int newY= Random.Range(0, height);;
            if (slimeTiles[newX, newY])
            {
                Vector2 adjacent= CheckForAdjacent(newX,newY);
                if (adjacent != Vector2.zero)
                {
                    Destroy(allDots[newX + (int)adjacent.x , newY + (int)adjacent.y] );
                    Vector2 tempPosition= new Vector2(newX + (int)adjacent.x ,newY + (int)adjacent.y );
                    GameObject tile = Instantiate(slimePiecePrefab, tempPosition, Quaternion.identity);
                    slimeTiles[newX + (int) adjacent.x, newY + (int) adjacent.y] = tile.GetComponent<BackgroundTile>();
                    slime = true;
                }
            }
            loops++;
        }
    }
    
    //switching the dot pieces.
    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        if (allDots[column + (int) direction.x, row + (int) direction.y] != null)
        {
            //take a second piece and save it in a holder
            GameObject holder = allDots[column + (int) direction.x, row + (int) direction.y] as GameObject;

            //switching the first dot to be second position.
            allDots[column + (int) direction.x, row + (int) direction.y] = allDots[column, row];

            //set the first dot to be the second dot
            allDots[column, row] = holder;
        }
    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    // make sure that one and to the right are in the board
                    if (i < width - 2)
                    {
                        //check if the dots to the right and two to the right exist
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }

                    if (j < height - 2)
                    {
                        // check if the dots above exist
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            //check if the dots to the up and down exist
                            if (allDots[i, j + 1].tag == allDots[i, j].tag && allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            } 
                        } 
                    } 
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column,row,direction);
        if (CheckForMatches())
        {
            SwitchPieces(column,row,direction);
            return true;
        }
        SwitchPieces(column,row,direction);
        return false;
    }

    private bool IsDeadlock()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        } 
                    }
                }
            }
        }
        return true;
    }

    private void ShuffleBoard()
    {
        // create a list of game objects
        List<GameObject> newBoard = new List<GameObject>();
        
        //add every pieces to this list
        for (int i = 0; i <width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i,j]);
                }
            }
        }
        // for every spot on the board
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if this spot shouldn't be blank
                if (!_blankSpaces[i, j] && !concreteTiles[i,j] && !slimeTiles[i,j])
                {
                    //pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);
                    
                    
                    //Assign the column and row to the piece
                    int maxIterations = 0;
                    
                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations <100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }
                    maxIterations = 0;
                    
                    //Make a container for the piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    
                    piece.column = i;
                    piece.row = j;
                    
                    //fill in the dots array with this new piece
                    allDots[i, j] = newBoard[pieceToUse];
                    
                    //remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }
        
        //Check if it's still deadlocked
        if (IsDeadlock())
        {
            ShuffleBoard();
        }
    }
    
    
}
 
 
 
 
 
 
 
 
 
 
 
 
 
 