using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {
    Pause, Play
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null) {
                    _instance = new GameObject("GameManager").AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }

    private GameState gameState = GameState.Play;
    private bool isFinished;

    public Action OnGameFinished;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
            isFinished = false;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ChangeScene(int index) {
        int maxIndexScene = SceneManager.sceneCountInBuildSettings;
        if (index >= 0 && index <= maxIndexScene + 1) {
            SceneManager.LoadScene(index);
            if (index >= 1) {
                gameState = GameState.Play;
                isFinished = false;
            }
        }
    }

    public void SetGameState(GameState gameState) {
        this.gameState = gameState;
    }

    public bool IsPaused() {
        return gameState == GameState.Pause || isFinished;
    }

    public void FinishGame() {
        isFinished = true;
        OnGameFinished?.Invoke();
    }
}
