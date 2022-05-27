using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSetting : MonoBehaviour
{
    Vector3 screenPoint;
    Vector3 offset;
    Vector3 originalPosition;
    bool use = false;
    [SerializeField] int value;
    [SerializeField] SpriteRenderer valueMark;
    [SerializeField] SpriteRenderer frame;
    private void Start()
    {
        SetOriginalPosition();
    }
    public void OnMouseDown()
    {
        print("mouse down");
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint
            (new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }
    private void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, 
                                             Input.mousePosition.y, 
                                             screenPoint.z);

        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = new Vector3(curPosition.x, curPosition.y, -2);
    }
    private void OnMouseUp()
    {
        if (!use) BackToOriginalPosition();
    }
    void SetOriginalPosition()
    {
        originalPosition = this.transform.localPosition;
    }
    void BackToOriginalPosition()
    {
        this.transform.localPosition = originalPosition;
    }
    public void SetFrame(Sprite frameSprite)
    {
        frame.sprite = frameSprite;
    }
    public void SetValue(int value, Sprite valueMark)
    {
        this.value = value;
        this.valueMark.sprite = valueMark;
    }
}
