using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRun : MonoBehaviour
{
    // 横移動のX軸の限界
    const int MinLane = -2;
    const int MaxLane = 2;
    const float LaneWidth = 1.0f;

    // 体力の最大値
    const int DefaultLife = 3;

    // ダメージを食らったときの硬直時間
    const float StunDuration = 0.5f;

    CharacterController controller;
    Animator animator;

    Vector3 moveDirection = Vector3.zero;       // 移動すべき量
    int targetLane;      // 向かうべき座標
    int life = DefaultLife;     // 現体力
    float recoverTime = 0.0f;       // 復帰までのカウントダウン

    float currentMoveInputX;        // InputSystemの入力値を格納

    // Inputを連続で認知しないためのインターバルのコルーチン
    Coroutine resetIntervalCol;

    public float gravity = 20.0f;       // 重直加速値
    public float speedZ = 5.0f;     // 前進スピード
    public float speedX = 3.0f;     // 横移動スピード
    public float speedJump = 8.0f;      // ジャンプ力
    public float accelerationZ = 10.0f;     // 全身加速力

    [Header("ソードのスクリプト")]
    public NormalSword normalSword;


    void OnMove(InputValue value)
    {
        // NormalSwordスクリプトのisSword変数を見て攻撃中なら何もできない
        if (normalSword.GetIsSword()) { return; }

        // すでに前に入力検知してインターバル中であれば何もしない
        if (resetIntervalCol == null)
        {
            // 検知した値(value)をVector2で表現して変数inputVectorに格納
            Vector2 inputVector = value.Get<Vector2>();
            // 変数inputVectorのうち、x座標にまつわる値を変数correntMoveInutXに格納
            currentMoveInputX = inputVector.x;
        }
    }

    void OnJump(InputValue value)
    {
        // NormalSwordスクリプトのisSword変数を見て攻撃中なら何もできない
        if (normalSword.GetIsSword()) { return; }

        // ジャンプに関するボタン検知をしたらジャンプメソッド
        Jump();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // 現在の体力を返す
    public int Life()
    {
        return (life);
    }

    // 体力を1回復
    public void LifeUP()
    {
        if (life < DefaultLife) { life++; }
        GameObject canvas = GameObject.FindGameObjectWithTag("UI");
        canvas.GetComponent<UIController>().UpdateLife(life);
    }

    public void LifeDown()
    {
        life--;
        GameObject canvas = GameObject.FindGameObjectWithTag("UI");
        canvas.GetComponent<UIController>().UpdateLife(life);
    }


    // Playerを硬直させるべきかチェックするメソッド
    bool IsStun()
    {
        return (recoverTime > 0 || life <= 0);
    }



    // Update is called once per frame
    void Update()
    {
        if (GameManager.gameState == GameState.stageclear || GameManager.gameState == GameState.result) { return; }

        // InputManagerシステム採用の場合
        //if (Input.GetKeyDown("left")) MoveToLeft();
        //if (Input.GetKeyDown("right")) MoveToRight();
        //if (Input.GetKeyDown("space")) Jump();

        // 左を押されていたら
        if (currentMoveInputX < 0) { MoveToLeft(); }

        // 右を押されていたら
        if (currentMoveInputX > 0) { MoveToRight(); }

        if (IsStun())       // 硬直フラグをチェック
        {
            // moveDirectionのxを0
            moveDirection.x = 0;
            // moveDirectionのzを0
            moveDirection.z = 0;

            // recovetTimeをカウントダウン
            recoverTime -= Time.deltaTime;
        }
        else
        {
            // その時のmoveDirection.zにaccelerationZの加速度を足していく
            float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
            // 導き出した値に上限を設けて、それをmoveDirection.zとする
            moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

            // 横移動のアルゴリズム。Updateでフレームごとに少量移動した量を見ながら
            // 目的地と自分の位置の差を取り、1レーンあたりの幅に対しての割合を見る
            float rationX = (targetLane * LaneWidth - transform.position.x) / LaneWidth;
            // 割合に変数speedXを係数としてかけた値がmoveDirection.x
            // 目的から遠いと早く、近いと遅くなる
            moveDirection.x = rationX * speedX;
        }

        // 重力の加速度をmoveDirection.y
        moveDirection.y -= gravity * Time.deltaTime;

        // 回転時、自分にとってのZ軸をグローバル座標の値に変換
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        // CharacterControllerコンポーネントのMoveメソッドに授けてPlayerを動かす
        controller.Move(globalDirection * Time.deltaTime);

        // 地面についていたら重力をリセット
        if (controller.isGrounded) { moveDirection.y = 0; }

    }

    public void MoveToLeft()
    {
        // 硬直フラグがtrueなら何もしない
        if (IsStun()) { return; }
        // 地面にいる且つtargetがまだ最小でない
        if (controller.isGrounded && targetLane > MinLane)
        {
            targetLane--;
            currentMoveInputX = 0;      // 何も入力していない状況にリセット
            // 次の入力検知を有効にするまでのインターバル
            resetIntervalCol = StartCoroutine(ResetIntervalCol());
        }
    }

    public void MoveToRight()
    {
        // 硬直フラグがtrueなら何もしない
        if (IsStun()) { return; }
        // 地面にいる且つtargetがまだ最大でない
        if (controller.isGrounded && targetLane < MaxLane)
        {
            targetLane++;
            currentMoveInputX = 0;      // 何も入力していない状況にリセット
            // 次の入力検知を有効にするまでのインターバル
            resetIntervalCol = StartCoroutine(ResetIntervalCol());
        }
    }

    IEnumerator ResetIntervalCol()
    {
        // とりあえず0.1秒待つ
        yield return new WaitForSeconds(0.1f);
        resetIntervalCol = null;        // コルーチン情報を解除
    }

    public void Jump()
    {
        // 硬直フラグがtrueなら何もしない
        if (IsStun()) { return; }
        if (controller.isGrounded)      // 地面にいたら
        {
            moveDirection.y = speedJump;
        }
    }

    // CharacterControllerコンポーネントが何かとぶつかったとき
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (IsStun()) { return; }

        // 相手がEnemyなら
        if(hit.gameObject.tag == "Enemy")
        {
            LifeDown();     // 体力が減る
            GetComponent<NormalShooter>().ShootPowerDown();        // 銃の威力を減らすメソッド
            recoverTime = StunDuration;     // recoverTimeに定数をセッティング


            // 体力がなくなったらゲームオーバー
            if (life <= 0)
            {
                GameManager.gameState = GameState.gameover;
            }

            //Destroy(hit.gameObject);        // 相手は消滅
            hit.gameObject.GetComponent<Wall>().CreateEffect();
        }
    }

    // ゴールに触れたらステータスをゲームクリアに変更
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Goal")
        {
            GameManager.gameState = GameState.stageclear;
        }
    }


}
