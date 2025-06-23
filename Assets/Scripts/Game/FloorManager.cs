using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class FloorManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] Tilemap backgroundMap;
    private Vector3 offset;
    private Dictionary<Vector2Int, GameObject> placeList = new Dictionary<Vector2Int, GameObject>();

    [Header("Floor")]
    [SerializeField] int xCnt = 9;
    [SerializeField] int yCnt = 7;
    [SerializeField] float moveTime;
    [SerializeField] float delayMoveTime;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject boxPrefab;
    [SerializeField] GameObject ballPrefab;
    [SerializeField] Sprite boxSprite;
    [SerializeField] int ballScoreFactor;
    private Dictionary<Vector2Int, GameObject> cardObjList = new Dictionary<Vector2Int, GameObject>();

    [Header("Available Positions")]
    [SerializeField] GameObject availablePositionMarkerPrefab; 
    [SerializeField] int poolSize = 20; 
    private Queue<GameObject> markerPool = new Queue<GameObject>(); 
    private List<GameObject> activeMarkers = new List<GameObject>(); 
    private HashSet<Vector2Int> availablePositions = new HashSet<Vector2Int>();

    [Header("Other")]
    [SerializeField] CardManager cardManager;
    [SerializeField] ScoreManager scoreManager;

    const int initialBoxCount = 11;

    private GameObject playerObj;
    private Vector2Int playerGridPos;

    private GameObject ballObj;
    private Vector2Int ballGridPos;

    private Vector2Int[] direction = new Vector2Int[4] {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private int minX, minY, maxX, maxY;

    private void Awake() {
        minX = -xCnt / 2 - 1;
        maxX = xCnt / 2;
        minY = -yCnt / 2 - 1;
        maxY = yCnt / 2;

        offset = backgroundMap.cellSize * 0.5f;

        InitializeMarkerPool();
    }

    private void InitializeMarkerPool() {
        for (int i = 0; i < poolSize; i++) {
            GameObject marker = Instantiate(availablePositionMarkerPrefab, transform);
            marker.SetActive(false);
            markerPool.Enqueue(marker);
        }
    }

    private void Start() {
        Init();
    }

    private void Init() {
        List<int> initialPosList = new List<int>();
        for (int i = 0; i < xCnt * yCnt; i++) {
            initialPosList.Add(i);
        }
        initialPosList = ShuffleTool.ArrangeList(initialPosList);

        Dictionary<Vector2Int, GameObject> boxObjList = new Dictionary<Vector2Int, GameObject>();
        for (int i = 0; i < initialBoxCount; i++) {
            int xPos = initialPosList[i] % xCnt - xCnt / 2 - 1;
            int yPos = initialPosList[i] / xCnt - yCnt / 2 - 1;
            Vector2Int gridPos = new Vector2Int(xPos, yPos);
            Vector3 pos = GetWorldFromCell((Vector3Int)gridPos);

            GameObject boxObj = Instantiate(boxPrefab, transform);
            boxObj.transform.localPosition = pos;
            AddObject(boxObj, gridPos, false);

            boxObjList.Add(gridPos, boxObj);
        }

        playerObj = Instantiate(playerPrefab, transform);
        bool playerObjHasPlace = false;

        foreach (Vector2Int gridPos in boxObjList.Keys) {
            if (!playerObjHasPlace) {
                int cnt = 0;
                foreach (Vector2Int dir in direction) {
                    Vector2Int newGridPos = gridPos + dir;
                    if (GetObject(newGridPos) == null && CanPlaceObj(newGridPos))
                        cnt++;
                }

                if (cnt >= 3) {
                    playerGridPos = gridPos;
                    playerObj.transform.position = GetWorldFromCell((Vector3Int)gridPos);
                    playerObjHasPlace = true;
                    break;
                }
            }
        }

        if (!playerObjHasPlace) {
            Init();
            return;
        }

        GenerateBall();

        InitializeAvailablePositions();
    }

    private void GenerateBall() {
        if (ballObj == null)
            ballObj = Instantiate(ballPrefab, transform);

        if (TryGetFreePos(minX, maxX, minY, maxY, out ballGridPos)) {
            ballObj.SetActive(true);
            ballObj.transform.position = GetWorldFromCell((Vector3Int)ballGridPos);
        }
    }

    private void InitializeAvailablePositions() {
        availablePositions.Clear();
        foreach (Vector2Int pos in placeList.Keys) {
            UpdateAvailablePositionsForNewTile(pos);
        }
    }

    private bool IsPlayerOnBall() {
        return playerGridPos == ballGridPos;
    }

    private IEnumerator PlayerMoveInPath(List<Vector2Int> path) {
        GameManager.Instance.SetGameState(GameState.Pause);
        Vector2Int currentDir = Vector2Int.zero;

        foreach (Vector2Int pos in path) {
            cardObjList.Remove(pos);
            currentDir = pos - playerGridPos;

            yield return StartCoroutine(MoveObj(playerObj, GetWorldFromCell((Vector3Int)playerGridPos), GetWorldFromCell((Vector3Int)pos), moveTime));

            GameObject obj = placeList[pos];
            Card card = obj.GetComponent<Card>();
            card.SetSprite(boxSprite);
            card.SetNormalScale();
            playerGridPos = pos;
            cardManager.ChangeCurrentCard(card.cardIndex);

            if (IsPlayerOnBall()) {
                scoreManager.MultiplyScoreFactor(ballScoreFactor);
                GenerateBall();
            }

            AddMove(card.cardIndex);

            yield return new WaitForSeconds(delayMoveTime);
        }

        bool canMove = false;
        bool hasMove = false;
        Vector2Int newPos = Vector2Int.zero;

        foreach (Vector2Int dir in direction) {
            Vector2Int newGridPos = playerGridPos + dir;
            if (!placeList.ContainsKey(newGridPos))
                canMove = true;
            else {
                Card card = placeList[newGridPos].GetComponent<Card>();
                if (card != null && cardObjList.ContainsKey(newGridPos)) {
                    if (cardManager.IsCardValid(card.cardIndex)) {
                        canMove = true;
                        hasMove = true;
                        newPos = newGridPos;
                        if (currentDir == newPos - playerGridPos) {
                            break;
                        }
                    }
                }
            }
        }

        GameManager.Instance.SetGameState(GameState.Play);

        if (hasMove) {
            List<Vector2Int> newPath = FindLongestPathThroughCards(newPos);
            StartCoroutine(PlayerMoveInPath(newPath));
            yield break;
        }

        EndMove();

        if (!canMove) {
            GameManager.Instance.FinishGame();
        }
    }

    private IEnumerator MoveObj(GameObject obj, Vector2 startPos, Vector3 endPos, float moveTime) {
        float timer = 0;
        while (timer < moveTime) {
            timer += Time.deltaTime;
            obj.transform.position = Vector2.Lerp(startPos, endPos, timer / moveTime);

            yield return null;
        }

        obj.transform.position = endPos;
    }

    private bool CheckFinish() {
        foreach (Vector2Int dir in direction) {
            Vector2Int newGridPos = playerGridPos + dir;
            if (CanPlaceObj(newGridPos)) {
                return false;
            }
            else {
                if (placeList.ContainsKey(newGridPos)) {
                    Card card = placeList[newGridPos].GetComponent<Card>();
                    if (card != null && cardObjList.ContainsKey(newGridPos)) {
                        if (cardManager.IsCardValid(card.cardIndex)) {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private void UpdateAvailablePositionsForNewTile(Vector2Int newPos) {
        if (availablePositions.Contains(newPos)) {
            availablePositions.Remove(newPos);
        }

        foreach (Vector2Int dir in direction) {
            Vector2Int adjacentPos = newPos + dir;
            if (CanPlaceObj(adjacentPos) && GetObject(adjacentPos) == null) {
                availablePositions.Add(adjacentPos);
            }
        }
    }

    public void AddObject(GameObject obj, Vector2Int pos, bool isUpdateAvailablePos) {
        if (!placeList.ContainsKey(pos)) {
            placeList.Add(pos, obj);

            if (isUpdateAvailablePos) {
                cardObjList.Add(pos, obj);
                UpdateAvailablePositionsForNewTile(pos);

                if (CheckFinish()) {
                    GameManager.Instance.FinishGame();
                }

                if (CheckMove(pos)) {
                    Card card = obj.GetComponent<Card>();
                    if (card != null) {
                        if (cardManager.IsCardValid(card.cardIndex)) {
                            List<Vector2Int> path = FindLongestPathThroughCards(pos);
                            StartCoroutine(PlayerMoveInPath(path));
                        }
                    }
                }
            }
        }
    }

    public GameObject GetObject(Vector2Int pos) {
        if (placeList.ContainsKey(pos))
            return placeList[pos];
        return null;
    }

    public bool CanPlaceObj(Vector2Int gridPos) {
        if (!backgroundMap.HasTile((Vector3Int)gridPos) || placeList.ContainsKey(gridPos)) {
            return false;
        }
        return true;
    }

    public bool IsInAvailablePos(Vector2Int gridPos) {
        return availablePositions.Contains(gridPos);
    }

    public bool TryGetFreePos(int minX, int maxX, int minY, int maxY, out Vector2Int freePos) {
        freePos = new Vector2Int(maxX, maxY);

        System.Random rand = new System.Random();
        List<Vector2Int> freePositions = new List<Vector2Int>();

        for (int x = minX; x < maxX; x++) {
            for (int y = minY; y < maxY; y++) {
                Vector2Int pos = new Vector2Int(x, y);
                if (GetObject(pos) == null) {
                    freePositions.Add(pos);
                }
            }
        }

        if (freePositions.Count > 0) {
            int randomIndex = rand.Next(0, freePositions.Count);
            freePos = freePositions[randomIndex];
            return true;
        }

        return false;
    }

    public Vector3 GetWorldFromCell(Vector3Int cell) {
        return backgroundMap.CellToWorld(cell) + offset;
    }

    public Vector3Int GetCellFromWorld(Vector3 worldPos) {
        return backgroundMap.WorldToCell(worldPos);
    }

    private GameObject GetMarkerFromPool() {
        if (markerPool.Count == 0) {
            GameObject marker = Instantiate(availablePositionMarkerPrefab, transform);
            return marker;
        }
        return markerPool.Dequeue();
    }

    private void ReturnMarkerToPool(GameObject marker) {
        marker.SetActive(false);
        markerPool.Enqueue(marker);
    }

    public void ShowMarkers() {
        if (activeMarkers.Count > 0) return;

        foreach (Vector2Int pos in availablePositions) {
            GameObject marker = GetMarkerFromPool();
            marker.transform.position = GetWorldFromCell((Vector3Int)pos);
            marker.SetActive(true);
            activeMarkers.Add(marker);
        }
    }

    public void HideMarkers() {
        foreach (GameObject marker in activeMarkers) {
            ReturnMarkerToPool(marker);
        }
        activeMarkers.Clear();
    }

    private bool CheckMove(Vector2Int gridPos) {
        Vector2Int moveDir = gridPos - playerGridPos;

        foreach (Vector2Int dir in direction) {
            if (moveDir == dir) {
                return true;
            }
        }
        return false;
    }

    private List<Vector2Int> FindLongestPathThroughCards(Vector2Int startCardPos) {
        List<Vector2Int> validCards = new List<Vector2Int>(cardObjList.Keys);

        List<Vector2Int> longestPath = new List<Vector2Int>();
        List<Vector2Int> currentPath = new List<Vector2Int> { startCardPos };
        HashSet<Vector2Int> visited = new HashSet<Vector2Int> { startCardPos };

        FindLongestPathDFS(startCardPos, validCards, currentPath, visited, ref longestPath);

        return longestPath;
    }

    private void FindLongestPathDFS(Vector2Int currentPos, List<Vector2Int> validCards,
        List<Vector2Int> currentPath, HashSet<Vector2Int> visited, ref List<Vector2Int> longestPath) {
        if (currentPath.Count > longestPath.Count) {
            longestPath = new List<Vector2Int>(currentPath);
        }

        foreach (Vector2Int dir in direction) {
            Vector2Int nextPos = currentPos + dir;
            if (validCards.Contains(nextPos) && !visited.Contains(nextPos)) {
                Card card = placeList[nextPos].GetComponent<Card>();
                if (card != null) {
                    if (cardManager.IsSameSuit(card.cardIndex)) {
                        visited.Add(nextPos);
                        currentPath.Add(nextPos);
                        FindLongestPathDFS(nextPos, validCards, currentPath, visited, ref longestPath);
                        currentPath.RemoveAt(currentPath.Count - 1);
                        visited.Remove(nextPos);
                    }
                }
            }
        }
    }


    public void AddMove(int cardIndex) {
        scoreManager.IncreaseScoreFactor();
        scoreManager.AddScore(cardManager.CalculateScoreFromIndex(cardIndex));
    }

    public void EndMove() {
        scoreManager.ReestScoreFactor();
    }
}
