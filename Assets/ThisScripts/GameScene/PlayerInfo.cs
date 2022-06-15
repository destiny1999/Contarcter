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
    [SerializeField] public int willToGraveValue { get; set; }
    [SerializeField] GameObject skillFire;
    [SerializeField] List<GameObject> allSkills;

    HashSet<int> handCards = new HashSet<int>();
    HashSet<int> graveCards = new HashSet<int>();
    List<string> swapUserIds = new List<string>();
    private void Start()
    {
        for(int i = 1; i <=10; i++)
        {
            handCards.Add(i);
        }
    }
    public void SetSkillFire(GameObject fireObject)
    {
        skillFire = fireObject;
    }
    public GameObject GetSkillFire()
    {
        return skillFire;
    }
    public List<HashSet<int>> GetEverywhereCards()
    {
        List<HashSet<int>> all = new List<HashSet<int>>();
        all.Add(handCards);
        all.Add(graveCards);
        return all;
    }
    public List<string> GetSwapUserIds()
    {
        return swapUserIds;
    }
    public void ResetSwapUserIds()
    {
        swapUserIds = new List<string>();
    }
    public void SetSwapUserId(string userId)
    {
        swapUserIds.Add(userId);
        if(swapUserIds.Count == 2)
        {
            GameManager.Instance.SetOtherActionOKToSetSkillCard(0);
        }
    }
    public void UseCard(int value)
    {
        handCards.Remove(value);
        willToGraveValue = value;
    }
    public void SendToGrave()
    {
        graveCards.Add(willToGraveValue);
        willToGraveValue = -1;
    }
}
