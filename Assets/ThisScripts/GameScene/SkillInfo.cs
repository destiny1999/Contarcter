using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillInfo : MonoBehaviour
{
    [SerializeField] bool canDrag = false;
    [SerializeField] bool showDetail = false;
    bool use = false;
    Vector3 originalPosition;
    Vector3 screenPoint;
    Vector3 offset;
    private void Start()
    {
        originalPosition = this.transform.localPosition;
    }
    public void SetDraggingStatus(bool status)
    {
        canDrag = status;
    }
    private void OnMouseUp()
    {
        if (!use)
        {
            this.transform.localPosition = originalPosition;
        }
        else
        {

        }
    }
    public void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint
            (new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }
    private void OnMouseDrag()
    {
        if (canDrag)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x,
                                             Input.mousePosition.y,
                                             screenPoint.z);

            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
        }
    }
    private void OnMouseEnter()
    {
        if (showDetail)
        {

        }
    }
}
