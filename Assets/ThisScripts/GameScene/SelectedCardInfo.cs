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
    int changeValue = 0;
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
    public void SetChangeValue(int value)
    {
        changeValue = value;
    }
    public int GetChangeValue()
    {
        return changeValue;
    }
    public void ChangeShowValue(int newValue, Sprite spriteMark)
    {
        value = newValue;
        valueMark.sprite = spriteMark;
    }
    public int GetValue()
    {
        return value;
    }
    public void SetCard(Sprite frameSprite, Sprite valueMarkSprite)
    {
        frame.sprite = frameSprite;
        valueMark.sprite = valueMarkSprite;
        background.sprite = cardBack;
    }
    public IEnumerator ShowCard(bool last)
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
        if (last)
        {
            // this waitTime mean when card show after N second deal with skill
            // if no skill it will immediate into final step;
            float waitTime = 3f;
            while(waitTime > 0)
            {
                waitTime -= Time.deltaTime * 1;
                yield return 1;
            }
            GameManager.Instance.SetAnimationOKToMaster();
        }
    }
}
