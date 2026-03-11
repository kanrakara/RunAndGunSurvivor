using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    GameObject player; //対象Player
    Vector3 diff; //距離の差

    [Header("追随スピード")]
    public float followSpeed = 4.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        diff = player.transform.position - transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(player != null)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                player.transform.position - diff,
                Time.deltaTime * followSpeed);
        }
    }
}
