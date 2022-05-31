using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiteManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject siteMark;
    bool cardIn = false;
    Animator markAni;

    public static SiteManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        markAni = siteMark.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cardIn)
        {
            markAni.SetBool("cardIn", true);
        }
        else
        {
            markAni.SetBool("cardIn", false);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        cardIn = true;
        if (collision.transform.CompareTag("card"))
        {
            collision.transform.GetComponent<CardSetting>().SetUse(true);
        }
        else if (collision.transform.CompareTag("skill"))
        {
            collision.transform.GetComponent<SkillInfo>().SetUse(true);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        cardIn = false;
        if (collision.transform.CompareTag("card"))
        {
            collision.transform.GetComponent<CardSetting>().SetUse(false);
        }
        else if (collision.transform.CompareTag("skill"))
        {
            collision.transform.GetComponent<SkillInfo>().SetUse(false);
        }
    }
    public void SetcardIn(bool status)
    {
        cardIn = status;
    }
}
