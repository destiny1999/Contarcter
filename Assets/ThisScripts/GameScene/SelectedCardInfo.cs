using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCardInfo : MonoBehaviour
{
    [SerializeField] Sprite showCardBackground;
    [SerializeField] Sprite cardBack;
    [SerializeField] SpriteRenderer background;
    [SerializeField] SpriteRenderer frame;
    [SerializeField] SpriteRenderer valueMark;
    [SerializeField] public int value { get; set; }
    [SerializeField] string userID;

    public float changeSizeSpeed;
    float originalX;
    //Animator selectedCardInfoAnimator;
    private void Awake()
    {
        //selectedCardInfoAnimator = this.GetComponent<Animator>();
    }
    private void Start()
    {
        originalX = this.transform.localScale.x;
    }
    public void SetCard(Sprite frameSprite, Sprite valueMarkSprite)
    {
        frame.sprite = frameSprite;
        valueMark.sprite = valueMarkSprite;
        background.sprite = cardBack;
    }
    public IEnumerator ShowCard()
    {
        while(this.transform.localScale.x > 0)
        {
            Vector3 scale = this.transform.localScale;
            scale.x = scale.x - Time.deltaTime * changeSizeSpeed;
            if (scale.x < 0) scale.x = 0;
            this.transform.localScale = scale;
            yield return 1;
        }

        background.sprite = showCardBackground;
        frame.transform.gameObject.SetActive(true);
        valueMark.transform.gameObject.SetActive(true);
        //selectedCardInfoAnimator.SetBool("turn", false);

        while (this.transform.localScale.x < originalX)
        {
            Vector3 scale = this.transform.localScale;
            scale.x = scale.x + Time.deltaTime * changeSizeSpeed / 2;
            if (scale.x > originalX) scale.x = originalX;
            this.transform.localScale = scale;
            yield return 1;
        }
    }
}
