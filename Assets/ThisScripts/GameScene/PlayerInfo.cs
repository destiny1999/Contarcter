using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] public string userId { get; set; }
    [SerializeField] public int winTurnCount { get; set; }
    [SerializeField] public int score { get; set; }
    [SerializeField] public int winGameCount { get; set; }
    [SerializeField] public bool showCharacter { get; set; }
    [SerializeField] public int professionCode { get; set; }
    [SerializeField] GameObject skillFire;
    [SerializeField] List<GameObject> allSkills;


    public void SetSkillFire(GameObject fireObject)
    {
        skillFire = fireObject;
    }
    public GameObject GetSkillFire()
    {
        return skillFire;
    }
}
