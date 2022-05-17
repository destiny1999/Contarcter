using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using HashTable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] double prepareTime;
    bool prepareStep = true;
    double startTimeValue;
    HashTable startTime = new HashTable();
    HashTable currentTime = new HashTable();
    [SerializeField] Text timer;

    [SerializeField] int siteCode;
    [SerializeField] Text siteDescription;
    [SerializeField] List<Material> allCharactersMaterial;
    [SerializeField] Material selfCharacterMaterial;
    [SerializeField] Material leftCharacterMaterial;
    [SerializeField] Material rightCharacterMaterial;

    // Start is called before the first frame update
    void Start()
    {
        prepareStep = true;
        if (PhotonNetwork.IsMasterClient)
        {
            startTimeValue = PhotonNetwork.Time;
            StartCoroutine(SubTime());
        }
    }
    IEnumerator SubTime()
    {
        while(prepareTime > 0 && prepareStep)
        {
            var currentTimeValue = PhotonNetwork.Time;
            var passTime = currentTimeValue - startTimeValue;

            var remainingTime = prepareTime - passTime;
            if (!currentTime.ContainsKey("prepareStepRemainingTime"))
            {
                currentTime.Add("prepareStepRemainingTime", remainingTime);
            }
            else
            {
                currentTime["prepareStepRemainingTime"] = remainingTime;
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(currentTime);
            yield return 1;
        }
    }
    public override void OnRoomPropertiesUpdate(HashTable propertiesThatChanged)
    {
        if (prepareStep)
        {
            if (propertiesThatChanged["prepareStepRemainingTime"] != null)
            {
                var remainingTime = (int)(double)propertiesThatChanged["prepareStepRemainingTime"];
                timer.text = remainingTime + "";
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
