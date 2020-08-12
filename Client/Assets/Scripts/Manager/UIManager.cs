using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : BaseManager<UIManager>
{
    [Header("剩余时间")]
    public Text minText;            //分钟的Text
    public Text secText;            //秒数的Text
    public KillAudio kA;            //击杀音效

    [Header("计分面板")]
    public GameObject pointsInfo;   //需要在计分面板实例化的预制体
    public Transform pointsPanel;   //计分面板

    [Header("击杀播报")]
    public float killRadioTime;     //播报显示的最长时间
    public GameObject killRadioInfo;//需要在击杀播报面板实例化的预制体
    public Transform killRadioPanel;//击杀播报面板
    public Sprite _3Burn3;          //他人烧死他人
    public Sprite _3Hit3;           //他人砸死他人
    public Sprite _3Burn1;          //他人烧死自己
    public Sprite _3Hit1;           //他人砸死自己
    public Sprite _1Burn3;          //自己烧死他人
    public Sprite _1Hit3;           //自己烧死他人

    [Header("击杀得分")]
    public Sprite onePoint;         //加一分
    public Sprite twoPoint;         //加两分
    public Sprite threePoint;       //加三分

    [Header("倒计时")]
    public Image countDown;
    public Sprite five;
    public Sprite four;
    public Sprite three;
    public Sprite two;
    public Sprite one;

    //表示击杀类型
    public enum KillType
    {
        OBO, OHO,
        OBM, OHM,
        MBO, MHO
    }

    private List<GameObject> pointList = new List<GameObject>();
    private List<GameObject> killList = new List<GameObject>();
    private Dictionary<GameObject, float> killDic = new Dictionary<GameObject, float>();


    private void Start() {
        if(GameObject.Find("KillAudio"))
            kA = GameObject.Find("KillAudio").GetComponent<KillAudio>();
    }
    protected override void Init()
    {
        countDown.gameObject.SetActive(false);
    }

    private void Update()
    {
        for (int i = 0; i < killList.Count; i++)
        {
            killDic[killList[i]] -= Time.deltaTime;
            if (killDic[killList[i]] <= 0)
            {
                killDic.Remove(killList[i]);
                Destroy(killList[i]);
                killList.Remove(killList[i]);
            }
        }
    }

    //在UI上更新时间
    public static void UpdateGameTime(float timeLeft)
    {
        if (timeLeft <= 6)
            instance.ShowCountDown(timeLeft);

        int min = (int)timeLeft / 60;
        int sec = (int)timeLeft - 60 * min;

        if (instance != null)
        {
            instance.minText.text = min.ToString();
            instance.secText.text = sec.ToString();
        }
    }

    //在UI上更新分数
    public static void UpdatePoints()
    {
        //清空上一次的数据
        foreach (var pointsInfo in instance.pointList)
            Destroy(pointsInfo);
        instance.pointList.Clear();
        //显示在UI上
        for (int i = 0; i < RoomPlayerManager.Instance.roomPlayers.Count; i++)
        {
            GameObject pointsInfo = Instantiate(instance.pointsInfo, instance.pointsPanel);
            pointsInfo.transform.GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
            pointsInfo.transform.GetChild(1).GetComponent<Text>().text = RoomPlayerManager.Instance.roomPlayers[i].name;
            pointsInfo.transform.GetChild(2).GetComponent<Text>().text = RoomPlayerManager.Instance.roomPlayers[i].points.ToString();
            instance.pointList.Add(pointsInfo);
        }
    }

    //在UI上更新击杀播报
    public static void UpdateKillRadio(string name1, KillType killType, string name2)
    {
        GameObject killRadio = Instantiate(instance.killRadioInfo, instance.killRadioPanel);
        killRadio.transform.GetChild(0).GetComponent<Text>().text = name1;
        killRadio.transform.GetChild(1).GetComponent<Image>().sprite = instance.ChooseKillImage(killType);
        killRadio.transform.GetChild(2).GetComponent<Text>().text = name2;
        //最多显示3个
        if (instance.killList.Count <= 3)
        {
            instance.killList.Add(killRadio);
            instance.killDic.Add(killRadio, instance.killRadioTime);
        }
        else
        {
            instance.killDic.Remove(instance.killList[0]);
            Destroy(instance.killList[0]);
            instance.killList.Remove(instance.killList[0]);
        }
    }

    //根据击杀类型选择图片
    private Sprite ChooseKillImage(KillType killType)
    {
        switch (killType)
        {
            case KillType.OBO:
                return _3Burn3;
            case KillType.OHO:
                return _3Hit3;
            case KillType.OBM:
                return _3Burn1;
            case KillType.OHM:
                return _3Hit1;
            case KillType.MBO:
                return _1Burn3;
            case KillType.MHO:kA.killOthersPlay();
                return _1Hit3;
            default:
                return null;
        }
    }

    //显示加分UI
    public static void UpdateAddPoint(PlayerManager player, int point)
    {
        //是自己才显示
        if (player.myUID == User.uid)
        {
            player.addPoint.SetActive(true);
            //根据得分选择图片
            switch (point)
            {
                case 1:
                    player.addPoint.GetComponent<Image>().sprite = instance.onePoint;
                    break;
                case 2:
                    player.addPoint.GetComponent<Image>().sprite = instance.twoPoint;
                    break;
                case 3:
                    player.addPoint.GetComponent<Image>().sprite = instance.threePoint;
                    break;
            }
        }
    }

    //显示倒计时
    private void ShowCountDown(float timeLeft)
    {
        countDown.gameObject.SetActive(true);

        if (timeLeft > 5 && timeLeft <= 6)
            countDown.sprite = five;
        else if (timeLeft > 4 && timeLeft <= 5)
            countDown.sprite = four;
        else if (timeLeft > 3 && timeLeft <= 4)
            countDown.sprite = three;
        else if (timeLeft > 2 && timeLeft <= 3)
            countDown.sprite = two;
        else if (timeLeft >= 0 && timeLeft <= 2)
            countDown.sprite = one;
    }
}
