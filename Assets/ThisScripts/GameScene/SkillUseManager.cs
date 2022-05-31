using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUseManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static SkillUseManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void UseSkill(string skillName)
    {
        switch (skillName)
        {
            case "replace":
                break;

        }
    }

}
