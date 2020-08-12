using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NoodManager : MonoBehaviour
{
    public bool moveLevel = false;//移动是否通过了
    public bool pickLevel = false;//捡起方块是否通过了
    public bool downLevel = false;//放下方块是否通过了
    public bool roolLevel = false;//投掷方块是否通过了
    public bool newPitLevel = false;//是否有新地标

    public bool rollError = false;//错误的扔了
    public bool onlyOne = true;//只播放一遍
    public TextManager tm;

    //各种组件
    public GameObject player;
    public GameObject pickObj;
    public GameObject throwObj;
    public AudioSource audioSource;

    public AudioClip[] audioClips;
    
    private Vector3 initPos;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player1");
        initPos = player.transform.position;
        throwObj.GetComponent<ThrowJoystick>().enabled = false;
        audioSource = GetComponent<AudioSource>();

        if(GameObject.Find("BackGroundAudio"))
        {
            GameObject.Find("BackGroundAudio").SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!moveLevel)
        {
            if (Vector3.Distance(player.transform.position, initPos) > 0.5f)
            {
                moveLevel = true;
                tm.currentTime = 10f;
            }
        }
        //在没有捡起的情况下
        else if(!pickLevel)
        {
            if(player.GetComponent<PlayerControl>().haveWeapon)
            {
                pickLevel = true;
                tm.currentTime = 10f;
            }
        }
        //捡起来了 没放下 并且拖动了投掷 第一次
        else if(pickLevel && !downLevel && throwObj.GetComponent<Joystick>().Horizontal!=0)
        {
            onlyOne = false;
            audioSource.clip = audioClips[11];
            audioSource.Play();
        }
        //在捡起完成的情况下 如果当前武器没有了 说明放下了 把扔的脚本打开
        else if(pickLevel && !downLevel)
        {
            if (!player.GetComponent<PlayerControl>().haveWeapon)
            {
                downLevel = true;
                tm.currentTime = 10f;
                throwObj.GetComponent<ThrowJoystick>().enabled = true;
            }
        }
        //当放下完成了 如果当前武器有了 说明扔出去了
        else if(downLevel && !roolLevel)
        {
            if (player.GetComponent<PlayerControl>().haveWeapon)
            {
                roolLevel = true;
                tm.currentTime = 10f;
            }
        }
        //当投掷完成了 并且生成2级地表没完成
        else if (roolLevel && !newPitLevel)
        {
            if (GameObject.FindGameObjectWithTag("FirePit2") || GameObject.FindGameObjectWithTag("WaterPit2") || GameObject.FindGameObjectWithTag("ThunderPit2")
                || GameObject.FindGameObjectWithTag("WTPit") || GameObject.FindGameObjectWithTag("SteamPit"))
            {
                newPitLevel = true;
                tm.currentTime = 10f;
            }
        }

    }

    public void ReturnMainScene() {
        User.isSingle = false;  //取消单机模式
        SceneManager.LoadScene("MainScene");
    }
}
