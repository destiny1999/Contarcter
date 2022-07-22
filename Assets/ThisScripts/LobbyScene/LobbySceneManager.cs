using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;
using HashTable = ExitGames.Client.Photon.Hashtable;
public class LobbySceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField] InputField iptfPlayerName;
    [SerializeField] InputField iptfRoomName;
    [SerializeField] Transform roomListParent;
    [SerializeField] GameObject Room;

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("StartScene");
        }
        else
        {
            if(PhotonNetwork.CurrentLobby == null)
            {
                PhotonNetwork.JoinLobby();
            }
        }
        
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public void SetPlayerName()
    {
        PhotonNetwork.LocalPlayer.NickName = iptfPlayerName.text;
        
    }
    public override void OnJoinedLobby()
    {
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("prepare"))
        {
            HashTable prepareStatus = new HashTable();
            prepareStatus.Add("prepare", false);
            //PhotonNetwork.LocalPlayer.SetCustomProperties(prepareStatus);
            PhotonNetwork.LocalPlayer.CustomProperties.Add("prepare", prepareStatus);
        }
        else
        {
            HashTable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            ((HashTable)customProperties["prepare"])["prepare"] = false;
        }
    }

    public string GetRoomName()
    {
        string roomName = iptfRoomName.text;
        return roomName.Trim();
    }
    public void OnClickCreateRoom()
    {
        var roomName = GetRoomName();
        print(roomName);
        if(roomName.Length > 0 && PhotonNetwork.LocalPlayer.NickName.Length > 0)
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 3;
            options.PublishUserId = true;
            PhotonNetwork.CreateRoom(roomName, options);
        }
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("failed");
    }
    public override void OnJoinedRoom()
    {
        print("Success");
        SceneManager.LoadScene("RoomScene");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        RoomInfoAtList[] allRooms;
        allRooms = roomListParent.GetComponentsInChildren<RoomInfoAtList>();
        foreach(RoomInfo info in roomList)
        {
            var roomName = info.Name;
            RoomInfoAtList[] targetRoom = allRooms.
                    Where(target => target.GetRoomInfo().roomName == roomName).ToArray();
            
            if (info.RemovedFromList && targetRoom.Count() > 0)
            {
                DestroyImmediate(targetRoom[0].gameObject);
            }
            else// add or update
            {
                if(targetRoom.Count() > 0)// update
                {
                    targetRoom[0].GetRoomInfo().playerNumsInRoom = info.PlayerCount;
                    targetRoom[0].GetComponent<RoomInfoAtList>().SetRoomShowData();
                }
                else// add
                {
                    if(info.PlayerCount > 0)
                    {
                        var newRoom = Instantiate(Room, roomListParent);
                        newRoom.GetComponent<RoomInfoAtList>().
                            SetRoomInfo(info.PlayerCount, "", false, info.Name);

                        newRoom.GetComponent<RoomInfoAtList>().SetRoomShowData();
                    }
                    
                }
            }
        }

    }
}
