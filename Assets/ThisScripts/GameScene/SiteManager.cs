using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiteManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject siteMark;
    bool cardIn = false;
    Animator markAni;
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
        collision.transform.GetComponent<CardSetting>().SetUse(true);
    }
    private void OnCollisionExit(Collision collision)
    {
        cardIn = false;
        collision.transform.GetComponent<CardSetting>().SetUse(false);
    }
}
