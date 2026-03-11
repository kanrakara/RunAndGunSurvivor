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

    [Header("UIオブジェクト")]
    public UIController ui;

    Coroutine bulletRecover; //発生中のコルーチン情報の参照用

    AudioSource playerAudio;
    [Header("SE音源")]
    public AudioClip se_Reload;

    void Start()
    {
        playerAudio = GetComponent<AudioSource>();
    }

    //弾の消費
    public void ConsumeBullet()
    {
        if (bulletRemaining > 0) //残弾があれば
        {
            bulletRemaining--; //弾を消費
            ui.UpdateBullet(); //UIを更新
        }
    }

    //残数の取得
    public int GetBulletRemaining()
    {
        return bulletRemaining; //現状の残弾を返す
    }

    //マガジン数の取得
    public int GetMagazineRemaining()
    {
        return magazine; //現状の残弾を返す
    }

    //弾の充填
    public void AddBullet(int num)
    {
        bulletRemaining = maxRemaining;
        ui.UpdateBullet(); //UIを更新
    }

    //マガジンの補充
    public void AddMagazine()
    {
        magazine++;
        ui.UpdateMagazine();　 //UIを更新
    }

    //充填メソッド
    public void RecoverBullet()
    {
        //充填コルーチンがすでに走っていれば何もしない
        if (bulletRecover == null) 
        {
            if (magazine > 0) //マガジンの残数があれば補充可能
            {
                playerAudio.PlayOneShot(se_Reload);
                magazine--; //マガジンは消費
                ui.UpdateMagazine(); //UI更新

                //補充コルーチンの発動(Coroutine型の変数に発動したコルーチンの情報を参照させる
                //※Coroutine型の変数が何かを参照していれば、すでにコルーチンが走っていると見なされる（コルーチンの終わりに解放予定）
                bulletRecover = StartCoroutine(RecoverBulletCol());
            }
        }
    }

    //充填コルーチン
    IEnumerator RecoverBulletCol()
    {
        ui.Reloding(); //UI（リロード中）を発動
        
        //カウンターの数字を整える
        counter = recoveryTime;

        while(counter > 0)
        {
            yield return new WaitForSeconds(1.0f); //ウェイト処理
            counter--;
        }
        AddBullet(maxRemaining);//弾数充填

        //充填が終わったのでCoroutine型の変数を解放
        //※またコルーチンが発動できるようにする
        bulletRecover = null;
    }

    //画面上に簡易GUI表示
    void OnGUI()
    {
        ////色を黒にする
        //GUI.color = Color.black;

        ////残弾数を表示
        //string label = "bullet：" + bulletRemaining;
        //GUI.Label(new Rect(50, 50, 100, 30), label);

        ////マガジン数を表示
        //label = "magazine：" + magazine;
        //GUI.Label(new Rect(50, 75, 100, 30), label);

        ////充填コルーチンが走っている間のみ
        //if (bulletRecover != null)
        //{
        //    //色を赤くする
        //    GUI.color = Color.red;

        //    //点滅で充填中であることを表示
        //    float val = Mathf.Sin(Time.time * 50);
        //    if (val > 0) label = "bulletRecover:" + (int)counter;
        //    else label = "";
        //    GUI.Label(new Rect(50, 25, 100, 30), label);
        //}
    }
}
