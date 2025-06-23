using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreShower : MonoBehaviour
{
    [Header("Scale Effect")]
    [SerializeField] AnimationCurve curve;
    [SerializeField] float scaleFactor;
    [SerializeField] float scaleTime;
    private RectTransform rectTrans;

    [Header("Score")]
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text scoreFactorText;

    private float timer = 0f;
    private int currentScore = 0;

    private void Awake() {
        rectTrans = GetComponent<RectTransform>();
    }

    public void ShowScore() {
        scoreFactorText.text = $"X {scoreManager.GetScoreFactor()}";

        int newScore = scoreManager.GetScore();
        if (currentScore != newScore) {
            currentScore = newScore;

            StopAllCoroutines();
            StartCoroutine(ShowScoreEffect(currentScore));
        }
    }

    private IEnumerator ShowScoreEffect(int score) {
        float scale;

        while (timer < scaleTime) {
            timer += Time.deltaTime;
            if (timer / scaleTime > 0.5f)
                scoreText.text = score.ToString();

            scale = curve.Evaluate(timer / scaleFactor) * scaleFactor;
            rectTrans.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        scale = curve.Evaluate(1);
        rectTrans.localScale = new Vector3(scale, scale, scale);
        timer = 0f;
    }
}
