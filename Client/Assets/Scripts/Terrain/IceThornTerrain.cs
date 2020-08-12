using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceThornTerrain : MonoBehaviour
{
    public float damage = 20f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //如果是玩家 触发冰荆棘状态
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //玩家是否处于打滑状态
            if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerSlider())
            {
                //不处于打滑状态设置为true 造成伤害
                other.gameObject.GetComponent<PlayerManager>().SetPlayerSlider(true);
                other.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
            }
        }
    }

}
