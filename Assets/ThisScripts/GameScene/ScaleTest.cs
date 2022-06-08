using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ScaleTest : MonoBehaviour
{
    // Start is called before the first frame update
    public PhotonView pv;
    public bool judge = true;

    public List<GameObject> testForeach = new List<GameObject>();
    [SerializeField] VideoPlayer videoPlayer;
    public List<VideoClip> allVideoClips;
    Dictionary<string, VideoClip> useClipsNameGetClips = new Dictionary<string, VideoClip>();
    public string testVideoName;
    public GameObject rawImage;
    [Range(0, 1)]
    public float testTime;
    
    public float testColor;
    public GameObject showOtherCardsView;
    private void Start()
    {
        foreach(VideoClip clip in allVideoClips)
        {
            useClipsNameGetClips.Add(clip.name, clip);
        }
        showOtherCardsView.SetActive(true);

        Transform layer1 = showOtherCardsView.transform.Find("ShowCardView").Find("layer1");
        Transform layer2 = showOtherCardsView.transform.Find("layer2");

        //List<HashSet<int>> everyWhereCards = allPlayersInfo[0].GetEverywhereCards();
        print(layer1.name);
        int i = 1;
        foreach (Transform child in layer1)
        {
            print(child.name);
            i++;
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 vec = testForeach[0].transform.localScale;
            vec.x += Time.deltaTime * 1;
            vec.y += Time.deltaTime * 1;
            vec.z += Time.deltaTime * 1;
            testForeach[0].transform.localScale = vec;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 vec = testForeach[0].transform.localScale;
            vec.x -= Time.deltaTime * 1;
            vec.y -= Time.deltaTime * 1;
            vec.z -= Time.deltaTime * 1;
            testForeach[0].transform.localScale = vec;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            pv.RPC("testing", RpcTarget.All);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            TestDestroyObjectInForeach();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(PlaySkillAnimation(testVideoName, testTime));
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(ChangeVideoTransparent());
        }
    }
    IEnumerator ChangeVideoTransparent()
    {
        Color color = rawImage.GetComponent<RawImage>().color;


        float time = 1;
        while (time > 0)
        {
            color.a -= (1) / 1 * Time.deltaTime;
            rawImage.GetComponent<RawImage>().color = color;
            time -= Time.deltaTime * 1;
            yield return 1;
        }
        color.a = 0;
        rawImage.GetComponent<RawImage>().color = color;
    }


    [PunRPC]
    IEnumerator testing()
    {
        while (judge)
        {
            print("testing...");
            yield return 1;
        }
    }
    void TestDestroyObjectInForeach()
    {
        int index = 0;
        foreach(GameObject t in testForeach)
        {
            if(index == 1)
            {
                Destroy(t);
            }
            index++;
        }
    }
    IEnumerator PlaySkillAnimation(string animationName, float playingTime)
    {
        
        videoPlayer.clip = null;
        videoPlayer.clip = useClipsNameGetClips[animationName];
        rawImage.SetActive(true);
        videoPlayer.Play();

        while (videoPlayer.frame < (long)videoPlayer.frameCount * playingTime - 1)
        {
            print(videoPlayer.frame + " " + (long)videoPlayer.frameCount * playingTime);
            yield return 1;
        }

        print("playvideo test finished");
        rawImage.SetActive(false);
        videoPlayer.Pause();
    }
}
