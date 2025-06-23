using UnityEngine;

public class CameraFitToBackground : MonoBehaviour
{
    [SerializeField] SpriteRenderer bg;

    private void Start() {
        FitToBackground();
    }

    private void FitToBackground() {
        Camera cam = Camera.main;

        float bgWidth = bg.bounds.size.x;
        float aspect = (float)Screen.width / Screen.height;

        cam.orthographicSize = (bgWidth / aspect) / 2f;
    }
}
