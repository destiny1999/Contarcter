using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using HashTable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using System.Linq;
public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] double prepareTime;
    bool prepareStep = true;
    double startTimeValue;
    HashTable startTime = new HashTable();
    HashTable currentTime = new HashTable();
    [SerializeField] Text timer;

    [SerializeField] int siteCode;
    [SerializeField] Text siteDescription;
    [SerializeField] List<Material> allCharactersMaterial;
    [SerializeField] Material selfCharacterMaterial;
    [SerializeField] Material leftCharacterMaterial;
    [SerializeField] Material rightCharacterMaterial;
    [SerializeField] List<Sprite> allSkillSprites;
    [SerializeField] List<GameObject> skills;

    [SerializeField] List<Sprite> allCardValueSprite;
    [SerializeField] List<Sprite> allProfessionCardBackground;
    [SerializeField] List<Sprite> allSiteMarkSprite;
    [SerializeField] List<Sprite> allSiteBackground;
    [SerializeField] GameObject background;
    [SerializeField] GameObject siteMark;
    [SerializeField] PhotonView photonView;

    [SerializeField] GameObject decks;

    HashTable professionInfo;
    int characterCode;

    // Start is called before the first frame update
    void Start()
    {
        professionInfo = (HashTable)PhotonNetwork.LocalPlayer.CustomProperties["professionInfo"];
        characterCode = (int)professionInfo["professsion"];

        prepareStep = true;
        if (PhotonNetwork.IsMasterClient)
        {
            // prepare time
            startTimeValue = PhotonNetwork.Time;
            StartCoroutine(SubTime());
            // set site info and wait all player loading OK
            int siteCode = Random.Range(0, 1);
            photonView.RPC("SetSiteInfo", RpcTarget.All, siteCode);
        }
        SetSelfPlayerData();
    }
    [PunRPC]
    public void SetSiteInfo(int siteCode)
    {
        background.GetComponent<SpriteRenderer>().sprite = allSiteBackground[siteCode];
        siteMark.GetComponent<SpriteRenderer>().sprite = allSiteMarkSprite[siteCode];
    }
    void SetSelfPlayerData()
    {
        //set character sprite, skills and cards
        selfCharacterMaterial = allCharactersMaterial[characterCode];

        string skill1 = (string)professionInfo["skill1"];
        string skill1Description = (string)professionInfo["skill1Description"];
        string skill2 = (string)professionInfo["skill2"];
        string skill2Description = (string)professionInfo["skill2Description"];
        skills[0].GetComponent<SpriteRenderer>().sprite = UseSkillNameGetSprite(skill1);
        skills[1].GetComponent<SpriteRenderer>().sprite = UseSkillNameGetSprite(skill2);

        SetCards();
    }
    void SetCards()
    {
        var cards = decks.GetComponentsInChildren<Transform>()
            .Where(child => child.GetComponent<CardSetting>()).ToArray();

        int index = 0;
        foreach(Transform card in cards)
        {
            card.GetComponent<CardSetting>().SetFrame(allProfessionCardBackground[characterCode]);
            card.GetComponent<CardSetting>().SetValue(index + 1, allCardValueSprite[index]);
            index++;
        }

    }
    Sprite UseSkillNameGetSprite(string spriteName)
    {
        Sprite targetSprite = null;
        foreach(Sprite sprite in allSkillSprites)
        {
            if(sprite.name == spriteName)
            {
                targetSprite = sprite;
                break;
            }
        }
        return targetSprite;
    }
    IEnumerator SubTime()
    {
        while(prepareTime > 0 && prepareStep)
        {
            var currentTimeValue = PhotonNetwork.Time;
            var passTime = currentTimeValue - startTimeValue;

            var remainingTime = prepareTime - passTime;
            if (!currentTime.ContainsKey("prepareStepRemainingTime"))
            {
                currentTime.Add("prepareStepRemainingTime", remainingTime);
            }
            else
            {
                currentTime["prepareStepRemainingTime"] = remainingTime;
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(currentTime);
            yield return 1;
        }
    }
    public override void OnRoomPropertiesUpdate(HashTable propertiesThatChanged)
    {
        if (prepareStep)
        {
            if (propertiesThatChanged["prepareStepRemainingTime"] != null)
            {
                var remainingTime = (int)(double)propertiesThatChanged["prepareStepRemainingTime"];
                timer.text = remainingTime + "";
            }
        }
    }
}
