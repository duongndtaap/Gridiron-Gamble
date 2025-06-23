using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] GameEvent OnScoreChanged;

    private int currentScore = 0;
    private int scoreFactor = 1;
    private bool isIncreasingScoreFactor = false;

    public void AddScore(int score) {
        currentScore += score * scoreFactor;
        OnScoreChanged?.BroadCast();
    }

    public int GetScore() => currentScore;
    public int GetScoreFactor() => scoreFactor;

    public void MultiplyScoreFactor(int factor) {
        scoreFactor *= factor;
    }

    public void IncreaseScoreFactor() {
        if (isIncreasingScoreFactor) {
            scoreFactor += 1;
        }
        else {
            isIncreasingScoreFactor = true;
        }
    }

    public void ReestScoreFactor() {
        scoreFactor = 1;
        isIncreasingScoreFactor = false;
        OnScoreChanged?.BroadCast();
    }
}
