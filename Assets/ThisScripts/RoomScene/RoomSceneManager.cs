using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using HashTable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

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
    [SerializeField] int skillIndex = -1;

    public static RoomSceneManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        InitRoomInfo();
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

    public void OnClickPrepare()
    {
        buttonPrepare.GetComponentInChildren<Text>().text = buttonPrepare.GetComponentInChildren<Text>().text
           == "Prepare" ? "Cancel" : "Prepare";

        HashTable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        ((HashTable)customProperties["prepare"])["prepare"] =
            !(bool)((HashTable)customProperties["prepare"])["prepare"];
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
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
                    print(player.GetComponent<PlayerRoomStatus>().GetPrepareOrNot());
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
        CharacterSelected.GetComponentInChildren<Image>().sprite = characterSprite;
        CharacterSelected.SetActive(true);
        Characters.SetActive(false);
        SkillSelector.SetActive(true);
    }
    public void OnClickBackToSelectCharacters()
    {
        Characters.SetActive(true);
        CharacterSelected.GetComponentInChildren<Image>().sprite = null;
        CharacterSelected.SetActive(false);
        SkillSelector.SetActive(false);
        SetSelectedSkill(0, -1, null);
        SetSelectedSkill(1, -1, null);
    }
    public void SetSelectedSkill(int index, int skillCode, Sprite skillSprite)
    {
        
    }
    public int GetSkillIndex()
    {
        return skillIndex;
    }
}
