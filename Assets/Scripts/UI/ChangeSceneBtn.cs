using UnityEngine;

public class ChangeSceneBtn : CustomBtn {
    [SerializeField] protected int sceneIndex;

    protected override void ClickedHandle() {
        GameManager.Instance.ChangeScene(sceneIndex);
    }
}