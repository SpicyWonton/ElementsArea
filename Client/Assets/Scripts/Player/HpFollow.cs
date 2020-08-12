using UnityEngine;
using UnityEngine.UI;

public class HpFollow : MonoBehaviour
{
    public Transform target;    //跟随目标
    public Vector3 offset;      //偏移量
    public Slider slider;       //血条

    private void Start()
    {
        target = transform.parent.parent.transform;
        slider = transform.GetComponent<Slider>();
    }

    private void Update()
    {
        transform.position = target.position + offset;
        transform.forward = Camera.main.transform.forward;
    }

    //更新血量
    public void UpdateHealth()
    {
        slider.value = target.GetComponent<PlayerManager>().currentHp / target.GetComponent<PlayerManager>().maxHp;
    }
}
