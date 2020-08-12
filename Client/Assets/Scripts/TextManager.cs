using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    public Sprite[] sourceImages;
    public Image image;
    public NoodManager nm;
    public GameObject enemy;
    public float intervalTime = 0.5f;//间隔时间
    public float currentTime = 0f;//当前时间
    public int[] needOther;//需要其他条件才能进行下一步的话是第几个
    public int i = 0;//当前对话是第几句
    int j = 0;//需要第几个条件

    // Start is called before the first frame update
    void Start()
    {
        image = GameObject.Find("Guide/image").GetComponent<Image>();
        nm = GameObject.Find("NoodManager").GetComponent<NoodManager>();
        image.sprite = sourceImages[0];
        nm.audioSource.clip = nm.audioClips[0];
        nm.audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= intervalTime)
        {
            currentTime = 0f;
            NextDialogue();
        }
    }

    public void NextDialogue()
    {
        //如果进行下一个对话除了 时间还有其他条件
        if (i == needOther[j])
        {
            if (i == needOther[0] && nm.moveLevel)
            {
                i++;
                j++;
                image.sprite = sourceImages[i];
            }
            else if (i == needOther[1] && nm.pickLevel)
            {
                i++;
                j++;
                image.sprite = sourceImages[i];
            }
            else if(i==needOther[2] && nm.downLevel)
            {
                i++;
                j++;
                image.sprite = sourceImages[i];
            }
            else if(i==needOther[3] && nm.roolLevel)
            {
                i++;
                j++;
                image.sprite = sourceImages[i];
            }
            else if (i == needOther[4] && nm.newPitLevel)
            {
                i++;
                j++;
                image.sprite = sourceImages[i];
            }
        }
        //不需要其他条件就能进行下一个对话
        else if (i+1< sourceImages.Length)
        {
            i++;
            image.sprite = sourceImages[i];
            //如果是倒数第二个 实例化一个敌人
            if(i==sourceImages.Length -2 )
            {
                enemy.SetActive(true);
            }
            currentTime +=2f;//非操作 快速下一个
        }
        //音效
        nm.audioSource.clip = nm.audioClips[i];
        nm.audioSource.Play();
    }
}
