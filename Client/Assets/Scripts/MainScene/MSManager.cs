using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MSManager : BaseManager<MSManager>
{
    [Header("按钮")]
    public Button userInfoBtn;  //用户信息
    public Button settingBtn;   //设置
    public Button startBtn;     //开始游戏
    public Button noodBtn;      //新手模式

    [Header("需要显示的数据")]
    public Image userPanel;     //用户头像
    public Text userName;       //用户昵称
    public Text rank;           //等级
    public Slider expSlider;    //经验条
    public Text currEXP;        //当前经验值
    public Text totalEXP;       //总经验值

    [Header("头像")]
    public Sprite headIcon0;    //冰
    public Sprite headIcon1;    //草
    public Sprite headIcon2;    //火
    public Sprite headIcon3;    //水

    protected override void Init()
    {
        //初始化数据
        userName.text = User.name;
        rank.text = User.rank.ToString();
        currEXP.text = User.exp.ToString();
        totalEXP.text = ((User.rank - 1) * 50 + 100).ToString();
        expSlider.value = float.Parse(currEXP.text) / float.Parse(totalEXP.text);
        //根据序号选择头像
        switch (User.headIcon)
        {
            case 0:
                userPanel.sprite = headIcon0;
                break;
            case 1:
                userPanel.sprite = headIcon1;
                break;
            case 2:
                userPanel.sprite = headIcon2;
                break;
            case 3:
                userPanel.sprite = headIcon3;
                break;
            default:
                Debug.LogError("头像序号错误");
                break;
        }
        //注册按钮监听事件
        userInfoBtn.onClick.AddListener(ShowUserInfo);
        settingBtn.onClick.AddListener(ShowSettingPanel);
        startBtn.onClick.AddListener(StartGame);
        noodBtn.onClick.AddListener(NoodGame);
    }

    //显示玩家信息
    private void ShowUserInfo()
    {

    }

    //显示设置面板
    private void ShowSettingPanel()
    {

    }

    //进入房间页面
    private void StartGame()
    {
        SceneManager.LoadScene("RoomScene");
    }

    //新手模式
    private void NoodGame()
    {
        User.isSingle = true;
        SceneManager.LoadScene("NoodScene");
    }
}
