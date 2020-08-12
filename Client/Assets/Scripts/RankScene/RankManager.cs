using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankManager : BaseManager<RankManager>
{
    [Header("排名")]
    public GameObject[] rankingList = new GameObject[6];

    [Header("我的评分")]
    public GameObject myScore;

    [Header("按钮")]
    public Button myScoreBtn;
    public Button continueBtn;

    [Header("评分图片")]
    public Sprite SS;
    public Sprite S;
    public Sprite A;
    public Sprite B;
    public Sprite C;

    [Header("头像图片")]
    public Sprite head0;
    public Sprite head1;
    public Sprite head2;
    public Sprite head3;

    /* 个人得分页面 */

    [Header("个人得分面板")]
    public GameObject myScoreBG;

    [Header("玩家面板")]
    public GameObject playerBG;
    public Sprite playerBG0;
    public Sprite playerBG1;
    public Sprite playerBG2;
    public Sprite playerBG3;

    [Header("个人得分信息")]
    public Image myScoreImage;
    public Text myScoreText;
    public Text addEXP;

    protected override void Init()
    {
        InitRankingList();
        InitMyRankIcon();
        InitButton();
        //关闭个人得分面板
        if (myScoreBG.activeSelf)
            myScoreBG.SetActive(false);
    }

    //初始化排行榜
    private void InitRankingList()
    {
        var players = RoomPlayerManager.Instance.roomPlayers;
        players.Sort();  //按照总得分降序排序
        for (int i = 0; i < players.Count; i++)
        {
            rankingList[i].SetActive(true);
            rankingList[i].transform.GetChild(0).GetComponent<Image>().sprite = GetHeadIcon(players[i].headIcon);
            rankingList[i].transform.GetChild(1).GetComponent<Text>().text = players[i].name;
            rankingList[i].transform.GetChild(2).GetComponent<Image>().sprite = GetRankIcon(players[i].score);
        }
    }

    //初始化我的评分图片
    private void InitMyRankIcon()
    {
        myScore.GetComponent<Image>().sprite = GetRankIcon(User.score);
    }

    //注册按钮监听事件
    private void InitButton()
    {
        myScoreBtn.onClick.AddListener(ShowMyScoreBG);
        continueBtn.onClick.AddListener(ToMainScene);
    }

    //根据下标获取头像
    private Sprite GetHeadIcon(int index)
    {
        switch (index)
        {
            case 0:
                return head0;
            case 1:
                return head1;
            case 2:
                return head2;
            case 3:
                return head3;
            default:
                return null;
        }
    }

    //根据评分获取图片
    private Sprite GetRankIcon(int score)
    {
        if (score >= 800)
            return SS;
        if (score >= 600 && score < 800)
            return S;
        if (score >= 400 && score < 600)
            return A;
        if (score >= 200 && score < 400)
            return B;
        if (score >= 0 && score < 200)
            return C;
        return null;
    }

    //根据下标获取玩家背景
    private Sprite GetPlayerBG(int index)
    {
        switch (index)
        {
            case 0:
                return playerBG0;
            case 1:
                return playerBG1;
            case 2:
                return playerBG2;
            case 3:
                return playerBG3;
            default:
                return null;
        }
    }

    //显示个人得分页面
    private void ShowMyScoreBG()
    {
        myScoreBG.SetActive(true);
        //设置个人信息
        playerBG.GetComponent<Image>().sprite = GetPlayerBG(User.headIcon);
        playerBG.transform.GetChild(0).GetComponent<Text>().text = User.name;
        playerBG.transform.GetChild(1).GetComponent<Text>().text = User.rank.ToString();
        //设置得分信息
        myScoreImage.sprite = GetRankIcon(User.score);
        myScoreText.text = User.score.ToString();
        addEXP.text = User.addEXP.ToString();
    }

    //返回主场景
    private void ToMainScene()
    {
        //清除一些数据
        foreach (var item in rankingList)
            if (item.activeSelf)
                item.SetActive(false);
        RoomPlayerManager.Instance.ClearRoomPlayer();
        //加载场景
        SceneManager.LoadScene("MainScene");
    }
}
