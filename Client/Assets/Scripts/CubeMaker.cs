using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMaker : MonoBehaviour
{
    public GameObject[] cubePrefabs;
    public Transform locateCubes;
    private List<Transform> targetTransforms=new List<Transform>();

    //public int rowNum;
    //public int columnNum;
    //public float betweenDistance;
    public float high;

    public float cd;
    private float _cd;

    public Transform father;

    private System.Random random;

    private void Awake()
    {
        _cd = cd;
        random = new System.Random(User.seed1);
        locateCubes.GetComponentsInChildren(true, targetTransforms);//获取所有定位方块的transform
        targetTransforms.RemoveAt(0);
        MakeCubes();
    }


    private void Update()
    {
        _cd -= Time.deltaTime;
        if (_cd <= 0)
        {
            MakeCubes();
            _cd = cd;
        }
    }

    public void MakeCubes()
    {
        /*
        for (int i = 0; i < rowNum; i++)
        {
            for (int j = 0; j < columnNum; j++)
            {
                Vector3 pos = new Vector3(-0.5f*(rowNum-1)*betweenDistance + betweenDistance * i, high, -0.5f*(columnNum-1) * betweenDistance + betweenDistance * j);
                GameObject cube = Instantiate(cubePrefabs[random.Next(0, cubePrefabs.Length)], pos, Quaternion.identity);
                cube.transform.SetParent(father);
            }
        }
        */
        foreach(Transform t in targetTransforms)
        {
            Vector3 pos = new Vector3(t.position.x, high, t.position.z);
            GameObject cube = Instantiate(cubePrefabs[random.Next(0, cubePrefabs.Length)], pos, Quaternion.identity);
            cube.transform.SetParent(father);
        }
    }
}
