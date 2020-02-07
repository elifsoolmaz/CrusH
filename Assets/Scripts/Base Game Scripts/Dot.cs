using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int targetX;
    public int targetY;
    public int previousColumn;
    public int previousRow;
    public bool isMatched = false;

    private EndGameManager _endGameManager;
    private HintManager _hintManager;
    private FindMatches _findMatches;
    private Board _board;
    public GameObject _otherDots;
    private Vector2 _firstTouchPosition;
    private Vector2 _finalTouchPosition;
    private Vector2 tempPosition;
    
    [Header("Swipe Stuff")]
    public float swipeAngle;
    public float swipeResist=1f;


    [Header("Poweup Stuff")] 
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isColorBomb;
    public bool isAdjacentBomb;
    public GameObject adjacentMarker;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorbomb;
    
    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        _endGameManager = FindObjectOfType<EndGameManager>();
        _board = FindObjectOfType<Board>();
        _hintManager = FindObjectOfType<HintManager>();
        
        // withTag kullanmak , oyunu ofType kullanmaktan daha hızlı yapar.
        _board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
        _findMatches = FindObjectOfType<FindMatches>();

        //targetX = (int) transform.position.x;
        //targetY = (int) transform.position.y;
        //row = targetY;
        //column = targetX;
        //previousColumn = column;
        //previousRow = row;

    }
    
    
    //This is for testing and debug only
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }


    // Update is called once per frame
    void Update()
    {
        /*
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color= new Color(1f,1f,1f, 0.2f);
        }
        */
        
        targetX = column;
        targetY = row;
        
        // X
        if (Math.Abs(targetX - transform.position.x) > 0.1f)
        {
            //move towards the target
            tempPosition= new Vector2(targetX,transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.6f);
            if (_board.allDots[column, row] != this.gameObject)
            {
                _board.allDots[column, row] = this.gameObject;
            }
             _findMatches.FindAllMatches();
        }
        else
        {
            //directly set the position
            tempPosition= new Vector2(targetX,transform.position.y);
            transform.position = tempPosition;
        }
        
        // Y
       if (Math.Abs(targetY - transform.position.y) > 0.1f)
        {
            //move towards the target
            tempPosition= new Vector2(transform.position.x,targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.6f);
            if (_board.allDots[column, row] != this.gameObject)
            {
                _board.allDots[column, row] = this.gameObject;
            }
            _findMatches.FindAllMatches();

        }
        else
        {
            //directly set the position
            tempPosition= new Vector2(transform.position.x,targetY);
            transform.position = tempPosition;
        }
            
    }

    public IEnumerator CheckMoveCo()
    {
        if (isColorBomb)
        {
            //this piece is a color bomb, and the other piece is the color to destroy
            _findMatches.MatchPiecesOfColor(_otherDots.tag);
            isMatched = true;
        }
        else if (_otherDots.GetComponent<Dot>().isColorBomb)
        {
            //the pther piece is a color bomb, and this piece the color to destroy
            _findMatches.MatchPiecesOfColor(this.gameObject.tag);
            _otherDots.GetComponent<Dot>().isMatched = true;
        }
        yield return  new WaitForSeconds(0.5f);
        if (_otherDots != null)
        {
            if (!isMatched && !_otherDots.GetComponent<Dot>().isMatched)
            {
                _otherDots.GetComponent<Dot>().row = row;
                _otherDots.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(0.5f);
                _board.currentDot = null;
                _board.currentState = GameState.move;
            }
            else
            {
                if (_endGameManager != null)
                {
                    if (_endGameManager.requirements.gameType == GameType.Moves)
                    {
                        _endGameManager.DecreaseCounterValue();
                    }
                }
                _board.DestroyMatches();
            }
            //_otherDots = null;
        }
    }

    private void OnMouseDown()
    {
        // Destroy the hint
        if (_hintManager != null)
        {
            _hintManager.DestroyHint();
        }
        if (_board.currentState == GameState.move)
        {
            _firstTouchPosition = Camera.main.ScreenToWorldPoint((Input.mousePosition));
        }
    }

    private void OnMouseUp()
    { 
        if(_board.currentState== GameState.move)
        {
            _finalTouchPosition= Camera.main.ScreenToWorldPoint((Input.mousePosition));
            CalculateAngle();
        
        }
    }

    
    void CalculateAngle()
    {
        if (Mathf.Abs(_finalTouchPosition.y - _firstTouchPosition.y) > swipeResist || Mathf.Abs(_finalTouchPosition.x - _firstTouchPosition.x) > swipeResist) {
            _board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(_finalTouchPosition.y - _firstTouchPosition.y, _finalTouchPosition.x - _firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            
            _board.currentDot = this;

        }
        else
        {
            _board.currentState = GameState.move;
        }
    }

    void MovePiecesActual(Vector2 direction)
    {
        _otherDots = _board.allDots[column + (int)direction.x,  row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        
        if (_board.lockTiles[column, row] == null && _board.lockTiles[column + (int) direction.x, row + (int) direction.y] == null)
        {
            if (_otherDots != null)
            {
                _otherDots.GetComponent<Dot>().column += -1 * (int) direction.x;
                _otherDots.GetComponent<Dot>().row += -1 * (int) direction.y;
                column += (int) direction.x;
                row += (int) direction.y;

                StartCoroutine(CheckMoveCo());
            }
            else
            {
                _board.currentState = GameState.move;
            }
        }
        else
        {
            _board.currentState = GameState.move;
        }
    }
    
    
    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column <_board.width-1)
        {
            //right swipe
            MovePiecesActual(Vector2.right);

            /*
            _otherDots = _board.allDots[column + 1, row];
            previousColumn = column;
            previousRow = row;
            _otherDots.GetComponent<Dot>().column -= 1;
            column += 1;
            StartCoroutine(CheckMoveCo());
            */
            
        }
        else  if (swipeAngle > 45 && swipeAngle <= 135 && row <_board.height-1)
        {
            //Up swipe
            MovePiecesActual(Vector2.up);

            /*
            _otherDots = _board.allDots[column, row+1];
            previousColumn = column;
            previousRow = row;
            _otherDots.GetComponent<Dot>().row -= 1;
            row += 1;
            StartCoroutine(CheckMoveCo());*/

        }
        else  if ((swipeAngle > 135  || swipeAngle <= -135 ) && column>0)
        {
            //Left swipe
            MovePiecesActual(Vector2.left);

            /*
            _otherDots = _board.allDots[column - 1, row];
            previousColumn = column;
            previousRow = row;
            _otherDots.GetComponent<Dot>().column += 1;
            column -= 1;
            StartCoroutine(CheckMoveCo());
            */

        }
        else  if (swipeAngle < -45 && swipeAngle >= -135 && row >0)
        {
            //down swipe
            MovePiecesActual(Vector2.down);

            /*
            _otherDots = _board.allDots[column, row-1];
            previousColumn = column;
            previousRow = row;
            _otherDots.GetComponent<Dot>().row += 1;
            row -= 1;
            StartCoroutine(CheckMoveCo());
            */
            
        }else
             _board.currentState = GameState.move;

    }

    void FindMatches()
    {
        if (column > 0 && column < _board.width - 1)
        {
            GameObject leftDot1 = _board.allDots[column - 1, row];
            GameObject rightDot1 = _board.allDots[column + 1, row];
          if(leftDot1 != null  && rightDot1!=null){
            if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
            {
                leftDot1.GetComponent<Dot>().isMatched = true;
                rightDot1.GetComponent<Dot>().isMatched = true;
                isMatched = true;
            }
          }
        }
        if (row > 0 && row < _board.height - 1)
        {
            GameObject uptDot1 = _board.allDots[column , row+1];
            GameObject downtDot1 = _board.allDots[column, row-1];
            if(uptDot1!= null && downtDot1 != null){
                if (uptDot1.tag == this.gameObject.tag && downtDot1.tag == this.gameObject.tag) {
                    uptDot1.GetComponent<Dot>().isMatched = true;
                    downtDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    public void MakeRowBomb()
    {
       if (!isColumnBomb && !isColorBomb && !isAdjacentBomb)
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }
    
    public void MakeColumnBomb()
    {
        if (!isRowBomb && !isColorBomb && !isAdjacentBomb)
        {
            isColumnBomb = true;
            GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }
    
    public void MakeAdjacentBomb()
    {
        if (!isRowBomb && !isColumnBomb && !isColorBomb)
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }
    
    public void MakeColorBomb(){
        if (!isRowBomb && !isColumnBomb && !isColorBomb)
        {
            isColorBomb = true;
            GameObject color = Instantiate(colorbomb, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;
            this.gameObject.tag = "Color";
        }
    }
}
