using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] public string userId { get; set; }
    [SerializeField] public int winTurnCount { get; set; }
    [SerializeField] public int score { get; set; }
    [SerializeField] public int winGameCount { get; set; }
    [SerializeField] public bool showCharacter { get; set; }

    [SerializeField] List<GameObject> allSkills;


}
