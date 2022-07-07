using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using HashTable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using System.Linq;
using Photon.Realtime;
using UnityEngine.Video;
using System;
using Random = UnityEngine.Random;

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
    [SerializeField] GameObject skillInfoView;
    [SerializeField] GameObject skillInfoWhenExecute;
    // 0 card selected consider, 1 skilled selected consider
    [SerializeField] List<bool> eachStepsStatus;

    [SerializeField] Text timerTitle;
    [SerializeField] List<Sprite> timerSprite;
    [SerializeField] Animator timerAnimator;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] GameObject showVideoObject;
    [SerializeField] List<VideoClip> allVideoClips;

    Dictionary<string, VideoClip> useClipsNameGetClips = new Dictionary<string, VideoClip>();
    int animationOK = 0;

    //Dictionary<string, string> useUserIdExecuteSkill = new Dictionary<string, List<string>>();
    [SerializeField] List<string> shouldShowOtherSkillName;
    [SerializeField] List<string> showFromHand;
    [SerializeField] List<string> showFromGrave;
    [SerializeField] List<string> showPlayers;
    [SerializeField] GameObject playerSelectedView;
    List<string> pendingSkillData;
    [SerializeField] GameObject showOtherCardsView;
    GameObject pendingSkill = null;

    int resultMagnification = 1;
    bool banSkill = false;
    string executeSkillUser = "";
    string executeSKillName = "";

    bool thisTurnSetCard = false;

    [SerializeField][Tooltip("0 en Name, 1 ch Name, 2 description")]
    List<TextAsset> fileList;
    // may can get this data from file or database
    Dictionary<string, string> useSkillNameGetShowSKillName = new Dictionary<string, string>();
    Dictionary<string, string> useSkillNameGetDescription = new Dictionary<string, string>();
    private void Awake()
    {
        if (!Screen.fullScreen)
        {
            float w = Screen.width;
            float h = 1080 * w / 1920;
            Screen.SetResolution((int)w, (int)h, false);
        }


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
        //
        var skillEnName = fileList[0].text.Split(',').ToArray();
        var skillChName = fileList[1].text.Split(',').ToArray();
        var descriptions = fileList[2].text.Split(',').ToArray();

        for(int i = 0; i < skillEnName.Count(); i++)
        {
            useSkillNameGetShowSKillName.Add(skillEnName[i], skillChName[i]);
            useSkillNameGetDescription.Add(skillEnName[i], descriptions[i]);
        }

        // set video clip
        foreach (VideoClip clip in allVideoClips)
        {
            useClipsNameGetClips.Add(clip.name, clip);
        }

        // get self data
        professionInfo = (HashTable)PhotonNetwork.LocalPlayer.CustomProperties["professionInfo"];
        characterCode = (int)professionInfo["professsion"];

        // wait for other
        prepareStep = true;
        if (PhotonNetwork.IsMasterClient)
        {
            // prepare time
            //startTimeValue = PhotonNetwork.Time;
            //StartCoroutine(SubTime());
            // set site info and wait all player loading OK
            Debug.Log("player count = " + PhotonNetwork.CurrentRoom.PlayerCount);
            foreach(KeyValuePair<int, Player> pair in PhotonNetwork.CurrentRoom.Players)
            {
                print(" key = " + pair.Key);
            }
            int key = 0;
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                while (!PhotonNetwork.CurrentRoom.Players.ContainsKey(key + 1))
                {
                    key++;
                }
                print(" get key = " + (key + 1));
                useUserIdGetPlayer.Add(PhotonNetwork.CurrentRoom.Players[key + 1].UserId,
                    PhotonNetwork.CurrentRoom.Players[key + 1]);
                Debug.Log(i + " OK");
                key++;
            }

            int siteCode = Random.Range(0, 1);
            photonView.RPC("SetSiteInfo", RpcTarget.All, siteCode);
            StartCoroutine(WaitAllPlayerLoading());
        }
        SetSelfPlayerData();
        SetAllPlayerInfo();
    }
    private void Update()
    {
        if (skillInfoView.activeSelf)
        {
            skillInfoView.transform.position = Input.mousePosition;
        }   
    }
    IEnumerator WaitAllPlayerLoading()
    {
        // wait all player loading OK.
        while (loadingOKPlayer < playerNums)
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
        photonView.RPC("SetCardCanBeSettedStatus", RpcTarget.All, true);
        timer.text = "";
        StartCoroutine(SubTime(cardSettedConsiderTime, 0));
        
    }
    void SetAllPlayerInfo()
    {
        int localIndex = -1;
        int key = 0;
        for (int i = 0; i < playerNums; i++)
        {
            while (!PhotonNetwork.CurrentRoom.Players.ContainsKey(key + 1))
            {
                key++;
            }
            allPlayersInfo[i].GetComponent<PlayerInfo>().userId =
                PhotonNetwork.CurrentRoom.Players[key + 1].UserId;
            if (PhotonNetwork.LocalPlayer.UserId == PhotonNetwork.CurrentRoom.Players[key + 1].UserId)
            {
                localIndex = i;
            }
            allPlayersInfo[i].GetComponent<PlayerInfo>().winGameCount = 0;
            allPlayersInfo[i].GetComponent<PlayerInfo>().winTurnCount = 0;
            allPlayersInfo[i].GetComponent<PlayerInfo>().score = 0;
            allPlayersInfo[i].GetComponent<PlayerInfo>().showCharacter = false;
            key++;
        }
        if (localIndex != 0)
        {
            string id = allPlayersInfo[0].GetComponent<PlayerInfo>().userId;
            allPlayersInfo[0].GetComponent<PlayerInfo>().userId =
                PhotonNetwork.LocalPlayer.UserId;
            allPlayersInfo[localIndex].GetComponent<PlayerInfo>().userId = id;
        }
        for (int i = 0; i < playerNums; i++)
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

        for (int i = 1; i <= 2; i++)
        {
            string skillName = (string)professionInfo[$"skill{i}"];
            string skillDescription = (string)professionInfo[$"skill{i}Description"];
            int skillOwner = (int)professionInfo[$"skillOwner{i}"];
            skills[i - 1].GetComponent<SpriteRenderer>().sprite = UseSkillNameGetSprite(skillName);
            skills[i - 1].GetComponent<SkillInfo>().skillName = skillName;
            skills[i - 1].GetComponent<SkillInfo>().skillOwner = skillOwner;
            skills[i - 1].GetComponent<SkillInfo>().skillDescription = skillDescription;
        }
        for (int i = 2; i < 5; i++)
        {
            skills[i].GetComponent<SkillInfo>().skillName = "";
            skills[i].GetComponent<SkillInfo>().skillDescription = "";
            skills[i].GetComponent<SkillInfo>().skillOwner = -2;
        }

        SetCards();
    }
    void SetCards()
    {
        var cards = decks.GetComponentsInChildren<Transform>()
            .Where(child => child.GetComponent<CardSetting>()).ToArray();

        int index = 0;
        foreach (Transform card in cards)
        {
            card.GetComponent<CardSetting>().SetFrame(allProfessionCardBackground[characterCode]);
            card.GetComponent<CardSetting>().SetValue(index + 1, allCardValueSprite[index]);
            index++;
        }

    }
    public void SetSelectedSkill(string skillName, int skillOwner)
    {
        // 0 user id, 1 card name(which use for deal with skill), 2 fireMark, 3 skillOwner
        string[] sendValue = new string[4];
        sendValue[0] = PhotonNetwork.LocalPlayer.UserId;
        sendValue[1] = skillName;
        sendValue[2] = allPlayersInfo[0].showCharacter == true ? skillOwner + "" : 6 + "";
        sendValue[3] = skillOwner + "";
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

        // when get skill fire should know skill owner to judge show the character or not
        // and should know the skill name or code to execute the skill

        if (useUserIdGetPlayerGameObject[id].GetComponent<PlayerInfo>().showCharacter)
        {
            fireMark = characterCode;
        }

        GameObject skillFire = Instantiate(allSkillsFire[fireMark], skillFirePosition);
        skillFire.transform.localPosition = Vector3.zero;
        skillFire.name = id + "_" + name + "_" + owner;

        useUserIdGetPlayerGameObject[id].GetComponent<PlayerInfo>().SetSkillFire(skillFire);

        if (PhotonNetwork.IsMasterClient)
        {
            skillSettedOrder.Add(skillFire);
            //useUserIdExecuteSkill.Add(id, name);
        }

    }
    public void SetSelectedCard(int value)
    {
        thisTurnSetCard = true;
        string[] sendValue = new string[3];
        sendValue[0] = PhotonNetwork.LocalPlayer.UserId;
        sendValue[1] = value + "";
        sendValue[2] = characterCode + "";
        allPlayersInfo[0].UseCard(value);
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
            .SetCard(frameSprite, allCardValueSprite[value - 1]);

        // this may had a animation about set card animation

        settedCards++;
        if (PhotonNetwork.IsMasterClient)
        {
            if (settedCards == playerNums)
            {
                eachStepsStatus[0] = false;
                settedCards = 0;
                if (!banSkill)
                {
                    StartCoroutine(ToSetSkillStep());
                }
                else
                {
                    photonView.RPC("SetBanSkill", RpcTarget.All, false);
                    skillSettedOrder = new List<GameObject>();
                    StartCoroutine(ToDealResultStep());
                    
                }
                

                
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

        for (int i = 0; i < allSkills.Length; i++)
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
        var waitTime = 0.5f;
        while (waitTime > 0)
        {
            waitTime -= Time.deltaTime * 1;
            yield return 1;
        }

        photonView.RPC("StartTimerAnimation", RpcTarget.All, 0);
        while (animationOK < playerNums)
        {
            //wating for animation OK...
            yield return 1;
        }

        animationOK = 0;

        eachStepsStatus[1] = true;
        StartCoroutine(JudgeSetSkillStatus());
        photonView.RPC("SetSkillCanBeSettedStatus", RpcTarget.All, true);
        photonView.RPC("SetCardCanBeSettedStatus", RpcTarget.All, false);

        eachStepsStatus[1] = true;

        timer.text = "";
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
        //string animationStatus = code == 0 ? "setSkill" : "summon";
        print("in timer animation");
        while (timerAnimator.GetCurrentAnimatorStateInfo(0).IsName("None"))
        {
            timerAnimator.SetBool("turn", true);
            yield return 1;
        }
        timerAnimator.transform.Find("Summon").GetComponent<Image>().sprite = timerSprite[code];
        timerAnimator.SetBool("turn", false);
        timer.text = "";
        while (!timerAnimator.GetCurrentAnimatorStateInfo(0).IsName("None"))
        {
            yield return 1;
        }
       
        timerTitle.text = code == 0 ? "Skill" : "Summon";
        photonView.RPC("AnimationOK", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void AnimationOK()
    {
        animationOK++;
    }
    public IEnumerator ToDealResultStep()
    {
        // only master can into this function
        // show card value
        // deal with skill
        // compare final result
        // record score
        // turn end

        photonView.RPC("CheckPendingSKill", RpcTarget.All);
        photonView.RPC("SetSkillCanBeSettedStatus", RpcTarget.All, false);
        animationOK = 0;
        photonView.RPC("ShowSelectedCard", RpcTarget.All);
        yield return WaitForAnimationOK();
        StartCoroutine(DealWithSkills());
    }
    public void SetAnimationOKToMaster()
    {
        photonView.RPC("AnimationOK", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void CheckPendingSKill()
    {
        if (pendingSkill != null)
        {
            CancelSkill();
        }
    }
    public void CancelSkill()
    {
        pendingSkill.GetComponent<SkillInfo>().SetUse(false);
        pendingSkill.GetComponent<SkillInfo>().OnMouseUp();
        showOtherCardsView.SetActive(false);
        pendingSkill.SetActive(true);
        pendingSkill = null;
    }
    IEnumerator DealWithSkills()
    {
        print("deal with skill");
        // only master into this function
        // check skill count
        // check skill useful nums, determine which skill will be execute
        // check skill onwer type
        // if skill owner not for all show character
        // excute a skill animation
        // deal with skill animation, if had other animation, show other
        // deal with excute skill
        // skillsettedOrder's name = skillName + skill owner, if null skillName = "" owner = -2 -1 mean common not profession

        // if destroy object while at foreach should setting the object info into other list and deal with data only
        List<string> userIDs = new List<string>();
        List<string> skillNames = new List<string>();
        List<int> skillOwners = new List<int>();
        int skillFireNums = skillSettedOrder.Count;

        print("skill settedorder count = " + skillSettedOrder.Count());

        foreach (GameObject skillFire in skillSettedOrder)
        {
            var skillInfo = skillFire.name.Split('_');
            // setted the info into data type
            userIDs.Add(skillInfo[0]);
            skillNames.Add(skillInfo[1]);
            skillOwners.Add(int.Parse(skillInfo[2]));

            // skillFire.name = id + "_" + name + "_" + owner;
            string userId = skillInfo[0];
            string skillName = skillInfo[1];
            int skillOwner = int.Parse(skillInfo[2]);

            //print("skillOwner = " + skillOwner);

            // profession skill
            if (skillOwner != -1 && skillOwner != -2)
            {
                // deal with first use profession skill and change fire and show character
                if (!useUserIdGetPlayerGameObject[userId].GetComponent<PlayerInfo>().showCharacter)
                {
                    string[] sendValue = new string[3];

                    sendValue[0] = userId;
                    sendValue[1] = skillOwner + "";
                    sendValue[2] = skillFire.name;

                    photonView.RPC("ChangeCharacterSpriteAndFire", RpcTarget.All, sendValue);

                }
            }

        }
        // deal with skills

        // userid, skillnums = animation type(move to center, split to two, queue),animation trigger code

        // each fire should burn then disappear or burnning
        List<string> skillAnimationList = new List<string>();

        for (int i = 0; i < userIDs.Count; i++)
        {
            skillAnimationList.Add(userIDs[i]);
        }

        skillAnimationList.Add(skillFireNums + "");

        // animation trigger, disappear or burning
        for (int i = 0; i < skillOwners.Count; i++)
        {
            string fireStatus = skillOwners[i] + "";

            skillAnimationList.Add(fireStatus);
        }
        for (int i = 0; i < skillNames.Count; i++)
        {
            skillAnimationList.Add(skillNames[i]);
        }
        var skillFireAnimationSendValue = skillAnimationList.ToArray();
        if (skillFireNums > 0)
        {
            photonView.RPC("ExecuteSkillAnimation", RpcTarget.All, skillFireAnimationSendValue);

            animationOK = 0;
            yield return WaitForAnimationOK();
        }



        if (executeSKillName != "")
        {
            photonView.RPC("SetExeCuteSkillInfo",RpcTarget.All, executeSKillName);

            animationOK = 0;
            photonView.RPC("ExecuteSkill", RpcTarget.All);
            yield return WaitForAnimationOK();
        }

        // look at final result 3sec
        float checkFinalStepTimes = 3f;
        while(checkFinalStepTimes > 0)
        {
            checkFinalStepTimes -= Time.deltaTime * 1;
            yield return 1;
        }

        yield return ToCompareStep();
    }
    [PunRPC]
    public void SetExeCuteSkillInfo(string skillName)
    {
        print("set execute skill info");
        string showName = useSkillNameGetShowSKillName[skillName];
        string description = useSkillNameGetDescription[skillName];

        print("show name = " + showName);
        print("description = " + description);

        skillInfoWhenExecute.transform.Find("SkillImage").GetComponent<Image>().sprite = UseSkillNameGetSprite(skillName);
        skillInfoView.transform.Find("Image").Find("Name").GetComponent<Text>().text = showName;
        skillInfoView.transform.Find("Image").Find("Description").GetComponent<Text>().text = description;
        skillInfoWhenExecute.SetActive(true);
    }
    [PunRPC]
    public IEnumerator ExecuteSkill()
    {
        // excute the skill
        if (executeSkillUser != "")
        {
            // use owner use skill and caculate the result finally send result to all
            // but if the skill change the game status just send to master client
            if (PhotonNetwork.LocalPlayer.UserId == executeSkillUser)
            {
                switch (executeSKillName)
                {
                    case "replace":
                        SkillAboutChangeValue(executeSkillUser);
                        break;
                    case "random":
                        int[] order = { 0, 1, 2 };
                        Shuffle(order);
                        List<int> values = GetNowShowValues();
                        for (int i = 0; i < allPlayersInfo.Count; i++)
                        {
                            allPlayersInfo[i].GetComponentInChildren<SelectedCardInfo>()
                                .SetChangeValue(values[i]);
                        }
                        for (int i = 0; i < playerNums; i++)
                        {
                            SkillAboutChangeValue(allPlayersInfo[i].userId);
                        }
                        break;
                    case "qualitycontrol":
                        break;
                    case "cave":
                        SkillAboutChangeValue(executeSkillUser);
                        break;
                    case "swords":
                        photonView.RPC("ChangeResultMagnification", RpcTarget.MasterClient, 2);
                        break;
                    case "swap":
                        List<string> userIds = allPlayersInfo[0].GetSwapUserIds();

                        for (int i = 0; i < 2; i++)
                        {
                            int changeValue = useUserIdGetPlayerGameObject[userIds[i]]
                                       .GetComponentInChildren<SelectedCardInfo>()
                                       .GetValue();

                            useUserIdGetPlayerGameObject[userIds[1 - i]]
                                       .GetComponentInChildren<SelectedCardInfo>()
                                       .SetChangeValue(changeValue);
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            SkillAboutChangeValue(userIds[i]);
                        }
                        break;
                    case "banned":
                        photonView.RPC("SetBanSkill", RpcTarget.MasterClient, true);
                        break;
                    case "damaged":
                        photonView.RPC("ChangeResultMagnification", RpcTarget.MasterClient, 0);
                        break;
                    case "invalid":
                        break;
                }
            }
        }
        float waitTime = 1.5f;
        while (waitTime > 0)
        {
            waitTime -= Time.deltaTime * 1;
            yield return 1;
        }
        photonView.RPC("AnimationOK", RpcTarget.MasterClient);
    }

    IEnumerator ToCompareStep()
    {
        print("to compare step");
        // only mast client can into this
        // 
        int max = -1;
        int min = 11;
        int mid = -1;
        int addValue = 0;
        string maxId = "";
        string minId = "";
        bool twoMax = false;
        bool threeSame = false;
        string winId = "";
        List<int> values = new List<int>();
        for(int i = 0; i<playerNums; i++)
        {
            int value = allPlayersInfo[i].transform.GetComponentInChildren<SelectedCardInfo>().value;
            values.Add(value);
        }

        for (int i = 0; i < values.Count; i++)
        {
            int value = values[i];
            if (value > max)
            {
                if (max != -1)
                {
                    if (min == 11)
                    {
                        min = max;
                        minId = maxId;
                    }
                    else
                    {
                        mid = max;
                    }
                }
                max = value;
                maxId = allPlayersInfo[i].userId;
            }
            else if (value == max)
            {
                if (min == 11)
                {
                    min = max;
                    minId = maxId;
                }
                else
                {
                    mid = max;
                }
                max = value;
                maxId = allPlayersInfo[i].userId;
            }
            else if (value < max)
            {
                if (min == 11)
                {
                    min = value;
                    minId = allPlayersInfo[i].userId;
                }
                else
                {
                    if (value < min)
                    {
                        mid = min;
                        min = value;
                        minId = allPlayersInfo[i].userId;
                    }
                    else
                    {
                        mid = value;
                    }
                }
            }
        }
        if (max == min)
        {
            threeSame = true;
        }
        else if (max == mid)
        {
            twoMax = true;
        }
        if (threeSame)
        {
            print("three same");
            string[] sendValue;
            animationOK = 0;
            for (int i = 0; i < playerNums; i++)
            {
                sendValue = new string[2];
                sendValue[0] = allPlayersInfo[i].userId;
                sendValue[1] = i == playerNums - 1 ? "last" : "normal";
                photonView.RPC("ChangeCardTransparent", RpcTarget.All, sendValue);
            }
            yield return WaitForAnimationOK();
        }
        else if (twoMax)
        {
            print("two max");
            string[] sendValue;
            animationOK = 0;
            int dealNums = 0;
            for (int i = 0; i < playerNums; i++)
            {
                if (allPlayersInfo[i].transform.GetComponentInChildren<SelectedCardInfo>().value == max)
                {
                    dealNums++;
                    sendValue = new string[2];
                    sendValue[0] = allPlayersInfo[i].userId;
                    sendValue[1] = dealNums == playerNums-1 ? "last" : "normal";
                    photonView.RPC("ChangeCardTransparent", RpcTarget.All, sendValue);
                    if (dealNums == 2) break;
                }
            }
            winId = minId;
            addValue = (max - min) * 2;
            yield return WaitForAnimationOK();
        }
        else
        {
            print("three different or two small");
            string[] sendValue;
            animationOK = 0;
            int dealNums = 0;
            for (int i = 0; i < playerNums; i++)
            {
                if (allPlayersInfo[i].transform.GetComponentInChildren<SelectedCardInfo>().value != max)
                {
                    addValue += max - allPlayersInfo[i].transform.GetComponentInChildren<SelectedCardInfo>().value;
                    dealNums++;
                    sendValue = new string[2];
                    sendValue[0] = allPlayersInfo[i].userId;
                    sendValue[1] = dealNums == playerNums - 1 ? "last" : "normal";
                    photonView.RPC("ChangeCardTransparent", RpcTarget.All, sendValue);
                }
            }
            if(testMode && playerNums == 1)
            {
                animationOK++;
            }
            winId = maxId;
            yield return WaitForAnimationOK();
        }

        yield return ToFinialStep(winId, addValue * resultMagnification);
    }
    IEnumerator ToFinialStep(string winId, int gainScore)
    {
        print("to final step");
        // only master client can into this 

        // record score
        // close card
        // close skillshow info view
        // initial data
        // call turn start

        string[] sendValue = new string[2];
        sendValue[0] = winId;
        sendValue[1] = gainScore + "";
        if (winId != "")
        {
            photonView.RPC("AddScore", RpcTarget.All, sendValue);
        }
        photonView.RPC("CloseCard", RpcTarget.All);
        photonView.RPC("InitialData", RpcTarget.All);
        // change timer situation
        animationOK = 0;
        photonView.RPC("StartTimerAnimation", RpcTarget.All, 1);
        yield return WaitForAnimationOK();
        TurnStart();
    }
    [PunRPC]
    public void InitialData()
    {
        executeSKillName = "";
        executeSkillUser = "";
        thisTurnSetCard = false;
        resultMagnification = 1;
        skillInfoWhenExecute.SetActive(false);
        skillInfoView.SetActive(false);

        DeletedAllSkillFire();
        SetCardCanBeSettedStatus(true);
    }
    void DeletedAllSkillFire()
    {
        for(int i = 0; i<allPlayersInfo.Count(); i++)
        {
            Transform fireParent = allPlayersInfo[i].transform.Find("SkillFirePosition");
            foreach(Transform child in fireParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
    [PunRPC]
    public void CloseCard()
    {
        print(" to close card");
        allPlayersInfo[0].SendToGrave();

        for(int i = 0; i < allPlayersInfo.Count(); i++)
        {
            Transform selectedCardRegion = allPlayersInfo[i].transform.GetComponentInChildren<SelectedCardInfo>().transform;
            selectedCardRegion.GetComponent<SpriteRenderer>().sprite = null;
            foreach(Transform child in selectedCardRegion)
            {
                child.gameObject.SetActive(false);
                Color color = child.GetComponent<SpriteRenderer>().color;
                color.a = 1;
                child.GetComponent<SpriteRenderer>().color = color;
            }
        }
    }
    [PunRPC]
    public void AddScore(string[] sendValue)
    {
        string id = sendValue[0];
        int score = int.Parse(sendValue[1]);
        useUserIdGetPlayerGameObject[id].GetComponent<PlayerInfo>().score += score;
    }
    [PunRPC]
    public IEnumerator ChangeCardTransparent(string[] sendValue)
    {
        print("change card transparent");
        string userId = sendValue[0];
        List<Transform> targets = useUserIdGetPlayerGameObject[userId].transform.GetComponentInChildren<SelectedCardInfo>().transform
                                    .GetComponentsInChildren<Transform>().ToList();
        bool lastOne = sendValue[1] == "last" ? true : false;
        Color color = targets[1].GetComponent<SpriteRenderer>().color;
        color.a = 1;

        var changeTime = 3f;
        while(changeTime > 0)
        {
            color.a -= 0.7f / 3 * Time.deltaTime;
            changeTime -= Time.deltaTime * 1;
            for (int i = 1; i < targets.Count; i++)
            {
                targets[i].GetComponent<SpriteRenderer>().color = color;
            }
            yield return 1;
        }
        if (lastOne)
        {
            photonView.RPC("AnimationOK", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    public IEnumerator ExecuteSkillAnimation(string[] skillAnimationData)
    {
        // all player into this

        executeSkillUser = "";
        executeSKillName = "";
        // if skillFireNum == 1, count = 1+1+1+1
        // if skillFireNum == 2, count = 2+1+2+2
        // if skillFireNum == 3, count = 3+1+3+3
        print("count = " + skillAnimationData.Count());
        if (skillAnimationData.Count() == 4)
        {
            string userId = skillAnimationData[0];
            GameObject skillFire = useUserIdGetPlayerGameObject[userId].GetComponent<PlayerInfo>().
                                    GetSkillFire();
            
            string fireStatus = skillAnimationData[2] == "-2" ? "disappear" : "burning";

            yield return ChangeFireSize(fireStatus, skillFire);

            

            if (fireStatus == "burning")
            {
                StartCoroutine(PlaySkillAnimation(skillAnimationData[3], 1, true));
                executeSkillUser = skillAnimationData[0];
                executeSKillName = skillAnimationData[3];
            }
            else
            {
                skillFire.SetActive(false);
                photonView.RPC("AnimationOK", RpcTarget.MasterClient);
            }

        }
        // two fire burn together then if true true show count animation at the same time
        else if(skillAnimationData.Count() == 7)
        {
            for(int i = 0; i<2; i++)
            {
                string userId = skillAnimationData[i];

                GameObject skillFire = useUserIdGetPlayerGameObject[userId].GetComponent<PlayerInfo>().
                                    GetSkillFire();

                string fireStatus = skillAnimationData[3 + i] == "-2" ? "disappear" : "burning";

                yield return ChangeFireSize(fireStatus, skillFire);

            }
            if (skillAnimationData[3] == "-2")
            {
                if (skillAnimationData[4] != "-2")
                {
                    StartCoroutine(PlaySkillAnimation(skillAnimationData[6], 1, true));
                    executeSkillUser = skillAnimationData[1];
                    executeSKillName = skillAnimationData[6];
                }
                else if (skillAnimationData[4] == "-2")
                {
                    print("two true fire ok");
                    executeSkillUser = "";
                    executeSKillName = "";
                    photonView.RPC("AnimationOK", RpcTarget.MasterClient);
                }
            }
            else if (skillAnimationData[3] != "-2")
            {
                if (skillAnimationData[4] != "-2")
                {
                    string conterProfession1 = skillAnimationData[3];
                    string conterProfession2 = skillAnimationData[4];

                    executeSkillUser = "";
                    executeSKillName = "";
                    photonView.RPC("AnimationOK", RpcTarget.MasterClient);
                    // maybe should use conter animation or show a powerful block and say a sentence
                    //StartCoroutine(PlaySkillAnimation(conterProfession1, 1, false));
                    //StartCoroutine(PlaySkillAnimation(conterProfession2, 1, true));
                }
                else if (skillAnimationData[4] == "-2")
                {
                    StartCoroutine(PlaySkillAnimation(skillAnimationData[5], 1, true));
                    executeSkillUser = skillAnimationData[0];
                    executeSKillName = skillAnimationData[5];
                }
            }
        }
        else if(skillAnimationData.Count() == 10)
        {
            float trueIndex = -1;
            bool allFake = true;
            int trueNums = 0;
            for(int i = 0; i<3; i++)
            {
                string userId = skillAnimationData[i];
                GameObject skillFire = useUserIdGetPlayerGameObject[userId].GetComponent<PlayerInfo>().
                                    GetSkillFire();
                
                string fireStatus = skillAnimationData[4+i] == "-2" ? "disappear" : "burning";

                yield return ChangeFireSize(fireStatus, skillFire);

                if (fireStatus != "disappear")
                {
                    // if trueIndex == 0, 0.33,
                    // if trueIndex == 1, 0.5
                    // if trueIndex == 2, -1
                    trueNums++;
                    allFake = false;
                    if (trueIndex == -1) trueIndex = i;
                    if (i < 2)
                    {
                        float keepTime = trueIndex == 0 ? 0.33f : trueIndex == 1 ? 0.5f : 1;
                        trueIndex = -1;
                        yield return (PlaySkillAnimation(skillAnimationData[7 + i], keepTime, false));
                        executeSkillUser = skillAnimationData[0 + i];
                        executeSKillName = skillAnimationData[7 + i];
                        yield return ChangeVideoTransparent();
                    }
                    else
                    {
                        yield return (PlaySkillAnimation(skillAnimationData[7 + i], 1, true));
                    }
                }
                else
                {
                    skillFire.SetActive(false);
                    if (trueNums > 0)
                    {
                        Color color = showVideoObject.GetComponent<RawImage>().color;
                        color.a = 1;
                        showVideoObject.GetComponent<RawImage>().color = color;
                        videoPlayer.Play();
                        if (i == 1)
                        {
                            while (videoPlayer.frame < (long)videoPlayer.frameCount * 0.66f - 1)
                            {
                                yield return 1;
                            }
                            videoPlayer.Pause();
                            yield return ChangeVideoTransparent();
                        }
                        else if(i == 2)
                        {
                            if(trueNums != 2)
                            {
                                while (videoPlayer.frame < (long)videoPlayer.frameCount - 1)
                                {
                                    yield return 1;
                                }
                            }
                            photonView.RPC("AnimationOK", RpcTarget.MasterClient);
                            showVideoObject.SetActive(false);
                        } 
                    }
                }
            }
            if (allFake)
            {
                photonView.RPC("AnimationOK", RpcTarget.MasterClient);
            }
            else
            {
                if(trueNums == 2)
                {
                    executeSkillUser = "";
                    executeSKillName = "";
                    photonView.RPC("AnimationOK", RpcTarget.MasterClient);
                }
            }


            
        }
    }

    IEnumerator ChangeFireSize(string fireStatus, GameObject skillFire)
    {
        int changeToSize = fireStatus == "disappear" ? 0 : 2;

        float time = 2;
        float baseScale = skillFire.transform.localScale.x;
        float baseTime = time;
        int plusOrSub = changeToSize == 0 ? -1 : 1;

        while (time > 0)
        {
            Vector3 scale = skillFire.transform.localScale;
            scale.x += plusOrSub * baseScale / baseTime * Time.deltaTime;
            scale.y += plusOrSub * baseScale / baseTime * Time.deltaTime;
            scale.z += plusOrSub * baseScale / baseTime * Time.deltaTime;
            skillFire.transform.localScale = scale;
            time -= Time.deltaTime * 1;
            yield return 1;
        }
    }
    [PunRPC]
    public void SetBanSkill(bool status)
    {
        banSkill = status;
    }
    [PunRPC]
    public void ChangeResultMagnification(int magnification)
    {
        resultMagnification = magnification;
    }
    public List<int> GetNowShowValues()
    {
        List<int> values = new List<int>();
        for(int i = 0; i<allPlayersInfo.Count; i++)
        {
            values.Add(allPlayersInfo[i].transform.GetComponentInChildren<SelectedCardInfo>().value);
        }
        return values;
    }
    public void SkillAboutChangeValue(string targetUserId)
    {
        int changeValue = useUserIdGetPlayerGameObject[targetUserId].
                                GetComponentInChildren<SelectedCardInfo>().GetChangeValue();
        string[] sendValue = new string[2];
        sendValue[0] = targetUserId;
        sendValue[1] = changeValue + "";
        photonView.RPC("ChangeShowCardValue", RpcTarget.All, sendValue);
    }

    public void Shuffle(int[] order)
    {
        for(int i = 0; i<order.Length; i++)
        {
            int changeIndex = Random.Range(0, order.Length);
            int currentValue = order[i];
            order[i] = order[changeIndex];
            order[changeIndex] = currentValue;
        }
    }
    [PunRPC]
    public void ChangeShowCardValue(string[] newValueData)
    {
        string id = newValueData[0];
        int value = int.Parse(newValueData[1]);

        useUserIdGetPlayerGameObject[id].GetComponentInChildren<SelectedCardInfo>()
            .ChangeShowValue(value, allCardValueSprite[value - 1]);
    }

    IEnumerator ChangeVideoTransparent()
    {
        Color color = showVideoObject.GetComponent<RawImage>().color;


        float time = 1;
        while (time > 0)
        {
            color.a -= (0.7f) / 1 * Time.deltaTime;
            showVideoObject.GetComponent<RawImage>().color = color;
            time -= Time.deltaTime * 1;
            yield return 1;
        }
        showVideoObject.GetComponent<RawImage>().color = color;
    }
    IEnumerator PlaySkillAnimation(string animationName, float playingTime, bool finalOK)
    {
        Color color = showVideoObject.GetComponent<RawImage>().color;
        color.a = 1;
        showVideoObject.GetComponent<RawImage>().color = color;
        videoPlayer.clip = null;

        // if all skill had animation this if can remove
        if (useClipsNameGetClips.ContainsKey(animationName))
        {
            videoPlayer.clip = useClipsNameGetClips[animationName];
            showVideoObject.SetActive(true);
            videoPlayer.Play();


            while (videoPlayer.frame < (long)videoPlayer.frameCount * playingTime - 1)
            {
                yield return 1;
            }
        }

        
        videoPlayer.Pause();
        if (finalOK)
        {
            photonView.RPC("AnimationOK", RpcTarget.MasterClient);
            showVideoObject.SetActive(false);
        }
        
    }
    [PunRPC]
    public void ChangeCharacterSpriteAndFire(string[] playerInfo)
    {
        // sendValue[0] = userId; sendValue[1] = skillOwner; sendValue[2] = skillFire.name;

        string id = playerInfo[0];
        int professionCode = int.Parse(playerInfo[1]);
        string fireName = playerInfo[2];
        // change character
        useUserIdGetPlayerGameObject[id].transform.Find("Character").GetComponent<SpriteRenderer>().
            sprite = allCharacterSprites[professionCode];

        useUserIdGetPlayerGameObject[id].GetComponent<PlayerInfo>().showCharacter = true;

        // change fire
        Destroy(useUserIdGetPlayerGameObject[id].GetComponent<PlayerInfo>().GetSkillFire());

        Transform skillFirePosition = useUserIdGetPlayerGameObject[id].
            transform.Find("SkillFirePosition");
        GameObject newSkillFire = Instantiate(allSkillsFire[professionCode], 
                                              skillFirePosition);
        newSkillFire.transform.localPosition = Vector3.zero;
        newSkillFire.name = fireName;

        useUserIdGetPlayerGameObject[id].GetComponent<PlayerInfo>().SetSkillFire(newSkillFire);

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
        print("show cards");
        int index = 0;
        bool last = false;
        foreach(PlayerInfo playerInfo in allPlayersInfo)
        {
            if (index == 2) last = true;
            StartCoroutine(playerInfo.transform.GetComponentInChildren<SelectedCardInfo>().
                            ShowCard(last));
            index++;
        }
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
        photonView.RPC("SetSkillCanBeSettedStatus", RpcTarget.All, false);

        var waitTime = 1.5f;
        while(waitTime > 0)
        {
            waitTime -= Time.deltaTime * 1;
            yield return 1;
        }

        StartCoroutine(ToDealResultStep());
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
        if (!currentTime.ContainsKey("remainingTime"))
        {
            currentTime.Add("remainingTime", remainingTime);
        }
        else
        {
            currentTime["remainingTime"] = remainingTime;
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(currentTime);
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
    [PunRPC]
    public void AutoSummonRandomCard()
    {
        if (!thisTurnSetCard)
        {
            HashSet<int> cardsInHand = allPlayersInfo[0].GetEverywhereCards()[0];
            var cardArray = cardsInHand.ToArray();
            int value = cardArray[Random.Range(0, cardArray.Count())];
            SetSelectedCard(value);
        }
    }
    public bool CheckShowOther(string checkName)
    {
        return shouldShowOtherSkillName.Contains(checkName);
    }
    /// <summary>
    /// if other card selected OK, will get data from this to set
    /// </summary>
    /// <param name="skillName"></param>
    /// <param name="skillOwner"></param>
    public void SetPendingSkill(string skillName, string skillOwner, GameObject skill)
    {
        pendingSkill = skill;
        pendingSkillData = new List<string>();
        pendingSkillData.Add(skillName);
        pendingSkillData.Add(skillOwner);
        
    }
    public void SetSelectedOtherCard(int value)
    {
        SetSelectedSkill(pendingSkillData[0], int.Parse(pendingSkillData[1]));
        allPlayersInfo[0].transform.GetComponentInChildren<SelectedCardInfo>().SetChangeValue(value);
        showOtherCardsView.SetActive(false);
        pendingSkill = null;
    }
    /// <summary>
    /// if hide index == 0, hide select player
    /// </summary>
    /// <param name="hideViewIndex"></param>
    public void SetOtherActionOKToSetSkillCard(int hideViewIndex)
    {
        SetSelectedSkill(pendingSkillData[0], int.Parse(pendingSkillData[1]));
        if(hideViewIndex == 0)
        {
            playerSelectedView.SetActive(false);
        }
        pendingSkill = null;
    }
    public void ShowPlayersToSelect()
    {
        playerSelectedView.SetActive(true);
        foreach(Transform child in playerSelectedView.transform)
        {
            child.gameObject.SetActive(true);
        }
        playerSelectedView.transform.Find("Self").GetComponent<Image>().sprite =
            GameObject.Find("SelfPlayer").transform.Find("Character").GetComponent<SpriteRenderer>().sprite;
        playerSelectedView.transform.Find("Left").GetComponent<Image>().sprite =
            GameObject.Find("LeftPlayer").transform.Find("Character").GetComponent<SpriteRenderer>().sprite;
        playerSelectedView.transform.Find("Right").GetComponent<Image>().sprite =
            GameObject.Find("RightPlayer").transform.Find("Character").GetComponent<SpriteRenderer>().sprite;
    }

    /// <summary>
    /// index = 0 mean from hand, index = 1, mean from grave
    /// </summary>
    /// <param name="index"></param>
    public void ShowOtherToSelect(int index)
    {
        showOtherCardsView.SetActive(true);
        Transform layer1 = showOtherCardsView.transform.Find("ShowCardView").Find("layer1");
        Transform layer2 = showOtherCardsView.transform.Find("ShowCardView").Find("layer2");

        List<HashSet<int>> everyWhereCards = allPlayersInfo[0].GetEverywhereCards();

        int i = 1;
        foreach (Transform child in layer1)
        {
            if (!everyWhereCards[index].Contains(i))
            {
                child.gameObject.SetActive(false);
            }
            i++;
        }
        foreach(Transform child in layer2)
        {
            if (!everyWhereCards[index].Contains(i))
            {
                child.gameObject.SetActive(false);
            }
            i++;
        }
    }
    public int CheckShowFromWhere(string checkName)
    {
        int index = -1;
        if (showFromHand.Contains(checkName)) index = 0;
        else if (showFromGrave.Contains(checkName)) index = 1;
        else if (showPlayers.Contains(checkName)) index = 2;
        return index;
    }
    public void SetSkillInfoView(bool status)
    {
        skillInfoView.SetActive(status);
    }
    /// <summary>
    /// index = 0, self, index = 1, left, index = 2, right
    /// </summary>
    /// <param name="index"></param>
    public void SetSwapUser(int index)
    {
        string id = "";
        switch (index)
        {
            case 0:
                id = allPlayersInfo[0].userId;
                break;
            case 1:
                id = GameObject.Find("LeftPlayer").GetComponent<PlayerInfo>().userId;
                break;
            case 2:
                id = GameObject.Find("RightPlayer").GetComponent<PlayerInfo>().userId;
                break;
        }
        allPlayersInfo[0].SetSwapUserId(id);
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
