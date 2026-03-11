using UnityEngine;
using UnityEngine.InputSystem;

public class NormalShooter : MonoBehaviour
{
    [Header("Bullet管理スクリプトと連携")]
    public BulletManager bulletManager;

    [Header("生成オブジェクトと位置")]
    public GameObject bulletPrefabs;//生成対象プレハブ
    public GameObject gate; //生成位置

    [Header("弾速")]
    public float shootSpeed = 30.0f; //弾速

    GameObject bullets; //生成した弾をまとめるオブジェクト

    const int maxShootPower = 3;　//最大威力
    int shootPower = 1; //現在威力

    [Header("ソードのスクリプト")]
    public NormalSword normalSword; //ソード中の動きを封じるため

    AudioSource playerAudio;
    [Header("SE音源")]
    public AudioClip se_Shot;

    //InputAction(Playerマップ)のAttackアクションがおされたら
    void OnAttack(InputValue value)
    {
        if (normalSword.GetIsSword()) return;　//ソード中なら何もできない

        //リトライ状態の時ならやり直し
        if (GameManager.gameState == GameState.retry) 
        {
            GameManager.RetryScene();
        }
        else if(GameManager.gameState == GameState.result) //リザルト状態の時ならネクスト
        {
            GameManager gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
            gm.NextScene(gm.nextScene);
        }
        else //ゲームステータスがプレイ中なら
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletManager.GetBulletRemaining() > 0)
        {
            playerAudio.PlayOneShot(se_Shot);
            //Bulletプレハブを生成
            GameObject obj = Instantiate(
                bulletPrefabs,
                gate.transform.position,
                Quaternion.Euler(90, 0, 0)
                );

            //生成したBulletをBulletsオブジェクトにまとめる
            obj.transform.parent = bullets.transform;

            //生成したBullet自身のRigidbodyを参照
            Rigidbody bulletRbody = obj.GetComponent<Rigidbody>();
            //前方（Z軸）に飛ばす
            bulletRbody.AddForce(new Vector3(0, 0, shootSpeed), ForceMode.Impulse);

            //bulletを消費
            bulletManager.ConsumeBullet();
        }
        else //残弾がない
        {
            //マガジンを消費して弾を補充
            bulletManager.RecoverBullet();
        }
    }

    void Start()
    {
        //指定したタグを持っているオブジェクトを検索
        bullets = GameObject.FindGameObjectWithTag("Bullets");
        playerAudio = GetComponent<AudioSource>();
    }

    //威力を上げる
    public void ShootPowerUp()
    {
        shootPower++; //威力を上げる
        if (shootPower > maxShootPower) shootPower = maxShootPower; //最大威力までに抑える
        GameObject canvas = GameObject.FindGameObjectWithTag("UI"); //UIタグの検索
        canvas.GetComponent<UIController>().UpdateGun(); //UIの更新
    }

    //威力が下がる
    public void ShootPowerDown()
    {
        shootPower--;//威力を下がる
        if (shootPower <= 0) shootPower = 1;//最小でも1
        GameObject canvas = GameObject.FindGameObjectWithTag("UI");//UIタグの検索
        canvas.GetComponent<UIController>().UpdateGun();//UIの更新
    }

    //現在威力の取得
    public int GetShootPower()
    {
        return shootPower;
    }
}
