using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using HashTable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using System.Linq;
using Photon.Realtime;
public class GameManager : MonoBehaviourPunCallbacks
{
    int playerNums = 3;
    [SerializeField] int testPlayerNuums;
    [SerializeField] bool testMode = false;
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

    Dictionary<string, Player> useUserIdGetPlayer = new Dictionary<string, Player>();

    HashTable professionInfo;
    int characterCode;

    public static GameManager Instance;

    [SerializeField] List<PlayerInfo> allPlayersInfo;
    Dictionary<string, GameObject> useUserIdGetPlayerGameObject = new Dictionary<string, GameObject>();

    [SerializeField] List<GameObject> allSkillsFire;

    int loadingOKPlayer = 0;

    [SerializeField] double cardSettedConsiderTime = 30;
    int settedCards = 0;// each turn how many player setted card

    [SerializeField] double skillSettedConsiderTime = 10;

    [SerializeField] List<GameObject> skillSettedOrder;

    // 0 card selected consider, 1 skilled selected consider
    [SerializeField] List<bool> eachStepsStatus;
    private void Awake()
    {
        Instance = this;
        if (testMode) playerNums = testPlayerNuums;
    }
    // Start is called before the first frame update
    void Start()
    {
        professionInfo = (HashTable)PhotonNetwork.LocalPlayer.CustomProperties["professionInfo"];
        characterCode = (int)professionInfo["professsion"];

        prepareStep = true;
        if (PhotonNetwork.IsMasterClient)
        {
            // prepare time
            //startTimeValue = PhotonNetwork.Time;
            //StartCoroutine(SubTime());
            // set site info and wait all player loading OK

            for(int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                useUserIdGetPlayer.Add(PhotonNetwork.CurrentRoom.Players[i+1].UserId,
                    PhotonNetwork.CurrentRoom.Players[i+1]);
            }

            int siteCode = Random.Range(0, 1);
            photonView.RPC("SetSiteInfo", RpcTarget.All, siteCode);
            StartCoroutine(WaitAllPlayerLoading());
        }
        SetSelfPlayerData();
        SetAllPlayerInfo();
    }
    IEnumerator WaitAllPlayerLoading()
    {
        // wait all player loading OK.
        while(loadingOKPlayer < playerNums)
        {
            yield return 1;
        }
        print("all player OK");
        TurnStart();
    }
    void TurnStart()
    {
        print("turn start");
        // only master client into this
        StartCoroutine(SubTime(cardSettedConsiderTime, 0));
        photonView.RPC("SetCardCanBeSettedStatus", RpcTarget.All, true);
    }
    void SetAllPlayerInfo()
    {
        int localIndex = -1;
        for(int i = 0; i<playerNums; i++)
        {
            allPlayersInfo[i].GetComponent<PlayerInfo>().userId =
                PhotonNetwork.CurrentRoom.Players[i+1].UserId;
            if(PhotonNetwork.LocalPlayer.UserId == PhotonNetwork.CurrentRoom.Players[i+1].UserId)
            {
                localIndex = i;
            }
            allPlayersInfo[i].GetComponent<PlayerInfo>().winGameCount = 0;
            allPlayersInfo[i].GetComponent<PlayerInfo>().winTurnCount = 0;
            allPlayersInfo[i].GetComponent<PlayerInfo>().score = 0;
            allPlayersInfo[i].GetComponent<PlayerInfo>().showCharacter = false;
        }
        if(localIndex != 0)
        {
            string id = allPlayersInfo[0].GetComponent<PlayerInfo>().userId;
            allPlayersInfo[0].GetComponent<PlayerInfo>().userId =
                PhotonNetwork.LocalPlayer.UserId;
            allPlayersInfo[localIndex].GetComponent<PlayerInfo>().userId = id;
        }
        for(int i = 0; i< playerNums; i++)
        {
            useUserIdGetPlayerGameObject.Add(
                allPlayersInfo[i].GetComponent<PlayerInfo>().userId,
                allPlayersInfo[i].gameObject);
        }
        photonView.RPC("SetLoadingOK", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void SetLoadingOK()
    {
        loadingOKPlayer++;
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
    public void SetSelectedCard(int value)
    {
        string[] sendValue = new string[2];
        sendValue[0] = PhotonNetwork.LocalPlayer.UserId;
        sendValue[1] = value+"";
        photonView.RPC("SetSelectedCardToAll", RpcTarget.All, sendValue);
    }
    [PunRPC]
    public void SetSelectedCardToAll(string[] cardInfo)
    {
        // set the card in SelectedCardInfo(script)
        // this script contain userId, value, animation
        // each player contain its selectedCardInfo at the first
        // so can know which player win this turn
        // if master client should judge the setted nums
        // if setted nums == 3, go to next part, "judge"

        string id = cardInfo[0];
        int value = int.Parse(cardInfo[1]);

        useUserIdGetPlayerGameObject[id].GetComponentInChildren<SelectedCardInfo>().value = value;
        useUserIdGetPlayerGameObject[id].GetComponentInChildren<SelectedCardInfo>().SetCardBack();
        settedCards++;
        if (PhotonNetwork.IsMasterClient)
        {
            if(settedCards == playerNums)
            {
                eachStepsStatus[0] = false;
                photonView.RPC("SetSkillCanBeSettedStatus", RpcTarget.All, true);
                photonView.RPC("SetCardCanBeSettedStatus", RpcTarget.All, false);
                ToSetSkillStep();
                settedCards = 0;
            }
        }
    }
    [PunRPC]
    public void SetCardCanBeSettedStatus(bool status)
    {
        CardsSortManager.Instance.SetCardCanBeDraggingStatus(status);
    }
    [PunRPC]
    public void SetSkillCanBeSettedStatus(bool status)
    {
        SkillInfo[] allSkills = useUserIdGetPlayerGameObject[PhotonNetwork.LocalPlayer.UserId].
            GetComponentsInChildren<SkillInfo>();

        for(int i = 0; i<allSkills.Length; i++)
        {
            allSkills[i].SetDraggingStatus(status);
        }
    }
    void ToSetSkillStep()
    {
        // only master client can into this function
        // deal with skill setted, should had skill animation manager, play with setted order
        // show card
        // deal with skill
        // finall compare the result card
        // set the result to all player
        // go to next turn
        eachStepsStatus[1] = true;
        StartCoroutine(JudgeSetSkillStatus());
        print("sub skill time and the time = " + skillSettedConsiderTime);
        StartCoroutine(SubTime(skillSettedConsiderTime, 1));
    }
    IEnumerator JudgeSetSkillStatus()
    {
        skillSettedOrder = new List<GameObject>();
        while (eachStepsStatus[1])
        {
            if(skillSettedOrder.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                eachStepsStatus[1] = false;
            }
            yield return 1;
        }
    }
    [PunRPC]
    public void SettedSkill(string[] skillInfo)
    {
        // client show fire and master should record the order
        string id = skillInfo[0];
        string skillName = skillInfo[1];
        int skillFireCode = int.Parse(skillInfo[2]);

        Transform skillSettedPosition = useUserIdGetPlayerGameObject[id].
            transform.Find("SkillFirePosition");

        GameObject skillFire = GameObject.Instantiate
            (allSkillsFire[skillFireCode], skillSettedPosition);

        if (PhotonNetwork.IsMasterClient)
        {
            skillSettedOrder.Add(skillFire);
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

    IEnumerator SubTime(double countDownTime, int stepIndex)
    {
        eachStepsStatus[stepIndex] = true;
        startTimeValue = PhotonNetwork.Time;
        var remainingTime = countDownTime;
        print("countDownTime = " + countDownTime);
        while (remainingTime > 0 && eachStepsStatus[stepIndex])
        {
            var currentTimeValue = PhotonNetwork.Time;
            var passTime = currentTimeValue - startTimeValue;
            print("passTime = " + passTime);
            remainingTime = countDownTime - passTime;
            print("remaingTime = " + remainingTime);
            if (!currentTime.ContainsKey("remainingTime"))
            {
                currentTime.Add("remainingTime", remainingTime);
            }
            else
            {
                currentTime["remainingTime"] = remainingTime;
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(currentTime);
            yield return 1;
        }
        eachStepsStatus[stepIndex] = false;
        //photonView.RPC("SetSkillCanBeSettedStatus", RpcTarget.All, false);
    }
    public override void OnRoomPropertiesUpdate(HashTable propertiesThatChanged)
    {
        if (propertiesThatChanged["remainingTime"] != null)
        {
            var remainingTime = (int)(double)propertiesThatChanged["remainingTime"];
            timer.text = remainingTime + "";
        }
    }
}
