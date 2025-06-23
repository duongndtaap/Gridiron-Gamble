using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] Vector3 normalScale;
    [SerializeField] Vector3 inGridScale;

    public int cardIndex { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private CardManager manager;
    private Vector3 initialPosition;
    private bool isDragging;

    private Animator animator;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    public void Init(int cardIndex, Sprite sprite, CardManager manager) {
        this.cardIndex = cardIndex;
        spriteRenderer.sprite = sprite;
        this.manager = manager;
    }

    public void SetSprite(Sprite sprite) {
        spriteRenderer.sprite = sprite;
    }

    public void SetCollider(bool isActive) {
        col.enabled = isActive;
    }

    private void OnMouseDown() {
        if (GameManager.Instance.IsPaused()) return;

        manager.ShowMarkers();

        isDragging = true;
        initialPosition = transform.position;
    }

    private void OnMouseDrag() {
        if (isDragging) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            transform.position = mousePos;
        }
    }

    private void OnMouseUp() {
        if (isDragging) {
            isDragging = false;
            manager.SetCard(this, initialPosition, transform.position);
            manager.HideMarkers();
        }
    }

    public void SetDroppedVisual() {
        animator.SetTrigger("Dropped");
    }

    public void SetNormalScale() {
        transform.localScale = normalScale;
    }

    public void SetGridScale() {
        transform.localScale = inGridScale;
    }
}
