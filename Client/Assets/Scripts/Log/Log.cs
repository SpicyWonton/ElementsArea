using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Log : BaseManager<Log>
{
    public static Queue<string> queue = new Queue<string>();

    protected override void Init()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (queue.Count > 0)
        {
            string content = queue.Dequeue();
            LogFile(content);
        }
    }

    public static void AddContent(string content)
    {
        queue.Enqueue(content);
    }

    public static void LogFile(string content)
    {
        string path = Application.persistentDataPath;
        StreamWriter sw = new StreamWriter(path + "\\TestLog.txt", true);
        sw.WriteLine(content);
        sw.Flush();
        sw.Close();
    }
}
