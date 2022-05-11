using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSetting : MonoBehaviour
{
    [SerializeField] Image characterImage;
    [SerializeField] int characterCode;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClickCharacter()
    {
        RoomSceneManager.Instance.SetCharacter(characterCode ,characterImage.sprite);
    }
}
