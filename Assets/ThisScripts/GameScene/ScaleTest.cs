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
    public List<bool> testSkillTrueOrFalse;

    public TextAsset skillNameFile;
    public TextAsset skillDescriptionFile;
    public TextAsset skillEnNameFile;
    Dictionary<string, string> useNameGetName = new Dictionary<string, string>();
    Dictionary<string, string> useNameGetDescription = new Dictionary<string, string>();

    public List<int> values = new List<int>();

    private void Start()
    {
        foreach(VideoClip clip in allVideoClips)
        {
            useClipsNameGetClips.Add(clip.name, clip);
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
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(TestVideoCutPoint());
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TestGetFileData();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            TestCompare();
        }
    }
    void TestCompare()
    {

        int max = -1;
        int mid = -1;
        int min = 11;

        for (int i = 0; i < values.Count; i++)
        {
            int value = values[i];
            if(value > max)
            {
                if(max != -1)
                {
                    if(min == 11)
                    {
                        min = max;
                    }
                    else
                    {
                        mid = max;
                    }
                }
                max = value;
            }
            else if(value == max)
            {
                if(min == 11)
                {
                    min = max;
                }
                else
                {
                    mid = max;
                }
                max = value;
            }
            else if(value < max)
            {
                if(min == 11)
                {
                    min = value;
                }
                else
                {
                    if(value < min)
                    {
                        mid = min;
                        min = value;
                    }
                    else
                    {
                        mid = value;
                    }
                }
            }
        }
        if(max == min)
        {
            print("three same");
        }
        else if(max == mid)
        {
            print("two max");
        }
        else
        {
            print("three different");
        }
    }
    void TestGetFileData()
    {
        var enName = skillEnNameFile.text.Split(',');
        var chName = skillNameFile.text.Split(',');
        var description = skillDescriptionFile.text.Split(',');
        for(int i = 0; i<enName.Length; i++)
        {
            useNameGetDescription.Add(enName[i], description[i]);
            useNameGetName.Add(enName[i], chName[i]);
        }

        foreach(KeyValuePair<string,string> keyValue in useNameGetDescription)
        {
            print("enName =  " + keyValue.Key);
            print("description = " + keyValue.Value);
        }
    }
    IEnumerator TestVideoCutPoint()
    {
        videoPlayer.clip = null;
        videoPlayer.clip = useClipsNameGetClips[testVideoName];
        int trueIndex = -1;
        int trueNums = 0;
        bool allFake = true;
        for(int i = 0; i<3; i++)
        {
            if (testSkillTrueOrFalse[i] == true)
            {
                trueNums++;
                allFake = false;
                if (trueIndex == -1) trueIndex = i;
                if (i < 2)
                {

                    float keepTime = trueIndex == 0 ? 0.33f : trueIndex == 1 ? 0.5f : 1;
                    trueIndex = -1;
                    if (trueNums > 1) testVideoName = "random";
                    yield return (PlaySkillAnimation(testVideoName, keepTime));

                    yield return ChangeVideoTransparent();
                }
                else
                {
                    yield return (PlaySkillAnimation(testVideoName, 1));
                    print("finished");
                    rawImage.SetActive(false);
                }
            }
            else
            {
                if(trueNums> 0)
                {
                    Color color = rawImage.GetComponent<RawImage>().color;
                    color.a = 1;
                    rawImage.GetComponent<RawImage>().color = color;
                    videoPlayer.Play();
                    if (i == 1)
                    {
                        while (videoPlayer.frame < (long)videoPlayer.frameCount * 0.66f - 1)
                        {
                            yield return 1;
                        }
                        videoPlayer.Pause();
                        yield return ChangeVideoTransparent();
                    }
                    else if (i == 2)
                    {
                        while (videoPlayer.frame < (long)videoPlayer.frameCount - 1)
                        {
                            yield return 1;
                        }
                        print("finished");
                        rawImage.SetActive(false);
                    }
                }
            }
        }
    }
    IEnumerator ChangeVideoTransparent()
    {
        Color color = rawImage.GetComponent<RawImage>().color;

        float time = 1;
        while (time > 0)
        {
            color.a -= (0.7f) / 1 * Time.deltaTime;
            rawImage.GetComponent<RawImage>().color = color;
            time -= Time.deltaTime * 1;
            yield return 1;
        }
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
        Color color = rawImage.GetComponent<RawImage>().color;
        color.a = 1;
        rawImage.GetComponent<RawImage>().color = color;
        videoPlayer.clip = null;
        videoPlayer.clip = useClipsNameGetClips[animationName];
        rawImage.SetActive(true);
        videoPlayer.Play();

        while (videoPlayer.frame < (long)videoPlayer.frameCount * playingTime - 1)
        {
            yield return 1;
        }
        videoPlayer.Pause();
    }
}
