using GrpcLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoiceAudio : MonoBehaviour
{
    //需要初始化 audioSource是自己的pm里面的voicePos上的audiosource
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public GameObject  soundPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void HitButton()
    {
        soundPanel.SetActive(!soundPanel.activeInHierarchy);
        // //随机选择一个语音播放
        // System.Random random = new System.Random((int)Time.time);
        // audioSource.clip = audioClips[random.Next(0,audioClips.Length)];
        // audioSource.Play();
    }
    //发声音
    public void PlayVoice(int i)
    {
        //当前没有说话 才可以点
        if(!audioSource.isPlaying)
        {
            if (i>=audioClips.Length)
            {
                return ;
            }
            audioSource.clip = audioClips[i];
            audioSource.Play();
            audioSource.transform.parent.GetComponent<PlayerManager>().SetAudioImg(true,audioClips[i].length);

            //发送消息
            TalkVoice msg = new TalkVoice
            {
                PID = User.pid,
                VID = i
            };
            Client.SendMsg((uint)MSGID.Voice, msg);
        }
    }
}
