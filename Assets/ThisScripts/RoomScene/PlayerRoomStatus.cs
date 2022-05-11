using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRoomStatus : MonoBehaviour
{
    [SerializeField] Text playerName;
    [SerializeField] Image ready;
    string userId;

    public void SetStatus(string name, string ID)
    {
        playerName.text = name;
        userId = ID;
    }
    public string GetPlayerName()
    {
        return playerName.text;
    }
    public string GetUserId()
    {
        return userId;
    }
    public void ChangePrepareStatus()
    {
        ready.gameObject.SetActive(!ready.gameObject.activeSelf);
    }
    public bool GetPrepareOrNot()
    {
        return ready.gameObject.activeSelf ? true : false;
    }
    public void SetPrepareStatusToFalse()
    {
        ready.gameObject.SetActive(false);
    }
}
