using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FinishGameDialog : Dialog
{
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] float effectTime;

    protected override void Awake() {
        base.Awake();

        GameManager.Instance.OnGameFinished += GameFinishedHandle;
    }

    private void GameFinishedHandle() {
        Show();
        StartCoroutine(InscreaseScoreEffect(0, scoreManager.GetScore(), effectTime));
    }

    private IEnumerator InscreaseScoreEffect(int startScore, int endScore, float time) {
        float timer = 0;

        while (timer < time) {
            timer += Time.deltaTime;
            scoreText.text = ((int)Mathf.Lerp(startScore, endScore, timer / time)).ToString();
            yield return null;
        }

        scoreText.text = endScore.ToString();
    }

    private void OnDisable() {
        GameManager.Instance.OnGameFinished -= GameFinishedHandle;
    }
}
