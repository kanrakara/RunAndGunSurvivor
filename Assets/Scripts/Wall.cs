using System.Collections;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [Header("生成プレハブオブジェクト")]
    public GameObject effectPrefab; //生成プレハブ

    [Header("耐久力")]
    public float life = 5.0f; //耐久力

    [Header("ダメージ時間・振動対象・振動スピード・振動量")]
    public float damageTime = 0.25f; //ダメージ中時間
    public GameObject damageBody; //振動対象オブジェクト
    public float speed = 75.0f; //振動スピード
    public float amplitude = 1.5f;  //振動量

    Vector3 startPosition; //振動対象の初期位置
    float x; //振動による移動座標

    Coroutine currentDamage; //ダメージコルーチン

    void Start()
    {
        //振動対象となるオブジェクトのローカル初期値を取得
        startPosition = damageBody.transform.localPosition;
    }

    void Update()
    {
        //もしもダメージコルーチンが発動中だったら振動
        if(currentDamage != null)
        {
            x = (amplitude * 0.01f) * Mathf.Sin(Time.time * speed);
            damageBody.transform.localPosition = startPosition + new Vector3(x, 0, 0);
        }
    }

    //衝突
    void OnTriggerEnter(Collider other)
    {
        if (currentDamage != null) return; //ダメージコルーチン中ならキャンセル

        //衝突相手が「Bullet」タグを持っていた場合
        if (other.gameObject.tag == "Bullet")
        {
            //ダメージコルーチンを発動
            currentDamage = StartCoroutine(DamageCol());
            if (life <= 0)　//lifeが残っていなければ消滅
            {
                CreateEffect();
            }
        }
    }

    //ダメージコルーチン
    IEnumerator DamageCol()
    {
        life--; //体力を減少
        yield return new WaitForSeconds(damageTime);
        //コルーチンを発動していたという情報の解除
        currentDamage = null;
        yield return new WaitForSeconds(0.1f);
        //振動していたボディをもとの位置に戻す
        damageBody.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void CreateEffect()
    {
        //もしエフェクトプレハブが存在していれば
        if (effectPrefab != null)
        {
            //エフェクトプレハブを生成
            Instantiate(
                effectPrefab,
                transform.position,
                Quaternion.identity);
        }

        //Wall自身は削除
        Destroy(gameObject);
    }
}
