using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddHP : PropsBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<PlayerControl>())
        {
            collision.gameObject.GetComponent<PlayerManager>().AddHp(Value);
            Destroy(this.gameObject);
        }
    }
}
