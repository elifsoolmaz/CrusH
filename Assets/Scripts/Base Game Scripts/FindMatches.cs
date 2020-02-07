using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  System.Linq;

public class FindMatches : MonoBehaviour
{

    public Board _board;
    public List<GameObject> _currentMatches = new List<GameObject>();
    
    
    void Start()
    {
        _board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots= new List<GameObject>();
        if (dot1.isAdjacentBomb) {
            _currentMatches.Union(GetAdjacentPieces(dot1.column,dot1.row));
        }
        if (dot2.isAdjacentBomb) {
            _currentMatches.Union(GetAdjacentPieces(dot2.column,dot2.row));
        }
        if (dot3.isAdjacentBomb) {
            _currentMatches.Union(GetAdjacentPieces(dot3.column,dot3.row));
        }
        return currentDots;
    }
    
    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
          List<GameObject> currentDots= new List<GameObject>();
          if (dot1.isRowBomb) {
              _currentMatches.Union(GetRowPieces(dot1.row));
              _board.BombRow(dot1.row);
          }
          if (dot2.isRowBomb) {
              _currentMatches.Union(GetRowPieces(dot2.row));
              _board.BombRow(dot2.row);

          }
          if (dot3.isRowBomb) {
              _currentMatches.Union(GetRowPieces(dot3.row));
              _board.BombRow(dot3.row);

          }
          return currentDots;
    }
    
    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots= new List<GameObject>();
        if (dot1.isColumnBomb) {
            _currentMatches.Union(GetColumnPieces(dot1.column));
            _board.BombColumn(dot1.column);
        }
        if (dot2.isColumnBomb) {
            _currentMatches.Union(GetColumnPieces(dot2.column));
            _board.BombColumn(dot2.column);
        }
        if (dot3.isColumnBomb) {
            _currentMatches.Union(GetColumnPieces(dot3.column));
            _board.BombColumn(dot3.column);
        }
        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!_currentMatches.Contains(dot))
        {
            _currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
       AddToListAndMatch(dot1);
       AddToListAndMatch(dot2);
       AddToListAndMatch(dot3);
    }
    
    
    private IEnumerator FindAllMatchesCo()
    {
        yield return  new WaitForSeconds(.2f);
        for (int i = 0; i < _board.width; i++)
        {
            for (int j = 0; j < _board.height; j++)
            {
                GameObject currentDot = _board.allDots[i,j];
                
                if (currentDot != null)
                {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();

                    if (i > 0 && i < _board.width - 1)
                    {
                        GameObject leftDot = _board.allDots[i - 1, j];
                        GameObject rightDot = _board.allDots[i + 1, j];
                        if (rightDot != null && leftDot != null)
                        {
                            Dot rigthDotDot = rightDot.GetComponent<Dot>();
                            Dot leftDotDot = leftDot.GetComponent<Dot>();

                            if (leftDot != null && rightDot != null)
                            {
                                if (rightDot.tag == currentDot.tag && leftDot.tag == currentDot.tag)
                                {
                                    _currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rigthDotDot));
                                    _currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rigthDotDot));
                                    _currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rigthDotDot));
                                    
                                    GetNearbyPieces(leftDot, currentDot, rightDot);
                                }
                            }
                        }
                    }
                    
                    if (j > 0 && j < _board.height - 1)
                    {
                        GameObject upDot = _board.allDots[i, j+1];
                        GameObject downDot = _board.allDots[i, j-1];
                        if (upDot != null && downDot != null)
                        {
                            Dot downDotDot = downDot.GetComponent<Dot>();
                            Dot upDotDot = upDot.GetComponent<Dot>();

                            if (upDot != null && downDot != null)
                            {
                                if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                                {
                                    _currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));
                                    _currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));
                                    _currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot));

                                    GetNearbyPieces(upDot, currentDot, downDot);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void MatchPiecesOfColor(string color)
    {
        for (int i = 0; i < _board.width; i++)
        {
            for (int j = 0; j < _board.height; j++)
            {
                //check if that pieces exists
                if (_board.allDots[i, j] != null)
                {
                    if (_board.allDots[i, j].tag == color)
                    {
                        //set that dot to be matched
                        _board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = column - 1; i < column + 1; i++) {
            for (int j = row - 1; j < row + 1; j++) {
                //check if the piece is inside the board
                if (i >= 0 && i < _board.width && j >= 0 && j < _board.height) {
                    if (_board.allDots[i, j] != null)
                    {
                        dots.Add(_board.allDots[i, j]);
                        _board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
        return dots;
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < _board.height; i++)
        {
            if (_board.allDots[column, i] != null)
            {
                Dot dot = _board.allDots[column, i].GetComponent<Dot>();
                if (dot.isRowBomb)
                {
                    dots.Union(GetRowPieces(i)).ToList();
                }
                dots.Add(_board.allDots[column,i]);
                dot.isMatched = true;
            }
        }
        return dots;
    }
    
    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < _board.width; i++)
        {
            if (_board.allDots[ i,row] != null)
            {
                Dot dot = _board.allDots[i, row].GetComponent<Dot>();
                if (dot.isColumnBomb)
                {
                    dots.Union(GetColumnPieces(i)).ToList();
                }
                dots.Add(_board.allDots[row,i]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    public void CheckBombs()
    {
        //did the player move something?
        if (_board.currentDot != null)
        {
            //is the piece they moved matched?
            if (_board.currentDot.isMatched)
            {
                 // make it unmatched
                 _board.currentDot.isMatched = false;

                 /*
                  //bombs-1
                 //decide what kind of bomb to make
                 int typeOfBomb = Random.Range(0, 100);
                 if (typeOfBomb < 50)
                 {
                     //make a row bomb
                     _board.currentDot.MakeRowBomb();
                 }
                else if (typeOfBomb >= 50)
                 {
                     //make a column bomb
                     _board.currentDot.MakeColumnBomb();
                 }*/

                 if ((_board.currentDot.swipeAngle > -45 && _board.currentDot.swipeAngle <= 45) ||
                     (_board.currentDot.swipeAngle < -135 && _board.currentDot.swipeAngle >= 135)) {
                     _board.currentDot.MakeRowBomb();
                 }else {
                     _board.currentDot.MakeColumnBomb();
                 }


            }
            // is the other pieces matched?
            else if (_board.currentDot._otherDots != null)
            {
                Dot otherDot = _board.currentDot._otherDots.GetComponent<Dot>();
                //is the other dot matched.
                if (otherDot.isMatched)
                {
                    otherDot.isMatched = false;
                    
                    /*
                    //Decide what kind of bomb to make 
                    int typeOfBomb = Random.Range(0, 100);
                    if (typeOfBomb < 50)
                    {
                        //make a row bomb
                        otherDot.MakeRowBomb();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        //make a column bomb
                        otherDot.MakeColumnBomb();
                    }
                    */
                    
                    if ((otherDot.swipeAngle > -45 && otherDot.swipeAngle <= 45) ||
                        (otherDot.swipeAngle < -135 && otherDot.swipeAngle >= 135)) {
                        otherDot.MakeRowBomb();
                    }else {
                        otherDot.MakeColumnBomb();
                    }
                } 
            }
        }
    }
    
}

