using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] List<Sprite> cardSpriteList = new List<Sprite>();
    [SerializeField] GameObject cardPrefab;
    [SerializeField] float dealCardTime;
    [SerializeField] float moveTime;
    [SerializeField] Transform shuffleTrans;
    [SerializeField] Transform cardTrans;
    [SerializeField] Animator cardTransAnim;
    [SerializeField] SpriteRenderer currentCardRenderer;

    [Header("Other")]
    [SerializeField] FloorManager floorManager;

    private Stack<int> cardIndexes = new Stack<int>();
    private int currentCardNumber = -1;
    private int currentCardSuit = -1;

    const int numberOfCardInASuit = 13;
    const int numberOfSuit = 4;

    private void Start() {
        Init();
    }

    private void Init() {
        List<int> cardIndexList = new List<int>();

        for (int i = 0; i < cardSpriteList.Count; i++) {
            cardIndexList.Add(i);
        }

        cardIndexList = ShuffleTool.ArrangeList(cardIndexList);

        foreach (int i in cardIndexList) {
            cardIndexes.Push(i);
        }

        DealCard(cardIndexes.Pop());
    }

    private void DealCard(int index) {
        GameObject cardObj = Instantiate(cardPrefab, shuffleTrans.position, Quaternion.identity);
        Card card = cardObj.GetComponent<Card>();
        card.Init(index, cardSpriteList[index], this);

        StartCoroutine(MoveObj(cardObj, shuffleTrans.position, cardTrans.position, dealCardTime));

        if (IsCardValid(index)) {
            cardTransAnim.SetTrigger("Valid");
        }
        else cardTransAnim.SetTrigger("Invalid");
    }

    private IEnumerator MoveObj(GameObject obj, Vector2 startPos, Vector3 endPos, float moveTime) {
        GameManager.Instance.SetGameState(GameState.Pause);

        float timer = 0;
        while (timer < moveTime) {
            timer += Time.deltaTime;
            obj.transform.position = Vector2.Lerp(startPos, endPos, timer / moveTime);
            yield return null;
        }

        obj.transform.position = endPos;

        GameManager.Instance.SetGameState(GameState.Play);
    }

    public bool IsCardValid(int index) {
        if (currentCardNumber == -1 || currentCardSuit == -1)
            return true;
        else {
            int number = numberOfCardInASuit - index % numberOfCardInASuit;
            int suit = index / numberOfCardInASuit;
            
            if (number == currentCardNumber || suit == currentCardSuit)
                return true;
        }
        return false;
    }

    public bool IsSameSuit(int index) {
        return (index / numberOfCardInASuit) == currentCardSuit;
    }

    public void SetCard(Card card, Vector3 startPos, Vector3 endPos) {
        Vector2Int gridPos = (Vector2Int)floorManager.GetCellFromWorld(endPos);

        if (floorManager.CanPlaceObj(gridPos) && floorManager.IsInAvailablePos(gridPos)) {
            Vector3 pos = floorManager.GetWorldFromCell((Vector3Int)gridPos);
            card.SetCollider(false);
            card.SetGridScale();
            card.SetDroppedVisual();
            StartCoroutine(MoveObj(card.gameObject, endPos, pos, 0.25f * moveTime));

            floorManager.AddObject(card.gameObject, gridPos, true);

            if (currentCardNumber == -1 || currentCardSuit == -1) {
                ChangeCurrentCard(card.cardIndex);
            }

            DealCard(cardIndexes.Pop());
        }
        else {
            StartCoroutine(MoveObj(card.gameObject, endPos, startPos, moveTime));
        }
    }

    public void ChangeCurrentCard(int index) {
        currentCardNumber = numberOfCardInASuit - index % numberOfCardInASuit;
        currentCardSuit = index / numberOfCardInASuit;
        currentCardRenderer.sprite = cardSpriteList[index];
    }

    public void ShowMarkers() {
        floorManager.ShowMarkers();
    }

    public void HideMarkers() {
        floorManager.HideMarkers();
    }

    public int CalculateScoreFromIndex(int cardIndex) {
        return numberOfCardInASuit - cardIndex % numberOfCardInASuit;
    }
}
