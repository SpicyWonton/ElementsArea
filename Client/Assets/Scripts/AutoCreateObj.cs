using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCreateObj : MonoBehaviour
{
    public GameObject createObj;//需要生成的物体
    public GameObject currObj;//当前的物体
    // Start is called before the first frame update
    void Start()
    {
        currObj = Instantiate(createObj, transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (currObj == null)
        {
            currObj = Instantiate(createObj, transform);
        }
    }

}
