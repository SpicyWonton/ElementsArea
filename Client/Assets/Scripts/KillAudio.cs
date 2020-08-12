using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillAudio : MonoBehaviour
{
    public AudioClip[] killClips;//杀人的语音
    public AudioClip[] killedClips;//被杀的语音
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    //杀了其他人播放 只在本地播放
    public void killOthersPlay()
    {
        //随机选择一个语音播放
        System.Random random = new System.Random((int)Time.time);
        audioSource.clip = killClips[random.Next(0,killClips.Length)];
        audioSource.Play();
    }
    
    //其他人杀了我
    public void othersKillPlay()
    {
        //随机选择一个语音播放
        System.Random random = new System.Random((int)Time.time);
        audioSource.clip = killedClips[random.Next(0,killedClips.Length)];
        audioSource.Play();
    }
}
