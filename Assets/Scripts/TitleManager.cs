using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("スタート時のシーン")]
    public string nextScene;

    bool inputAvail;

    private void Start()
    {
        StartCoroutine(InputAvailCol());
    }

    IEnumerator InputAvailCol()
    {
        yield return new WaitForSeconds(1.0f);
        inputAvail = true;
    }

    void OnAttack(InputValue value)
    {
        if (inputAvail)
        {
            SceneChange();
        }
    }

    public void SceneChange()
    {
        //トータルスコアをリセット
        ScoreManager.totalScore = 0;
        SceneManager.LoadScene(nextScene);
    }
}
