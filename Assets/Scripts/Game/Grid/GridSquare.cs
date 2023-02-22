using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSquare : MonoBehaviour
{
    public Image hoverImage;
    public Image activeImage;
    public Image normalImage;
    public List<Sprite> normalImages;

    public bool selected { get; set; }
    public int squareIndex { get; set; }
    public bool squareOccupied { get; set; }

    private void Start()
    {
        selected = false;
        squareOccupied = false;
    }

    public bool CanWeUseThisSquare()
    {
        return hoverImage.gameObject.activeSelf;
    }

    public void ActivateSquare()
    {
        hoverImage.gameObject.SetActive(false);
        activeImage.gameObject.SetActive(true);
        selected = true;
        squareOccupied = true;
    }

    public void SetImage(bool setFirstImage)
    {
        normalImage.GetComponent<Image>().sprite = setFirstImage ? normalImages[1] : normalImages[0];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(false);
    }

}
