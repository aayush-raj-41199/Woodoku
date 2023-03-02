using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public ShapeStorage shapeStorage;
    public int columns = 0;
    public int rows = 0;
    public float squareGap = 0.1f;
    public GameObject gridSquare;
    public Vector2 startPosition = new Vector2(0f, 0f);
    public float squareScale = 0.5f;
    public float squareOffset = 0f;

    private Vector2 offset = new Vector2(0f, 0f);
    private List<GameObject> gridSquares = new List<GameObject>();
    private LineIndicator lineIndicator;

    private void OnEnable()
    {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;
    }

    private void OnDisable()
    {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
    }

    private void Start()
    {
        lineIndicator = GetComponent<LineIndicator>();
        CreateGrid();
    }

    private void CreateGrid()
    {
        SpawnGridSquares();
        SetGridSquarePosition();
    }

    private void SpawnGridSquares()
    {
        int squareIndex = 0;

        for(var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                gridSquares.Add(Instantiate(gridSquare) as GameObject);
                gridSquares[gridSquares.Count - 1].GetComponent<GridSquare>().squareIndex = squareIndex;
                gridSquares[gridSquares.Count - 1].transform.SetParent(this.transform);
                gridSquares[gridSquares.Count - 1].transform.localScale = new Vector3(squareScale, squareScale, squareScale);
                gridSquares[gridSquares.Count - 1].GetComponent<GridSquare>().SetImage(lineIndicator.GetGridSquareIndex(squareIndex) % 2 == 0);
                squareIndex++;
            }
        }
    }

    private void SetGridSquarePosition()
    {
        int columnNumber = 0;
        int rowNumber = 0;
        Vector2 squareGapNumber = new Vector2(0f, 0f);
        bool rowMoved = false;

        var squareRect = gridSquares[0].GetComponent<RectTransform>();

        offset.x = squareRect.rect.width * squareRect.transform.localScale.x + squareOffset;
        offset.y = squareRect.rect.height * squareRect.transform.localScale.y + squareOffset;

        foreach(GameObject square in gridSquares)
        {
            if(columnNumber + 1 > columns)
            {
                squareGapNumber.x = 0;
                columnNumber = 0;
                rowNumber++;
                rowMoved = false;
            }

            var posXOffset = offset.x * columnNumber + (squareGapNumber.x * squareGap);
            var posYOffset = offset.y * rowNumber + (squareGapNumber.y * squareGap);
            
            if (columnNumber > 0 && columnNumber % 3 == 0)
            {
                squareGapNumber.x++;
                posXOffset += squareGap;
            }
            if (rowNumber > 0 && rowNumber % 3 == 0 && rowMoved == false)
            {
                rowMoved = true;
                squareGapNumber.y++;
                posYOffset += squareGap;
            }
            square.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPosition.x + posXOffset, 
                startPosition.y - posYOffset);
            square.GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x + posXOffset,
                startPosition.y - posYOffset, 0f);
            columnNumber++;
        }
    }

    private void CheckIfShapeCanBePlaced()
    {
        var squareIndexes = new List<int>();
        foreach (var square in gridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();

            if(gridSquare.selected && !gridSquare.squareOccupied)
            {
                //gridSquare.ActivateSquare();
                squareIndexes.Add(gridSquare.squareIndex);
                gridSquare.selected = false;
            }
        }
        var currentSelectedShape = shapeStorage.GetCurrentSelectedShape();
        if (currentSelectedShape == null)
            return;
        if(currentSelectedShape.totalSquareNumber == squareIndexes.Count)
        {
            foreach(var squareIndex in squareIndexes)
            {
                gridSquares[squareIndex].GetComponent<GridSquare>().PlaceShapeOnBoard();
            }
            var shapeLeft = 0;
            foreach(var shape in shapeStorage.shapeList)
            {
                if(shape.IsOnStartPosition() && shape.IsAnyOfShapeSquareActive())
                {
                    shapeLeft++;
                }
            }
            
            if (shapeLeft == 0)
            {
                GameEvents.RequestNewShapes();
            }
            else
            {
                GameEvents.SetShapeInactive();
            }
            CheckIfAnyLineCompleted();
        }
        else
        {
            GameEvents.MoveShapeToStartPosition();
        }
    }

    private void CheckIfAnyLineCompleted()
    {
        List<int[]> lines = new List<int[]>();

        //columns
        foreach(var column in lineIndicator.columnIndexes)
        {
            lines.Add(lineIndicator.GetVerticalLine(column));
        }
        //rows
        for(var row = 0; row < 9; row++)
        {
            List<int> data = new List<int>(9);
            for(var index = 0; index < 9; index++)
            {
                data.Add(lineIndicator.lineData[row, index]);
            }
            lines.Add(data.ToArray());
        }
        //squares
        for(var square = 0; square < 9; square++)
        {
            List<int> data = new List<int>(9);
            for(var index = 0; index < 9; index++)
            {
                data.Add(lineIndicator.squareData[square, index]);
            }
            lines.Add(data.ToArray());
        }

        var completedLines = CheckIfSquaresAreCompleted(lines);

        if(completedLines > 2)
        {

        }
        var totalScores = 10 * completedLines;
        GameEvents.AddScores(totalScores);
        CheckIfPlayerLost();
    }

    private int CheckIfSquaresAreCompleted(List<int[]> data)
    {
        List<int[]> completedLines = new List<int[]>();

        var linesCompleted = 0;

        foreach(var line in data)
        {
            var lineCompleted = true;
            foreach(var squareIndex in line)
            {
                var comp = gridSquares[squareIndex].GetComponent<GridSquare>();
                if(comp.squareOccupied == false)
                {
                    lineCompleted = false;
                }
            }

            if(lineCompleted)
            {
                completedLines.Add(line);
            }
        }

        foreach(var line in completedLines)
        {
            var completed = false;
            foreach(var squareIndex in line)
            {
                var comp = gridSquares[squareIndex].GetComponent<GridSquare>();
                comp.Deactivate();
                completed = true;
            }
            foreach (var squareIndex in line)
            {
                var comp = gridSquares[squareIndex].GetComponent<GridSquare>();
                comp.ClearOccupied();
            }
            if(completed)
            {
                linesCompleted++;
            }
        }
        return linesCompleted;
    }

    private void CheckIfPlayerLost()
    {
        var validShapes = 0;
        for(var index = 0; index < shapeStorage.shapeList.Count; index++)
        {
            var isShapeActive = shapeStorage.shapeList[index].IsAnyOfShapeSquareActive();
            if (CheckIfShapeCanBePlacedOnGrid(shapeStorage.shapeList[index]) && isShapeActive)
            {
                shapeStorage.shapeList[index]?.ActivateShape();
                validShapes++;
            }
        }

        if(validShapes == 0)
        {
            //GameEvents.GameOver(false);
            Debug.Log("GameOver");
        }
    }

    private bool CheckIfShapeCanBePlacedOnGrid(Shape currentShape)
    {
        var currentShapeData = currentShape.currentShapeData;
        var shapeColumns = currentShapeData.columns;
        var shapeRows = currentShapeData.rows;

        List<int> originalShapeFilledUpSquares = new List<int>();
        var squareIndex = 0;
        for (var rowIndex = 0; rowIndex < shapeRows; rowIndex++)
        {
            for(var columnIndex = 0; columnIndex < shapeColumns; columnIndex++)
            {
                if (currentShapeData.board[rowIndex].column[columnIndex])
                {
                    originalShapeFilledUpSquares.Add(squareIndex);
                }
                squareIndex++;
            }
        }

        if(currentShape.totalSquareNumber != originalShapeFilledUpSquares.Count)
            Debug.LogError("Number of filled up squares are not the same as the original shape have ");

        var squareList = GetAllSquaresCombinations(shapeColumns, shapeRows);

        bool canBePlaced = false;
        foreach(var number in squareList)
        {
            bool shapeCanBePlacedOnTheBoard = true;
            foreach(var squareIndexToCheck in originalShapeFilledUpSquares)
            {
                var comp = gridSquares[number[squareIndexToCheck]].GetComponent<GridSquare>();
                if(comp.squareOccupied)
                {
                    shapeCanBePlacedOnTheBoard = false;
                }
            }
            if (shapeCanBePlacedOnTheBoard)
            {
                canBePlaced = true;
            }
        }
        return canBePlaced;
    }

    private List<int[]> GetAllSquaresCombinations(int columns, int rows)
    {
        var squareList = new List<int[]>();
        var lastColumnIndex = 0;
        var lastRowIndex = 0;

        int safeIndex = 0;

        while(lastRowIndex + (rows -1) < 9)
        {
            var rowData = new List<int>();
            for(var row = lastRowIndex; row < lastRowIndex + rows; row++)
            {
                for(var column = lastColumnIndex; column < lastColumnIndex + columns; column++)
                {
                    rowData.Add(lineIndicator.lineData[row, column]);
                }
            }

            squareList.Add(rowData.ToArray());
            lastColumnIndex++;
            if(lastColumnIndex +(columns - 1) >= 9)
            {
                lastRowIndex++;
                lastColumnIndex = 0;
            }

            safeIndex++;
            if(safeIndex > 100)
            {
                break;
            }
        }

        return squareList;
    }

}
