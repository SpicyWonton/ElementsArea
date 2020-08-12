using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Cube
{
    public ParticleSystem particle;//特效
    private ParticleSystem currentParticle;//当前特效

    // Start is called before the first frame update
    void Start()
    {
        
    }
    //爆炸
    protected override void OnCollisionEnter(Collision collision)
    {
        if(openDamage && cubeWeapType == cubeType.Bomb)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
            foreach (var col in colliders)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
                    col.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
            }
            Debug.Log("chansheng");
            currentParticle = Instantiate(particle);
            currentParticle.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            Destroy(gameObject);
        }
    }
}
