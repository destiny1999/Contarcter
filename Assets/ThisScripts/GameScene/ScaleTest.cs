using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleTest : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> test;
    private void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetActiveToFalseTest();
        }
    }
    void SetActiveToFalseTest()
    {
        int index = 0;
        foreach(GameObject gameObject in test)
        {
            if(index == 1)
            {
                gameObject.SetActive(false);
            }
            index++;
        }
    }
}
