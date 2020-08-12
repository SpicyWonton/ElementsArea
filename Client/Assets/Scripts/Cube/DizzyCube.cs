using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DizzyCube : Cube
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    protected override void OnCollisionEnter(Collision collision)
    {
        //开启伤害的时候
        if(openDamage)
        {
            if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                //掉血触发眩晕
                collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                collision.gameObject.GetComponent<PlayerManager>().SetPlayerDizzy(true);
            }
            Destroy(this.gameObject);
        }
    }
}
