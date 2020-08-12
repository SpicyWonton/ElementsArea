using GrpcLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RSLoadManager : BaseManager<RSLoadManager>
{
    private bool loadOver = false;  // 场景是否加载完成
    private bool canEnter;          // 是否可进入下一个场景

    // MSNetManager调用，异步场景加载
    public static void LoadLevel()
    {
        instance.StartCoroutine(instance.Load());
    }

    IEnumerator Load()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync("BattleScene");
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            if (ao.progress >= 0.9f)
            {
                if (!loadOver)
                {
                    loadOver = true;
                    // 给服务器发送消息，表示场景已经加载完成
                    Sync sync = new Sync { Tag = true };
                    Client.SendMsg((uint)MSGID.Loadmap, sync);
                }
                // 当所有人都准备好的时候，进入场景
                if (canEnter)
                    ao.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // MSNetManager调用
    public static void SetCanEnter()
    {
        instance.canEnter = true;
    }
}
