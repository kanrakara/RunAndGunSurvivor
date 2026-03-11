using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRun : MonoBehaviour
{
    const int MinLane = -2; //最小レーン番号
    const int MaxLane = 2; //最大レーン番号
    const float LaneWidth = 2.0f; //レーン幅
    const int DefaultLife = 3; //最大体力
    const float StunDuration = 0.5f; //硬直時間

    CharacterController controller;
    Animator animator;

    [Header("対象アニメオブジェクト")]
    public GameObject animeBody; //対象body
    bool isAnime; //retry,result切り替え済みか

    AudioSource[] playerAudio;
    //足音判定
    float footstepInterval = 0.3f; //足音間隔
    float footstepTimer; //時間計測
    [Header("SE音源")]
    public AudioClip se_Walk;
    public AudioClip se_Damage;
    public AudioClip se_Explosion;
    public AudioClip se_Jump;
    public AudioClip se_Dash;
    public AudioClip se_Reload;

    Vector3 moveDirection = Vector3.zero; //Moveメソッドの目標値
    int targetLane; //目標レーン番号
    int life = DefaultLife; //現体力
    float recoverTime = 0.0f; //硬直カウントダウン

    float currentMoveInputX; //インプットされたX値
    Coroutine resetIntervalCol; //インプットのインターバルコルーチン

    [Header("重力・前進・横移動・ジャンプ")]
    public float gravity = 20.0f; //重力加速値
    public float speedZ = 5.0f; //前進スピード
    public float speedX = 3.0f; //横移動スピード
    public float speedJump = 8.0f; //ジャンプ力

    [Header("前進加速力")]
    public float accelerationZ = 10.0f; //加速値

    [Header("ソードのスクリプト")]
    public NormalSword normalSword; //ソード中の動きを封じるため

    //現体力の取得
    public int Life()
    {
        return life;
    }

    //体力の回復
    public void LifeUP()
    {
        life++;
        if (life > DefaultLife) life = DefaultLife;
        GameObject canvas = GameObject.FindGameObjectWithTag("UI"); //UIタグの検索
        canvas.GetComponent<UIController>().UpdateLife(Life()); //UIの更新
    }

    //体力のダメージによる減少
    public void LifeDown()
    {
        life--;
        GameObject canvas = GameObject.FindGameObjectWithTag("UI"); //UIタグの検索
        canvas.GetComponent<UIController>().UpdateLife(Life()); //UIの更新
    }

    //硬直カウントダウン中か体力0なら止まる
    bool IsStun()
    {
        return recoverTime > 0.0f || life <= 0;
    }

    void OnMove(InputValue value)
    {
        if (normalSword.GetIsSword()) return; //ソード中なら何もできない

        if (resetIntervalCol == null)
        {
            Vector2 inputVector = value.Get<Vector2>();
            currentMoveInputX = inputVector.x; // x成分が左右の入力（-1:左, 0:なし, 1:右）
        }
    }
    void OnJump(InputValue value)
    {
        if (normalSword.GetIsSword()) return; //ソード中なら何もできない

        Jump();
    }
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = animeBody.GetComponent<Animator>();
        playerAudio = GetComponents<AudioSource>();
    }

    void Update()
    {
        if (GameManager.gameState == GameState.stageclear || GameManager.gameState == GameState.result) return;
        //if (Input.GetKeyDown("left")) MoveToLeft();
        //if (Input.GetKeyDown("right")) MoveToRight();
        //if (Input.GetKeyDown("space")) Jump();

        //左右の入力があれば
        if (currentMoveInputX < 0) MoveToLeft();
        if (currentMoveInputX > 0) MoveToRight();

        //硬直中なら何もできない
        if (IsStun())
        {
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
            recoverTime -= Time.deltaTime; //カウントダウンする
        }
        else
        {
            //moveDirectionのZ(前進加速)を決める
            float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
            moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

            //moveDirectionのX(横移動)を決める
            float ratioX = (targetLane * LaneWidth - transform.position.x) / LaneWidth;
            moveDirection.x = ratioX * speedX;
        }

        //moveDirectionのX(重力)を決める
        moveDirection.y -= gravity * Time.deltaTime;

        //ローカルの進行方向をグローバルの進行方向に切り替えてMoveメソッドで動かす
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        controller.Move(globalDirection * Time.deltaTime);

        //地面に接触していたらY軸の力は0
        if (controller.isGrounded) moveDirection.y = 0;

        //足音メソッド
        HandleFootsteps();
    }

    //足音メソッド
    void HandleFootsteps()
    {
        //地面にいてプレイヤーが動いていれば
        if (controller.isGrounded && moveDirection.z != 0)
        {
            footstepTimer += Time.deltaTime; //時間計測

            if (footstepTimer >= footstepInterval) //インターバルチェック
            {
                playerAudio[1].PlayOneShot(se_Walk);
                footstepTimer = 0;
            }
        }
        else //動いていなければ時間計測リセット
        {
            footstepTimer = 0f;
        }
    }

    public void MoveToLeft()
    {
        if (IsStun()) return;
        if (controller.isGrounded && targetLane > MinLane)
        {
            playerAudio[0].PlayOneShot(se_Dash);
            targetLane--;
            currentMoveInputX = 0;
            resetIntervalCol = StartCoroutine(ResetIntervalCol());
        }
    }

    public void MoveToRight()
    {
        if (IsStun()) return;
        if (controller.isGrounded && targetLane < MaxLane)
        {
            playerAudio[0].PlayOneShot(se_Dash);
            targetLane++;
            currentMoveInputX = 0;
            resetIntervalCol = StartCoroutine(ResetIntervalCol());
        }
    }

    IEnumerator ResetIntervalCol()
    {
        yield return new WaitForSeconds(0.1f);
        resetIntervalCol = null;
    }

    public void Jump()
    {
        if (IsStun()) return;
        if (controller.isGrounded)
        {
            moveDirection.y = speedJump;
            animator.SetTrigger("jump");
            playerAudio[0].PlayOneShot(se_Jump);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (IsStun()) return;

        if (hit.gameObject.tag == "Enemy")
        {
            playerAudio[2].PlayOneShot(se_Damage);
            LifeDown(); //体力減少
            GetComponent<NormalShooter>().ShootPowerDown(); //威力も減らす
            recoverTime = StunDuration; //硬直カウントダウンの開始

            //もし体力0ならゲームオーバー
            if (life <= 0)
            {
                GameManager.gameState = GameState.gameover;
                if (!isAnime)
                {
                    animator.SetTrigger("retry");
                    isAnime = true;
                }
            }

            animator.SetTrigger("damage");

            //相手のエフェクト発生（と消滅）を発動
            hit.gameObject.GetComponent<Wall>().CreateEffect();

        }
    }

    //ゴールに触れたらステータスをゲームクリアに変更
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Goal")
        {            
            GameManager.gameState = GameState.stageclear;
            if (!isAnime)
            {
                playerAudio[0].PlayOneShot(se_Reload);
                animator.SetTrigger("result");
                isAnime = true;
            }
            Destroy(other.gameObject); //ゴールオブジェクトを抹消
        }
    }

}
