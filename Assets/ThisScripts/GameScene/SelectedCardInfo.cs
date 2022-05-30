using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCardInfo : MonoBehaviour
{
    [SerializeField] Sprite cardBack;
    [SerializeField] SpriteRenderer background;
    [SerializeField] SpriteRenderer frame;
    [SerializeField] SpriteRenderer valueMark;
    [SerializeField] public int value { get; set; }
    [SerializeField] string userID;

    public void SetCardBack()
    {
        background.sprite = cardBack;
    }
}
