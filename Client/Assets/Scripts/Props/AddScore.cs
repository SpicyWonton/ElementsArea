using UnityEngine;

public class AddScore : PropsBase
{
    private void Start() 
    {
        Destroy(gameObject,durationTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerControl>())
        {
            collision.gameObject.GetComponent<PlayerManager>().GetSocre(Value);
            UIManager.UpdateAddPoint(collision.gameObject.GetComponent<PlayerManager>(), 3);
            Destroy(gameObject);
        }
    }
}
