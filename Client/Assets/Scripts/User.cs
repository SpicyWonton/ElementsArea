using JetBrains.Annotations;

public static class User
{
    public static string uid;             //用户登录时输入的id
    public static string password;        //用户登录时输入的密码
    public static string name;            //用户的昵称
    public static int rank;               //玩家等级
    public static int exp;                //玩家经验值
    public static int headIcon;           //根据序号选择玩家头像 0-3
    public static int pid = -1;           //每局游戏开始时分配的id
    public static int gameTime;           //一局游戏的时间
    public static bool isHost;            //是否是房主
    public static int seed1;              //第一个随机数种子
    public static int addEXP;             //一局游戏增加的时间
    public static int score;              //一局游戏的评分
    public static bool isSingle;          //是否是单机游戏
}