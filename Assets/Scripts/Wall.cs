using System.Collections;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [Header("生成プレハブオブジェクト")]
    public GameObject effectPrefab; //生成プレハブ

    [Header("耐久力")]
    public float life = 5.0f; //耐久力

    [Header("ダメージ時間・振動対象・振動スピード・振動量")]
    public float damegeTime = 0.25f; //ダメージ中時間
    public GameObject damageBody; //振動対象オブジェクト
    public float speed = 75.0f; //振動スピード
    public float amplitude = 1.5f;  //振動量

    [Header("スコア点数")]
    public int point = 100;

    Vector3 startPosition; //振動対象の初期位置
    float z; //振動による移動座標

    Coroutine currentDamage; //ダメージコルーチン

    AudioSource enemyAudio;
    [Header("SE音源")]
    public AudioClip se_Damage;

    void Start()
    {
        //振動対象の初期値を取得
        startPosition = damageBody.transform.localPosition;
        enemyAudio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (currentDamage != null)
        {
            z = (amplitude * 0.01f) * Mathf.Sin(Time.time * speed);
            //Debug.Log("移動座標:" + z);
            damageBody.transform.localPosition = startPosition + new Vector3(z, 0, 0);
        }
    }

    //衝突
    void OnTriggerEnter(Collider other)
    {
        if (currentDamage != null) return; //ダメージコルーチン中ならキャンセル

        //衝突相手が「Bullet」の場合
        if(other.gameObject.tag == "Bullet" || other.gameObject.tag == "Sword")
        {
            enemyAudio.PlayOneShot(se_Damage);
            string tag = other.gameObject.tag;
            //ダメージコルーチンを発動
            currentDamage = StartCoroutine(DamageCol(tag));
            if (life <= 0)　//lifeが残っていなければ消滅
            {                
                ScoreManager.ScoreUp(point); //撃破によるスコアアップ
                CreateEffect(); //エフェクト処理と削除
            }
        }
    }

    //ダメージコルーチン
    IEnumerator DamageCol(string tag)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        //耐久力を減らす
        if (tag == "Bullet")
        {
            life -= player.gameObject.GetComponent<NormalShooter>().GetShootPower();
        }
        else if(tag == "Sword")
        {
            life -= player.gameObject.GetComponent<NormalSword>().GetSwordPower(); 
        }
        yield return new WaitForSeconds(damegeTime);
        currentDamage = null; //コルーチン参照を解放
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
