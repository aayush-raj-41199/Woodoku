using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shape : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public GameObject squareShapeImage;
    public Vector3 shapeSelectedScale;
    public Vector2 offset = new Vector2(0f, 700f);

    [HideInInspector]
    public ShapeData currentShapeData;

    public int totalSquareNumber { get; set; }

    private List<GameObject> currentShape = new List<GameObject>();
    private Vector3 shapeStartScale;
    private RectTransform transform;
    private bool shapeDraggable = true;
    private Canvas canvas;
    private Vector3 startPosition;
    private bool shapeActive = true;

    public void Awake()
    {
        shapeStartScale = this.GetComponent<RectTransform>().localScale;
        transform = this.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        shapeDraggable = true;
        startPosition = transform.localPosition;
        shapeActive = true;
    }

    private void OnEnable()
    {
        GameEvents.MoveShapeToStartPosition += MoveShapeToStartPosition;
        GameEvents.SetShapeInactive += SetShapeInactive;
    }
    private void OnDisable()
    {
        GameEvents.MoveShapeToStartPosition -= MoveShapeToStartPosition;
        GameEvents.SetShapeInactive -= SetShapeInactive;
    }

    public bool IsOnStartPosition()
    {
        return transform.localPosition == startPosition;
    }

    public bool IsAnyOfShapeSquareActive()
    {
        foreach(var square in currentShape)
        {
            if(square.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    public void DeactivateShape()
    {
        if(shapeActive)
        {
            foreach(var square in currentShape)
            {
                square?.GetComponent<ShapeSquare>().DeactivateShape();
            }
        }
        shapeActive = false;
    }

    private void SetShapeInactive()
    {
        if(IsOnStartPosition() == false && IsAnyOfShapeSquareActive())
        {
            foreach(var square in currentShape)
            {
                square.gameObject.SetActive(false);
            }
        }
    }

    public void ActivateShape()
    {
        if (!shapeActive)
        {
            foreach(var square in currentShape)
            {
                square?.GetComponent<ShapeSquare>().ActivateShape();
            }
        }
        shapeActive = true;
    }

    public void RequestNewShape(ShapeData shapeData)
    {
        transform.localPosition = startPosition;
        CreateShape(shapeData);
    }

    public void CreateShape(ShapeData shapeData)
    {
        currentShapeData = shapeData;
        totalSquareNumber = GetNumberOfSquares(shapeData);

        while (currentShape.Count <= totalSquareNumber)
        {
            currentShape.Add(Instantiate(squareShapeImage, transform) as GameObject);
        }

        foreach (var square in currentShape)
        {
            square.gameObject.transform.position = Vector3.zero;
            square.gameObject.SetActive(false);
        }

        var squareRect = squareShapeImage.GetComponent<RectTransform>();
        var moveDistance = new Vector2(squareRect.rect.width * squareRect.localScale.x,
            squareRect.rect.height * squareRect.localScale.y);
        int currentIndexInList = 0;

        for (var row = 0; row < shapeData.rows; row++)
        {
            for (var column = 0; column < shapeData.columns; column++)
            {
                if (shapeData.board[row].column[column])
                {
                    currentShape[currentIndexInList].SetActive(true);
                    currentShape[currentIndexInList].GetComponent<RectTransform>().localPosition =
                        new Vector2(GetXPositionForShapeSquare(shapeData, column, moveDistance),
                        GetYPositonForShapeSquare(shapeData, row, moveDistance));
                    currentIndexInList++;
                }
            }
        }
    }

    private float GetYPositonForShapeSquare(ShapeData shapeData, int row, Vector2 moveDistance)
    {
        float shiftOnY = 0f;

        if (shapeData.rows > 1)
        {
            if (shapeData.rows % 2 != 0)
            {
                var middleSquareIndex = (shapeData.rows - 1) / 2;
                var multiplier = (shapeData.rows - 1) / 2;

                if (row < middleSquareIndex)
                {
                    shiftOnY = moveDistance.y * 1;
                    shiftOnY *= multiplier;
                }
                else if (row > middleSquareIndex)
                {
                    shiftOnY = moveDistance.y * -1;
                    shiftOnY *= multiplier;
                }
            }
            else
            {
                var middleSquareIndex2 = (shapeData.rows == 2) ? 1 : (shapeData.rows / 2);
                var middleSquareIndex1 = (shapeData.rows == 2) ? 0 : shapeData.rows - 2;
                var multiplier = shapeData.rows / 2;

                if (row == middleSquareIndex1 || row == middleSquareIndex2)
                {
                    if (row == middleSquareIndex2)
                        shiftOnY = (moveDistance.y / 2) * -1;
                    if (row == middleSquareIndex1)
                        shiftOnY = (moveDistance.y / 2);
                }
                if (row < middleSquareIndex1 && row < middleSquareIndex2)
                {
                    shiftOnY = moveDistance.y * 1;
                    shiftOnY *= multiplier;
                }
                else if (row > middleSquareIndex1 && row > middleSquareIndex2)
                {
                    shiftOnY = moveDistance.y * -1;
                    shiftOnY *= multiplier;
                }
            }
        }
        return shiftOnY;
    }

    private float GetXPositionForShapeSquare(ShapeData shapeData, int column, Vector2 moveDistance)
    {
        float shiftOnX = 0f;

        if (shapeData.columns > 1)
        {
            if (shapeData.columns % 2 != 0)
            {
                var middleSquareIndex = (shapeData.columns - 1) / 2;
                var multiplier = (shapeData.columns - 1) / 2;
                if (column < middleSquareIndex)
                {
                    shiftOnX = moveDistance.x * -1;
                    shiftOnX *= multiplier;
                }
                else if (column > middleSquareIndex)
                {
                    shiftOnX = moveDistance.x * 1;
                    shiftOnX *= multiplier;
                }
            }
            else
            {
                var middleSquareIndex2 = (shapeData.columns == 2) ? 1 : (shapeData.columns / 2);
                var middleSquareIndex1 = (shapeData.columns == 2) ? 0 : shapeData.columns - 1;
                var multiplier = shapeData.columns / 2;

                if (column == middleSquareIndex1 || column == middleSquareIndex2)
                {
                    if (column == middleSquareIndex2)
                        shiftOnX = moveDistance.x / 2;
                    if (column == middleSquareIndex1)
                        shiftOnX = (moveDistance.x / 2) * -1;
                }
                if (column < middleSquareIndex1 && column < middleSquareIndex2)
                {
                    shiftOnX = moveDistance.x - 1;
                    shiftOnX *= multiplier;
                }
                else if (column > middleSquareIndex1 && column > middleSquareIndex2)
                {
                    shiftOnX = moveDistance.x * 1;
                    shiftOnX *= multiplier;
                }
            }
        }
        return shiftOnX;
    }

    private int GetNumberOfSquares(ShapeData shapeData)
    {
        int number = 0;
        foreach (var rowData in shapeData.board)
        {
            foreach (var active in rowData.column)
            {
                if (active)
                    number++;
            }
        }
        return number;
    }

    public void OnPointerClick(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.GetComponent<RectTransform>().localScale = shapeSelectedScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.anchorMin = new Vector2(0, 0);
        transform.anchorMax = new Vector2(0, 0);
        transform.pivot = new Vector2(0, 0);

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
            eventData.position, Camera.main, out pos);
        transform.localPosition = pos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.GetComponent<RectTransform>().localScale = shapeStartScale;
        GameEvents.CheckIfShapeCanBePlaced();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    private void MoveShapeToStartPosition()
    {
        transform.transform.localPosition = startPosition;
    }
}
