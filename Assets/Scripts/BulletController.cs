using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float deleteTime = 3.0f;

    void Start()
    {
        Destroy(gameObject,deleteTime);
        
    }

    private void OnTriggerEnter(Collider other)
    {
            Destroy(gameObject);
    }
}
