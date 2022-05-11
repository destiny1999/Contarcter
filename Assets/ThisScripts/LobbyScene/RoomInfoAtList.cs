using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class RoomInfoAtList : MonoBehaviour
{
    SelfRoomInfo roomInfo = new SelfRoomInfo();
    [SerializeField] Text roomName;
    [SerializeField] Image lockImage;
    [SerializeField] Text playerNums;
    
    public SelfRoomInfo GetRoomInfo()
    {
        return roomInfo;
    }
    public void SetRoomShowData()
    {
        roomName.text = roomInfo.roomName;
        playerNums.text = roomInfo.playerNumsInRoom.ToString();
        lockImage.gameObject.SetActive(roomInfo.lockRoom);
    }
    public void SetRoomInfo(int playerNums, string password, bool privateRoom, string roomName)
    {
        roomInfo.playerNumsInRoom = playerNums;
        roomInfo.password = password;
        roomInfo.lockRoom = privateRoom;
        roomInfo.roomName = roomName;
    }
    public void ClickToJoin()
    {
        if(PhotonNetwork.LocalPlayer.NickName.Length > 0 && roomInfo.playerNumsInRoom < 3)
        {
            PhotonNetwork.JoinRoom(roomInfo.roomName);
        }
    }
}
public class SelfRoomInfo
{
    public int playerNumsInRoom = 0;
    public string password = "";
    public bool lockRoom = false;
    public string roomName = "";
}