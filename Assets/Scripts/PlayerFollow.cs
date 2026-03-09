using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    Vector3 diff;

    public GameObject target;
    public float followSpeed = 5.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        diff = target.transform.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Vector3.Lerpは2つのベクトルを線形補間したベクトルを返すメソッド
        transform.position = Vector3.Lerp(
            transform.position,
            target.transform.position - diff,
            Time.deltaTime * followSpeed
            );
    }
}
