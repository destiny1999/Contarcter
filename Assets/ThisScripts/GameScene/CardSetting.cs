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
    Animator cardAni;
    int cardStatus = 0; // 0 in the hand, -1 in the grave
    bool canDrag = false;
    public bool testDrag = false;
    private void Start()
    {
        cardAni = this.GetComponent<Animator>();
        SetOriginalPosition();
    }
    public void InitialThisCard()
    {
        cardStatus = 0;
        this.gameObject.SetActive(true);
    }
    private void OnMouseEnter()
    {
        SpriteRenderer[] spriteRenders = this.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0 ; i < spriteRenders.Length; i++)
        {
            spriteRenders[i].sortingOrder = 3 + i;
        }
        cardAni.SetBool("Selected", true);
    }
    private void OnMouseExit()
    {
        SpriteRenderer[] spriteRenders = this.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < spriteRenders.Length; i++)
        {
            spriteRenders[i].sortingOrder = i;
        }
        cardAni.SetBool("Selected", false);
    }
    public void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint
            (new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }
    private void OnMouseDrag()
    {
        if (canDrag || testDrag)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x,
                                             Input.mousePosition.y,
                                             screenPoint.z);

            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
        }
    }
    public void SetCanDragStatus(bool status)
    {
        canDrag = status;
    }
    private void OnMouseUp()
    {
        if (!use) BackToOriginalPosition();
        else
        {
            GameManager.Instance.SetSelectedCard(value);
            cardStatus = -1;
            this.gameObject.SetActive(false);
            CardsSortManager.Instance.SortCard();
            SiteManager.Instance.SetcardIn(false);
        }
    }
    public void SetOriginalPosition()
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
    public void SetUse(bool status)
    {
        use = status;
    }
    public int GetValue()
    {
        return value;
    }
}
