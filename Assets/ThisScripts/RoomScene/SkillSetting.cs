using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSetting : MonoBehaviour
{
    [SerializeField] int skillOnwer; // -1 all
    [SerializeField] int skillCode;
    [SerializeField] string skillDescription;
    [SerializeField] string skillName;
    [SerializeField] Image skillImage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnClickSkill()
    {
        int skillIndex = RoomSceneManager.Instance.GetSkillIndex();
        RoomSceneManager.Instance.SetSelectedSkill(skillIndex, skillCode, skillImage.sprite);
    }
}
