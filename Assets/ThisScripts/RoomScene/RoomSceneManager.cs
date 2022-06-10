using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using HashTable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using System.Linq;

public class RoomSceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Text roomName;
    [SerializeField] Button buttonPrepare;
    [SerializeField] Button buttonStartGame;
    [SerializeField] GameObject playerStatusInRoom;
    [SerializeField] Transform playerList;
    [SerializeField] Color selfMarkColor;

    PlayerRoomStatus selfInfo;

    public bool testMode = false;

    [SerializeField] GameObject Characters;
    [SerializeField] GameObject CharacterSelected;
    [SerializeField] GameObject SkillSelector;
    //[SerializeField] GameObject Skills;
    [SerializeField] List<GameObject> allSkills; 
    [SerializeField] int skillIndex = -1;
    [SerializeField] List<Image> skillBlocks = new List<Image>();
    [SerializeField] GameObject skillInfoView;
    HashTable professionInfo;

    [SerializeField] GameObject tipsView;

    public static RoomSceneManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        professionInfo = new HashTable();
        PhotonNetwork.AutomaticallySyncScene = true;
        InitRoomInfo();
    }
    private void Update()
    {
        if (skillInfoView.activeSelf)
        {
            skillInfoView.transform.position = Input.mousePosition;
        }
    }
    public void InitRoomInfo()
    {
        //set room name
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        // ini player info in the room
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            var playerObject = Instantiate(playerStatusInRoom, playerList);
            
            // set self prepareData to false
            if (player.Value.UserId == PhotonNetwork.LocalPlayer.UserId)
            {
                selfInfo = playerObject.GetComponent<PlayerRoomStatus>();
                ((HashTable)player.Value.CustomProperties["prepare"])["prepare"] = false;
                //playerObject.GetComponentInChildren<Image>().color = selfMarkColor;
            }
            //set player data, nickname to show, user id to idendity, prepare status to let other know
            playerObject.GetComponent<PlayerRoomStatus>().
                SetStatus(player.Value.NickName, player.Value.UserId,
                          (bool)((HashTable)player.Value.CustomProperties["prepare"])["prepare"]);

        }
        // if master client, show start button other show prepare button
        if (testMode)
        {
            buttonPrepare.gameObject.SetActive(true);
        }
        else
        {
            buttonPrepare.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
            buttonStartGame.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }
        
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList(newPlayer, true);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName);
        UpdatePlayerList(otherPlayer, false);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            buttonPrepare.gameObject.SetActive(false);
            buttonStartGame.gameObject.SetActive(true);

            HashTable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            ((HashTable)customProperties["prepare"])["prepare"] = false;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        }
    }

    void UpdatePlayerList(Player targetPlayer, bool add)
    {
        if (add)
        {
            var playerObject = Instantiate(playerStatusInRoom, playerList);
            playerObject.GetComponent<PlayerRoomStatus>().
                SetStatus(targetPlayer.NickName, targetPlayer.UserId,
                           (bool)((HashTable)targetPlayer.CustomProperties["prepare"])["prepare"]);
        }
        else
        {
            foreach(Transform player in playerList)
            {
                if(player.GetComponent<PlayerRoomStatus>().GetUserId() == targetPlayer.UserId)
                {
                    DestroyImmediate(player.gameObject);
                    break;
                }
            }
        }
    }
    
    public void OnClickLeaveRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickPrepare(GameObject reselectButton)
    {
        
        if(skillBlocks[0].sprite == null || skillBlocks[1].sprite == null)
        {
            tipsView.transform.Find("Text").GetComponent<Text>().text = "Check role and skills";
            tipsView.SetActive(true);
            StartCoroutine(HideSomething(tipsView, 1.5f));
            return;
        }
        HideAllSkills();
        reselectButton.SetActive(!reselectButton.activeSelf);
        buttonPrepare.GetComponentInChildren<Text>().text = buttonPrepare.GetComponentInChildren<Text>().text
           == "Prepare" ? "Cancel" : "Prepare";

        HashTable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        if (customProperties.ContainsKey("professionInfo"))
        {
            customProperties["professionInfo"] = professionInfo;
        }
        else
        {
            customProperties.Add("professionInfo", professionInfo);
        }
        

        ((HashTable)customProperties["prepare"])["prepare"] =
            !(bool)((HashTable)customProperties["prepare"])["prepare"];
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        if (testMode)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                buttonPrepare.transform.gameObject.SetActive(false);
                buttonStartGame.transform.gameObject.SetActive(true);
            }
            
        }
        
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        print("update");
        foreach(Transform player in playerList)
        {
            if(player.GetComponent<PlayerRoomStatus>().GetUserId() == targetPlayer.UserId)
            {
                player.GetComponent<PlayerRoomStatus>().ChangePrepareStatus
                    ((bool)((HashTable)changedProps["prepare"])["prepare"]);
            }
        }
    }

    public void OnClickStartGame()
    {
        // Check player nums
        if (testMode)
        {
            SceneManager.LoadScene("GameScene");
            return;
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 3)
            {
                return;
            }
            // Check all prepare
            foreach (Transform player in playerList)
            {
                if (player.GetComponent<PlayerRoomStatus>().
                    GetUserId() != PhotonNetwork.MasterClient.UserId)
                {
                    //print(player.GetComponent<PlayerRoomStatus>().GetPrepareOrNot());
                    if (!player.GetComponent<PlayerRoomStatus>().GetPrepareOrNot())
                    {
                        return;
                    }
                }
            }
            SceneManager.LoadScene("GameScene");
        }

    }

    public void SetCharacter(int code, Sprite characterSprite)
    {
        professionInfo.Add("professsion", code);
        CharacterSelected.GetComponentInChildren<Image>().sprite = characterSprite;
        CharacterSelected.SetActive(true);
        Characters.SetActive(false);
        InitialSkillBlocks(code);
        SkillSelector.SetActive(true);
        skillIndex = 0;
    }
    void InitialSkillBlocks(int characterCode)
    {
        foreach(GameObject child in allSkills)
        {
            if(child.GetComponent<SkillSetting>().GetSkillOnwer() != characterCode && 
                child.GetComponent<SkillSetting>().GetSkillOnwer() != -1)
            {
                child.transform.gameObject.SetActive(false);
            }
            else
            {
                child.transform.gameObject.SetActive(true);
            }
        }
    }
    void HideAllSkills()
    {
        foreach (GameObject child in allSkills)
        {
            child.transform.gameObject.SetActive(false);
        }
    }
    public void OnClickBackToSelectCharacters()
    {
        professionInfo = new HashTable();
        Characters.SetActive(true);
        CharacterSelected.GetComponentInChildren<Image>().sprite = null;
        CharacterSelected.SetActive(false);
        SkillSelector.SetActive(false);
        SetSelectedSkill(0, -1, null, "", "", -2);
        SetSelectedSkill(1, -1, null, "", "", -2);
    }
    public void OnClickSkillIndex(int index )
    {
        skillIndex = index;
    }
    void SetTargetSKillCanBeSelected(string skillSpriteName)
    {
        foreach (GameObject child in allSkills)
        {
            if (child.transform.Find("Image").GetComponent<Image>().sprite.name == skillSpriteName)
            {
                child.SetActive(true);
            }
        }
    }
    public void SetSelectedSkill(int index, int skillCode, Sprite skillSprite, string skillName,
                                 string descritpion, int skillOwner)
    {
        if (index == -1) return;
        skillInfoView.SetActive(false);

        if(skillBlocks[index].sprite != null)
        {
            SetTargetSKillCanBeSelected(skillBlocks[index].sprite.name);
        }
        if(index == 0)
        {
            if (professionInfo.ContainsKey("skill1"))
            {
                professionInfo["skill1"]=skillName;
                professionInfo["skill1Description"] = descritpion;
                professionInfo["skillOwner1"] = skillOwner;
            }
            else
            {
                professionInfo.Add("skill1", skillName);
                professionInfo.Add("skill1Description", descritpion);
                professionInfo.Add("skillOwner1", skillOwner);
            }
            
        }
        else if(index == 1)
        {
            if (professionInfo.ContainsKey("skill2"))
            {
                professionInfo["skill2"] = skillName;
                professionInfo["skill2Description"] = descritpion;
                professionInfo["skillOwner2"] = skillOwner;
            }
            else
            {
                professionInfo.Add("skill2", skillName);
                professionInfo.Add("skill2Description", descritpion);
                professionInfo.Add("skillOwner2", skillOwner);
            }
        }
        skillBlocks[index].sprite = skillSprite;
        if(index == 0)
        {
            if(skillBlocks[1].sprite == null)
            {
                skillIndex = 1;
            }
        }
    }
    public int GetSkillIndex()
    {
        return skillIndex;
    }
    public void ShowSkillInfoView(string skillName, string skillDescription)
    {
        var infoName = skillInfoView.transform.GetComponentsInChildren<Text>().
                    Where(child => child.transform.name == "Name").ToArray();
        
        var infoDescription = skillInfoView.transform.GetComponentsInChildren<Text>().
                    Where(child => child.transform.name == "Description").ToArray();

        infoName[0].text = skillName;
        infoDescription[0].text = skillDescription;

        skillInfoView.SetActive(true);
    }
    public void HideSkillInfoView()
    {
        skillInfoView.SetActive(false);
    }
    IEnumerator HideSomething(GameObject target, float afterTime)
    {
        float time = 0;
        while(time < afterTime)
        {
            time += Time.deltaTime * 1;
            yield return 1;
        }
        target.SetActive(false);
    }
}
