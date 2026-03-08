using System.Collections;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    const int maxRemaining = 10; //充填数の上限

    [Header("弾数・保有マガジン数")]
    public int bulletRemaining = maxRemaining; //残弾数
    public int magazine = 1; //マガジン数 ※充填時に消費

    [Header("充填時間")]
    public float recoveryTime = 1.0f; //マガジン補充時間
    float counter; //充填までの残時間

    Coroutine bulletRecover; //発生中のコルーチン情報の参照用

    //弾の消費
    public void ConsumeBullet()
    {
        if (bulletRemaining > 0) //残弾があれば
        {
            bulletRemaining--; //ひとつ減らす
        }
    }

    //残数の取得
    public int GetBulletRemaining()
    {
        return bulletRemaining;
    }

    //弾の充填
    public void AddBullet(int num)
    {
        //今の残数を決められた最大の数にする
        bulletRemaining = num;

    }

    //充填メソッド
    public void RecoverBullet()
    {
        if(bulletRecover == null) //コルーチンが発動していないなら充填
        {
            if (magazine > 0) //マガジンの残数があれば
            {
                magazine--; //マガジンを消費

                //コルーチンの発動とコルーチン情報を変数に格納
                bulletRecover = StartCoroutine(RecoverBulletCol());
            }
        }
    }

    //充填コルーチン
    IEnumerator RecoverBulletCol()
    {
        //グローバル変数counterのセットアップ
        counter = recoveryTime;

        while (counter > 0)
        {
            yield return new WaitForSeconds(1.0f); //ウェイト処理
            counter--;
        }
        AddBullet(maxRemaining);
        bulletRecover = null;
    }

    //画面上に簡易GUI表示
    void OnGUI()
    {
        //残弾数を表示（左50、上50、幅100、高さ30:黒色）
        GUI.color = Color.black;
        string label = "bullet：" + bulletRemaining;
        GUI.Label(new Rect(50,50,100,30), label);

        //残マガジンを表示(上75)
        label = "magazine：" + magazine;
        GUI.Label(new Rect(50, 75, 100, 30), label);

        //充填開始～充填完了まで（充填中）
        //赤い文字で点滅表示
        if (bulletRecover != null)
        {
            GUI.color = Color.red; //赤字にする

            float val = Mathf.Sin(Time.time * 50);
            if(val > 0)
            {
                label = "bulletRecover：" + counter;
            }
            else
            {
                label = "";
            }

            GUI.Label(new Rect(50, 25, 100, 30), label);
        }

    }
}
