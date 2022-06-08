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

    // 0 card selected consider, 1 skilled selected consider
    [SerializeField] List<bool> eachStepsStatus;

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
    List<string> pendingSkillData;
    [SerializeField] GameObject showOtherCardsView;
    GameObject pendingSkill = null;

    int resultMagnification = 1;
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

        foreach(VideoClip clip in allVideoClips)
        {
            useClipsNameGetClips.Add(clip.name, clip);
        }

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
        string[] sendValue = new string[4];
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

        // when get skill fire should know skill owner to judge show the character or not
        // and should know the skill name or code to execute the skill


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
        string[] sendValue = new string[3];
        sendValue[0] = PhotonNetwork.LocalPlayer.UserId;
        sendValue[1] = value+"";
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
        var waitTime = 0.5f;
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

        photonView.RPC("CheckPendingSKill", RpcTarget.All);
        print("into to deal result");
        photonView.RPC("ShowSelectedCard", RpcTarget.All);
        StartCoroutine(DealWithSkills());
    }
    [PunRPC]
    public void CheckPendingSKill()
    {
        if(pendingSkill != null)
        {
            pendingSkill.GetComponent<SkillInfo>().SetUse(false);
            pendingSkill.GetComponent<SkillInfo>().OnMouseUp();
            showOtherCardsView.SetActive(false);
            pendingSkill.SetActive(true);
            pendingSkill = null;
        }
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
        // skillsettedOrder's name = skillName + skill owner, if null skillName = "" owner = -2 -1 mean common not profession

        // if destroy object while at foreach should setting the object info into other list and deal with data only
        List<string> userIDs = new List<string>();
        List<string> skillNames = new List<string>();
        List<int> skillOwners = new List<int>();
        int skillFireNums = skillSettedOrder.Count;



        foreach(GameObject skillFire in skillSettedOrder)
        {
            var skillInfo = skillFire.name.Split('_');

            // setted the info into data type
            if (skillInfo.Count() > 2)
            {
                userIDs.Add(skillInfo[0]);
                skillNames.Add(skillInfo[1]);
                skillOwners.Add(int.Parse(skillInfo[2]));
            }
            else
            {
                userIDs.Add(skillInfo[0]);
                skillNames.Add(" ");
                skillOwners.Add(int.Parse(skillInfo[2]));
            }

            // if count < 2 mean had skill owner -2 and user id, the fake skill, other mean true skill
            if(skillInfo.Count() > 2)
            {
                // skillFire.name = id + "_" + name + "_" + owner;
                string userId = skillInfo[0];
                string skillName = skillInfo[1];
                int skillOwner = int.Parse(skillInfo[2]);

                // profession skill
                if(skillOwner != -1 && skillOwner != -2)
                {
                    // deal with first use profession skill and change fire and show character
                    if (!useUserIdGetPlayerGameObject[userId].GetComponent<PlayerInfo>().showCharacter)
                    {
                        string[] sendValue = new string[3];

                        sendValue[0] = userId;
                        sendValue[1] = skillOwner+"";
                        sendValue[2] = skillFire.name;

                        photonView.RPC("ChangeCharacterSpriteAndFire", RpcTarget.All, sendValue);

                    }
                }
            }
        }
        // deal with skills

        // userid, skillnums = animation type(move to center, split to two, queue),animation trigger code

        // each fire should burn then disappear or burnning
        List<string> skillAnimationList = new List<string>();

        for(int i = 0; i<userIDs.Count; i++)
        {
            skillAnimationList.Add(userIDs[i]);
        }
        
        skillAnimationList.Add(skillFireNums+"");

        // animation trigger, disappear or burning
        for(int i = 0; i<skillOwners.Count; i++)
        {
            string fireStatus = skillOwners[i]+"";

            skillAnimationList.Add(fireStatus);
        }
        for(int i = 0; i<skillNames.Count; i++)
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
        ToCompareStep();
    }

    
    void ToCompareStep()
    {
        // only mast client can into this
        // 
        print("compareSteps");
    }
    [PunRPC]
    public IEnumerator ExecuteSkillAnimation(string[] skillAnimationData)
    {

        string executeSkillUser = "";
        string executeSKillName = "";
        // if skillFireNum == 1, count = 1+1+1+1
        // if skillFireNum == 2, count = 2+1+2+2
        // if skillFireNum == 3, count = 3+1+3+3

        if (skillAnimationData.Count() == 4)
        {
            string userId = skillAnimationData[0];
            GameObject skillFire = useUserIdGetPlayerGameObject[userId].GetComponent<PlayerInfo>().
                                    GetSkillFire();
            
            string fireStatus = skillAnimationData[2] == "-2" ? "disappear" : "burning";

            int changeToSize = skillAnimationData[2] == "-2" ? 0 : 2;

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
                Animator fireAnimator = useUserIdGetPlayerGameObject[userId].GetComponent<PlayerInfo>().
                                        GetSkillFire().GetComponent<Animator>();

                string fireStatus = skillAnimationData[3 + i] == "-2" ? "disappear" : "burning";
                fireAnimator.SetTrigger(fireStatus);
                if(i == 1)
                {
                    while (fireAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime != 1)
                    {
                        yield return 1;
                    }
                }
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
                    photonView.RPC("AnimationOK", RpcTarget.MasterClient);
                }
            }
            else if (skillAnimationData[3] != "-2")
            {
                if (skillAnimationData[4] != "-2")
                {
                    string conterProfession1 = skillAnimationData[3];
                    string conterProfession2 = skillAnimationData[4];

                    StartCoroutine(PlaySkillAnimation(conterProfession1, 1, false));
                    StartCoroutine(PlaySkillAnimation(conterProfession2, 1, true));
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
                Animator fireAnimator = useUserIdGetPlayerGameObject[userId].GetComponent<PlayerInfo>().
                                        GetSkillFire().GetComponent<Animator>();
                string fireStatus = skillAnimationData[4+i] == "-2" ? "disappear" : "burning";
                fireAnimator.SetTrigger(fireStatus);
                while (fireAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime != 1)
                {
                    yield return 1;
                }
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
                        if (keepTime != 1)
                        {
                            yield return ChangeVideoTransparent();
                        }
                    }
                    else
                    {
                        yield return (PlaySkillAnimation(skillAnimationData[7 + i], 1, true));
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
                }
            }
            if(executeSkillUser != "")
            {
                if(PhotonNetwork.LocalPlayer.UserId == executeSkillUser)
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
                            for(int i = 0; i<allPlayersInfo.Count; i++)
                            {
                                allPlayersInfo[i].GetComponentInChildren<SelectedCardInfo>()
                                    .SetChangeValue(values[i]);
                            }
                            for(int i = 0; i<playerNums; i++)
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
                            photonView.RPC("ChangeResultMagnification",RpcTarget.MasterClient, 2);
                            break;
                        case "swap":
                            List<string> userIds = allPlayersInfo[0].GetSwapUserIds();
                            
                            for(int i = 0; i<2; i++)
                            {
                                int changeValue = useUserIdGetPlayerGameObject[userIds[i]]
                                           .GetComponentInChildren<SelectedCardInfo>()
                                           .GetValue();

                                useUserIdGetPlayerGameObject[userIds[1 - i]]
                                           .GetComponentInChildren<SelectedCardInfo>()
                                           .SetChangeValue(changeValue);
                            }
                            for(int i = 0; i<2; i++)
                            {
                                SkillAboutChangeValue(userIds[i]);
                            }
                            break;
                    }
                }
                
            }
        }
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
    void ExecuteSkill(string skillName)
    {
        
    }
    IEnumerator ChangeVideoTransparent()
    {
        Color color = showVideoObject.GetComponent<RawImage>().color;


        float time = 1;
        while (time > 0)
        {
            color.a -= (1) / 1 * Time.deltaTime;
            showVideoObject.GetComponent<RawImage>().color = color;
            time -= Time.deltaTime * 1;
            yield return 1;
        }
        color.a = 0;
        showVideoObject.GetComponent<RawImage>().color = color;
    }
    IEnumerator PlaySkillAnimation(string animationName, float playingTime, bool finalOK)
    {
        Color color = showVideoObject.GetComponent<RawImage>().color;
        color.a = 1;
        showVideoObject.GetComponent<RawImage>().color = color;
        videoPlayer.clip = null;
        videoPlayer.clip = useClipsNameGetClips[animationName];
        showVideoObject.SetActive(true);
        videoPlayer.Play();


        while (videoPlayer.frame < (long)videoPlayer.frameCount * playingTime - 1)
        {
            yield return 1;
        }
        
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
        //useUserIdExecuteSkill.Add()
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
        else if (showFromHand.Contains(checkName)) index = 1;
        return index;
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
