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
    //[SerializeField] List<Material> allCharactersMaterial;
    [SerializeField] List<Sprite> allCharacterSprites;
    [SerializeField] SpriteRenderer selfCharacter;
    [SerializeField] SpriteRenderer leftCharacter;
    [SerializeField] SpriteRenderer rightCharacter;
    //[SerializeField] Material selfCharacterMaterial;
    //[SerializeField] Material leftCharacterMaterial;
    //[SerializeField] Material rightCharacterMaterial;
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

    [SerializeField] double skillSettedConsiderTime = 11;

    [SerializeField] List<GameObject> skillSettedOrder;

    // 0 card selected consider, 1 skilled selected consider
    [SerializeField] List<bool> eachStepsStatus;

    [SerializeField] Animator timerAnimator;
    int animationOK = 0;
    private void Awake()
    {
        Instance = this;
        if (testMode)
        {
            if (PhotonNetwork.IsConnected)
            {
                playerNums = PhotonNetwork.CurrentRoom.PlayerCount;
            }
            else
            {
                playerNums = testPlayerNuums;
            }
        }
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
        selfCharacter.sprite = allCharacterSprites[characterCode];
        //selfCharacterMaterial.mainTexture = allCharactersMaterial[characterCode].mainTexture;

        for(int i = 1; i<=2; i++)
        {
            string skillName = (string)professionInfo[$"skill{i}"];
            string skillDescription = (string)professionInfo[$"skill{i}Description"];
            int skillOwner = (int)professionInfo[$"skillOwner{i}"];
            skills[i - 1].GetComponent<SpriteRenderer>().sprite = UseSkillNameGetSprite(skillName);
            skills[i - 1].GetComponent<SkillInfo>().skillName = skillName;
            skills[i - 1].GetComponent<SkillInfo>().skillOwner = skillOwner;
            skills[i - 1].GetComponent<SkillInfo>().skillDescription = skillDescription;
        }
        for(int i = 2; i <5; i++)
        {
            skills[i].GetComponent<SkillInfo>().skillName = "";
            skills[i].GetComponent<SkillInfo>().skillDescription = "";
            skills[i - 1].GetComponent<SkillInfo>().skillOwner = -2;
        }

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
    public void SetSelectedSkill(string skillName, int skillOwner)
    {
        // 0 user id, 1 card name(which use for deal with skill), 2 fireMark, 3 skillOwner
        string[] sendValue = new string[3];
        sendValue[0] = PhotonNetwork.LocalPlayer.UserId;
        sendValue[1] = skillName;
        sendValue[2] = allPlayersInfo[0].showCharacter == true ? skillOwner+"" : 6+"";
        sendValue[3] = skillOwner+"";
        photonView.RPC("SetSelectedSkillToAll", RpcTarget.All, sendValue);
        SetSkillCanBeSettedStatus(false);
    }
    [PunRPC]
    public void SetSelectedSkillToAll(string[] skillInfo)
    {
        string id = skillInfo[0];
        string name = skillInfo[1];
        int fireMark = int.Parse(skillInfo[2]);
        int owner = int.Parse(skillInfo[3]);

        Transform skillFirePosition = useUserIdGetPlayerGameObject[id].
            transform.Find("SkillFirePosition");

        GameObject skillFire = Instantiate(allSkillsFire[fireMark], skillFirePosition);
        skillFire.transform.localPosition = Vector3.zero;
        skillFire.name = name + owner;
        if (PhotonNetwork.IsMasterClient)
        {
            skillSettedOrder.Add(skillFire);
        }
        
    }
    public void SetSelectedCard(int value)
    {
        string[] sendValue = new string[3];
        sendValue[0] = PhotonNetwork.LocalPlayer.UserId;
        sendValue[1] = value+"";
        sendValue[2] = characterCode + "";
        photonView.RPC("SetSelectedCardToAll", RpcTarget.All, sendValue);
        SetCardCanBeSettedStatus(false);
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
        int cardProfessionCode = int.Parse(cardInfo[2]);

        useUserIdGetPlayerGameObject[id].GetComponentInChildren<SelectedCardInfo>().value = value;

        int frameCode = useUserIdGetPlayerGameObject[id].GetComponent<PlayerInfo>().showCharacter
                            == true ?
                            useUserIdGetPlayerGameObject[id].GetComponent<PlayerInfo>().professionCode :
                            6;

        Sprite frameSprite = allProfessionCardBackground[frameCode];

        useUserIdGetPlayerGameObject[id].GetComponentInChildren<SelectedCardInfo>()
            .SetCard(frameSprite, allCardValueSprite[value-1]);

        // this may had a animation about set card animation

        settedCards++;
        if (PhotonNetwork.IsMasterClient)
        {
            if(settedCards == playerNums)
            {
                eachStepsStatus[0] = false;
                StartCoroutine(ToSetSkillStep());
                
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
    IEnumerator ToSetSkillStep()
    {
        // call all start animation
        // only master client can into this function
        // deal with skill setted, should had skill animation manager, play with setted order
        // show card
        // deal with skill
        // finall compare the result card
        // set the result to all player
        // go to next turn
        var waitTime = 1f;
        while(waitTime > 0)
        {
            waitTime -= Time.deltaTime * 1;
            yield return 1;
        }

        photonView.RPC("StartTimerAnimation", RpcTarget.All, 0);
        while(animationOK < playerNums)
        {
            //wating for animation OK...
            yield return 1;
        }
        animationOK = 0;
        print("animation OK");
        eachStepsStatus[1] = true;
        StartCoroutine(JudgeSetSkillStatus());
        photonView.RPC("SetSkillCanBeSettedStatus", RpcTarget.All, true);
        photonView.RPC("SetCardCanBeSettedStatus", RpcTarget.All, false);

        eachStepsStatus[1] = true;
        StartCoroutine(SubTime(skillSettedConsiderTime, 1));
        
        
    }
    /// <summary>
    /// if code == 0, from summon to skill, if code == 1, from skill to summon
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [PunRPC]
    IEnumerator StartTimerAnimation(int code)
    {
        string animationStatus = code == 0 ? "setSkill" : "summon";
        while (timerAnimator.GetCurrentAnimatorStateInfo(0).IsName("None"))
        {
            timerAnimator.SetBool(animationStatus, true);
            yield return 1;
        }
        while (timerAnimator.GetCurrentAnimatorStateInfo(0).IsName("None"))
        {
            yield return 1;
        }
        timerAnimator.SetBool(animationStatus, false);
        photonView.RPC("AnimationOK", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void AnimationOK()
    {
        animationOK++;
    }
    public void ToDealResultStep()
    {
        // only master can into this function
        // show card value
        // deal with skill
        // compare final result
        // record score
        // turn end
        print("into to deal result");
        photonView.RPC("ShowSelectedCard", RpcTarget.All);
        StartCoroutine(DealWithSkills());
    }
    IEnumerator DealWithSkills()
    {
        // only master into this function
        // check skill count
        // check skill useful nums, determine which skill will be execute
        // check skill onwer type
        // if skill owner not for all show character
        // excute a skill animation
        // deal with skill animation, if had other animation, show other
        // deal with excute skill

        int trueSkill = 0;
        List<int> skillType = new List<int>();
        
        foreach(GameObject skillFire in skillSettedOrder)
        {
            if(skillFire.name.Substring(0, skillFire.name.Length-1) != "")
            {
                trueSkill++;
            }
            skillType.Add(int.Parse(skillFire.name.Substring(skillFire.name.Length - 1, 1)));
        }
        string executeSkillName = "";
        if(trueSkill == 1 || trueSkill == 3)
        {
            foreach (GameObject skillFire in skillSettedOrder)
            {
                if (skillFire.name.Substring(0, skillFire.name.Length - 1) != "")
                {
                    executeSkillName = skillFire.name.Substring(0, skillFire.name.Length - 1);
                }
            }
        }
        foreach(GameObject skillFire in skillSettedOrder)
        {
            // unuseful skill
            if(skillFire.name == "-2")
            {
                skillFire.GetComponent<Animator>().SetTrigger("unuseful");
            }
            else
            {
                // control fire animation

                // change fire color and burn, show character, and play skill animation
                if (skillType.IndexOf(0) != -1)
                {
                    string[] sendValue;
                    string userID = skillFire.transform.GetComponentInParent<PlayerInfo>()
                                    .userId;
                    // change character and fire then burn the fire
                    
                    string professionCode = useUserIdGetPlayerGameObject[userID].
                                            GetComponent<PlayerInfo>().professionCode+"";
                    sendValue = new string[3];
                    sendValue[0] = userID;
                    sendValue[1] = professionCode;
                    sendValue[2] = skillFire.name;

                    
                    
                    photonView.RPC("ChangeCharacterSpriteAndFire", RpcTarget.All, sendValue);

                    // start to play animation
                    sendValue[1] = "New" + skillFire.name;

                    animationOK = 0;
                    photonView.RPC("StartSkillAnimationTrigger", RpcTarget.All, sendValue);
                    
                    yield return WaitForAnimationOK();
                }
                else
                {
                    // burn fire, and play skill animation
                    skillFire.GetComponent<Animator>().
                        SetTrigger("fireUnknow");
                }
            }
        }
        //photonView.RPC("PlaySkillAnimation", RpcTarget.All, );

    }
    [PunRPC]
    public void ChangeCharacterSpriteAndFire(string[] playerInfo)
    {
        string id = playerInfo[0];
        int professionCode = int.Parse(playerInfo[1]);
        string fireName = playerInfo[2];
        // change character
        useUserIdGetPlayerGameObject[id].transform.Find("Character").GetComponent<SpriteRenderer>().
            sprite = allCharacterSprites[professionCode];

        // change fire
        useUserIdGetPlayerGameObject[id].
            transform.Find(fireName).gameObject.SetActive(false);

        Transform skillFirePosition = useUserIdGetPlayerGameObject[id].
            transform.Find("SkillFirePosition");
        GameObject newSkillFire = Instantiate(allSkillsFire[professionCode], 
                                              skillFirePosition);
        newSkillFire.name = "New" + fireName;
    }
    [PunRPC]
    public void StartSkillAnimationTrigger(string[] skillInfo)
    {
        // id, object name, skill name

        string id = skillInfo[0];
        string objectName = skillInfo[1];
        string skillName = skillInfo[2];
        
        GameObject targetObject = useUserIdGetPlayerGameObject[id].transform.Find(objectName).gameObject;
        StartCoroutine(SkillAnimation(targetObject, skillName));

    }
    IEnumerator SkillAnimation(GameObject skillMark, string skillName)
    {
        while (skillMark.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("None"))
        {
            skillMark.GetComponent<Animator>().SetTrigger("burn");
            yield return 1;
        }
        while (!skillMark.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("arrive"))
        {
            // wait for change position and burning
            yield return 1;
        }
        // play video
        photonView.RPC("AnimationOK", RpcTarget.MasterClient);
    }
    IEnumerator WaitForAnimationOK()
    {
        while(animationOK < playerNums)
        {
            yield return 1;
        }
    }
    [PunRPC]
    public void ShowSelectedCard()
    {
        print("to show select card");
        foreach(PlayerInfo playerInfo in allPlayersInfo)
        {
            StartCoroutine(playerInfo.transform.GetComponentInChildren<SelectedCardInfo>().
                            ShowCard());
        }
    }
    IEnumerator JudgeSetSkillStatus()
    {
        skillSettedOrder = new List<GameObject>();
        while (eachStepsStatus[1])
        {
            if(skillSettedOrder.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                print("count same");
                eachStepsStatus[1] = false;
            }
            yield return 1;
        }
        photonView.RPC("SetSkillCanBeSettedStatus", RpcTarget.All, false);

        var waitTime = 1.5f;
        while(waitTime > 0)
        {
            waitTime -= Time.deltaTime * 1;
            yield return 1;
        }

        ToDealResultStep();
    }/*
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
    }*/
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
        
        while (remainingTime > 0 && eachStepsStatus[stepIndex])
        {
            var currentTimeValue = PhotonNetwork.Time;
            var passTime = currentTimeValue - startTimeValue;
            remainingTime = countDownTime - passTime;
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
        // deal with the player who didn't summon
        if(stepIndex == 0 && remainingTime <= 0)
        {
            photonView.RPC("AutoSummonRandomCard", RpcTarget.All);
        }
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
