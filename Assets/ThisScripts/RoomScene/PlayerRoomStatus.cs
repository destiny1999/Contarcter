using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRoomStatus : MonoBehaviour
{
    [SerializeField] Text playerName;
    [SerializeField] Image ready;
    public string userId;

    public void SetStatus(string name, string ID, bool prepare)
    {
        playerName.text = name;
        userId = ID;
        ready.gameObject.SetActive(prepare);
    }
    public string GetPlayerName()
    {
        return playerName.text;
    }
    public string GetUserId()
    {
        return userId;
    }
    public void ChangePrepareStatus(bool status)
    {
        ready.gameObject.SetActive(status);
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
