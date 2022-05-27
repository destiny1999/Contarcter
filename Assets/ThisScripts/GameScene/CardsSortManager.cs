using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CardsSortManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float startX;
    [SerializeField] float distanceWithSideCard;



    public void SortCard()
    {
        print(this.transform.childCount);
    }
    
}
