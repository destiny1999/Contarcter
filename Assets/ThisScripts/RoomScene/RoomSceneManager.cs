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
    [SerializeField] Button prepare;
    [SerializeField] Button startGame;
    [SerializeField] GameObject playerStatusInRoom;
    [SerializeField] Transform playerList;

    PlayerRoomStatus selfInfo;

    public bool testMode = false;
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        InitRoomInfo();
    }

    // Update is called once per frame
    public void InitRoomInfo()
    {
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            var playerObject = Instantiate(playerStatusInRoom, playerList);
            playerObject.GetComponent<PlayerRoomStatus>().
                SetStatus(player.Value.NickName, player.Value.UserId);
            if(playerObject.GetComponent<PlayerRoomStatus>().GetUserId() ==
                PhotonNetwork.LocalPlayer.UserId)
            {
                selfInfo = playerObject.GetComponent<PlayerRoomStatus>();
            }
        }
        prepare.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
        startGame.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList(newPlayer, true);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList(otherPlayer, false);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            prepare.gameObject.SetActive(false);
            startGame.gameObject.SetActive(true);
            selfInfo.SetPrepareStatusToFalse();
        }
    }

    void UpdatePlayerList(Player targetPlayer, bool add)
    {
        if (add)
        {
            var playerObject = Instantiate(playerStatusInRoom, playerList);
            playerObject.GetComponent<PlayerRoomStatus>().
                SetStatus(targetPlayer.NickName, targetPlayer.UserId);
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
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickPrepare()
    {
        prepare.GetComponentInChildren<Text>().text = prepare.GetComponentInChildren<Text>().text
           == "Prepare" ? "Cancel" : "Prepare";

        HashTable hashTable = new HashTable();
        hashTable.Add("prepareStatus", prepare.GetComponentInChildren<Text>().text);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashTable);
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

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        foreach(Transform player in playerList)
        {
            if(player.GetComponent<PlayerRoomStatus>().GetPlayerName() == targetPlayer.NickName)
            {
                player.GetComponent<PlayerRoomStatus>().ChangePrepareStatus();
            }
        }
    }
}
