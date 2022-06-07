using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillSetting : MonoBehaviour
{
    [SerializeField] int skillOnwer; // -1 all, -2 fake skill
    [SerializeField] int skillCode;
    [SerializeField] string skillDescription;
    [SerializeField] string skillName;
    [SerializeField] string spriteName;
    [SerializeField] Sprite skillImage;
    // Start is called before the first frame update
    void Start()
    {
        skillImage = this.GetComponentsInChildren<Image>().ToList()[1].sprite;
        spriteName = skillImage.name;
    }

    public void OnClickSkill()
    {
        int skillIndex = RoomSceneManager.Instance.GetSkillIndex();
        RoomSceneManager.Instance.SetSelectedSkill(skillIndex, skillCode, skillImage, spriteName, skillDescription, skillOnwer);
        this.gameObject.SetActive(false);
    }
    public int GetSkillOnwer()
    {
        return skillOnwer;
    }
    public void ShowSkillInfo()
    {
        RoomSceneManager.Instance.ShowSkillInfoView(skillName, skillDescription);
    }
    public void HideSkillInfo()
    {
        RoomSceneManager.Instance.HideSkillInfoView();
    }
}
