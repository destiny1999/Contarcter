using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public class CardsSortManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float localY;
    [SerializeField] float localZ;
    [SerializeField] float startX;
    [SerializeField] float distanceWithSideCard;
    [SerializeField] List<GameObject> allCards;
    public static CardsSortManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    public void InitialallCards()
    {
        foreach(GameObject card in allCards)
        {
            card.GetComponent<CardSetting>().InitialThisCard();
        }
        SortCard();
    }
    public void SortCard()
    {
        int index = 0;
        Transform[] cards = this.transform.GetComponentsInChildren<Transform>().
            Where(child => child.GetComponent<CardSetting>()).ToArray();
        foreach(Transform card in cards)
        {
            card.localPosition =
                                new Vector3(
                                            startX + distanceWithSideCard * index,
                                            localY,
                                            localZ
                                            );
            card.GetComponent<CardSetting>().SetOriginalPosition();
            index++;
        }
    }
    public void SetCardCanBeDraggingStatus(bool status)
    {
        Transform[] cards = this.transform.GetComponentsInChildren<Transform>().
            Where(child => child.GetComponent<CardSetting>()).ToArray();
        foreach (Transform card in cards)
        {
            card.GetComponent<CardSetting>().SetCanDragStatus(status);
        }
    }
}
