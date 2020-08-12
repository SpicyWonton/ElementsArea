using GrpcLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : BaseManager<LoginManager>
{
    [Header("服务器信息")]
    public string ip = "172.20.80.52";  //IP地址
    public int port = 7777;             //端口号

    [Header("需要显示和关闭的面板")]
    public GameObject signInPanel;      //登录面板
    public GameObject signUpPanel;      //注册面板
    public GameObject warnPanel;        //警告面板

    [Header("登录")]
    public InputField signInID;       //账号
    public InputField signInPWD;      //密码
    public Button signInBtn;          //登录 
    public Button signUpBtn;          //注册

    [Header("注册")]
    public InputField signUpID;       //账号
    public InputField signUpPWD;      //密码
    public InputField signUpName;     //昵称
    public Button confirmBtn;         //确认注册
    public Button cancelBtn;          //取消注册

    [Header("警告")]
    public Button okBtn;

    public enum Wrong { IDLength, IDDigit, PWDLength, SignInFaliure, SignUpFaliure };

    protected override void Init()
    {
        signInBtn.onClick.AddListener(SignIn);
        signUpBtn.onClick.AddListener(ShowSignUpPanel);
        confirmBtn.onClick.AddListener(SignUp);
        cancelBtn.onClick.AddListener(CancelSignUp);
        okBtn.onClick.AddListener(CloseWarnPanel);
    }

    //点击登录按钮时调用
    private void SignIn()
    {
        //获取账号和密码
        string uid = signInID.text;
        string pwd = signInPWD.text;
        //ID长度为6-12
        if (uid.Length < 6 || uid.Length > 12)
        {
            ShowWarnPanel(Wrong.IDLength);
            return;
        }
        //ID是纯数字
        if (!Regex.IsMatch(uid, @"^\d+$"))
        {
            ShowWarnPanel(Wrong.IDDigit);
            return;
        }
        //密码长度为6-12
        if (pwd.Length < 6 || pwd.Length > 12)
        {
            ShowWarnPanel(Wrong.PWDLength);
            return;
        }
        //建立连接
        Client.Connect(ip, port);
        //如果格式正确，就像服务器发送消息
        SignIn msg = new SignIn
        {
            UID = uid,
            Password = pwd
        };
        Client.SendMsg((uint)MSGID.Signin, msg);
    }

    //点击注册按钮时调用
    private void ShowSignUpPanel()
    {
        instance.signInPanel.SetActive(false);
        instance.signUpPanel.SetActive(true);
    }

    //点击注册面板的确认按钮时调用
    private void SignUp()
    {
        //获取账号和密码
        string uid = signUpID.text;
        string pwd = signUpPWD.text;
        string name = signUpName.text;
        //ID长度为6-12
        if (uid.Length < 6 || uid.Length > 12)
        {
            ShowWarnPanel(Wrong.IDLength);
            return;
        }
        //ID是纯数字
        if (!Regex.IsMatch(uid, @"^\d+$"))
        {
            ShowWarnPanel(Wrong.IDDigit);
            return;
        }
        //密码长度为6-12
        if (pwd.Length < 6 || pwd.Length > 12)
        {
            ShowWarnPanel(Wrong.PWDLength);
            return;
        }
        //建立连接
        Client.Connect(ip, port);
        //如果格式正确，就像服务器发送消息
        SignUp msg = new SignUp
        {
            UID = uid,
            Password = pwd,
            Name = name
        };
        Client.SendMsg((uint)MSGID.Signup, msg);
    }

    //点击注册面板的取消按钮时调用
    private void CancelSignUp()
    {
        instance.signUpID.text = "";
        instance.signUpPWD.text = "";
        instance.signUpName.text = "";
        instance.signUpPanel.SetActive(false);
        instance.signInPanel.SetActive(true);
    }

    //点击警告面板的按钮时调用
    private void CloseWarnPanel()
    {
        instance.warnPanel.SetActive(false);
        //账号和密码置空
        instance.signInID.text = "";
        instance.signInPWD.text = "";
        instance.signUpID.text = "";
        instance.signUpPWD.text = "";
        instance.signUpName.text = "";
    }

    //显示警告面板
    public static void ShowWarnPanel(Wrong wrong)
    {
        instance.warnPanel.SetActive(true);
        switch (wrong)
        {
            case Wrong.IDLength:
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "账号长度必须在6到12之间";
                break;
            case Wrong.IDDigit:
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "账号只能包含数字";
                break;
            case Wrong.PWDLength:
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "密码长度必须在6到12之间";
                break;
            case Wrong.SignInFaliure:
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "账号或密码错误";
                break;
            case Wrong.SignUpFaliure:
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "用户名已存在，请重新注册";
                break;
        }
    }
}
