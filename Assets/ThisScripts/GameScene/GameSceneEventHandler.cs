using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameSceneEventHandler : MonoBehaviour
{
    public static GameSceneEventHandler Instance;

    private void Awake()
    {
        Instance = this;
    }
    delegate void OnCardsNumberChangeCallbackDelegate(Transform transform);

}