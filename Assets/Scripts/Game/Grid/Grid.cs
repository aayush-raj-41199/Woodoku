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
                gridSquares[gridSquares.Count - 1].transform.SetParent(this.transform);
                gridSquares[gridSquares.Count - 1].transform.localScale = new Vector3(squareScale, squareScale, squareScale);
                gridSquares[gridSquares.Count - 1].GetComponent<GridSquare>().SetImage(squareIndex % 2 == 0);
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
        foreach (var square in gridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();

            if(gridSquare.CanWeUseThisSquare() == true)
            {
                gridSquare.ActivateSquare();
            }
        }
    }

}
