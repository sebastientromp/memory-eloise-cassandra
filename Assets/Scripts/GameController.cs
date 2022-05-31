using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public GameConfig gameConfig;
    public GameObject cardPrefab;
    public CardEventChannel CardEventChannel;

    private MemoryGrid memoryGrid;

    private void Awake()
    {
        GetComponent<AudioSource>().clip = gameConfig.wonAudio;
    }

    private void Start()
    {
        // Clean existing
        var existingContainer = GameObject.Find("CardsContainer");
        if (existingContainer != null)
        {
            DestroyImmediate(existingContainer);
        }
        //float cardScale = 1f; // Use gameConfig.numberOfPairs
        memoryGrid = BuildGrid();

        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight, 0));
        Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, 0));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, 0));

        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;

        float horizontalSpacing = width / (memoryGrid.NumberOfColumns + 1);
        float verticalSpacing = height / (memoryGrid.NumberOfRows);

        var container = new GameObject("CardsContainer");
        foreach (LiveCardInfo info in memoryGrid.CardInfo)
        {
            //Debug.Log($"positioning {bottomLeft} {topRight} {width} {height}");
            float xOffset = (info.LeftPosition + 1) * horizontalSpacing;
            float yOffset = (info.TopPosition + 0.5f) * verticalSpacing;
            Vector3 newPosition = bottomLeft + new Vector3(xOffset, yOffset, 0);
            Vector3 newPositionWithZ = new Vector3(newPosition.x, newPosition.y, 0);
            GameObject newCard = Instantiate(cardPrefab, newPositionWithZ, Quaternion.identity, container.transform);
            newCard.GetComponent<MemoryCard>().liveData = info;
            newCard.GetComponent<MemoryCard>().Init();
        }
    }

    private void OnEnable()
    {
        CardEventChannel.OnEventRaised += OnCardClicked;
    }

    private void OnDisable()
    {
        CardEventChannel.OnEventRaised -= OnCardClicked;
    }

    private bool isClickHandlerRunning;
    public void OnCardClicked(MemoryCard card)
    {
        Debug.Log($"registger click {card.liveData.CardData.sprite}");
        if (isClickHandlerRunning)
        {
            Debug.Log($"isClickHandlerRunning? {isClickHandlerRunning}");
            return;
        }
        isClickHandlerRunning = true;
        StartCoroutine(OnCardClickedCoroutine(card));
    }

    private IEnumerator OnCardClickedCoroutine(MemoryCard card)
    {
        var liveData = card.liveData;
        if (liveData.IsAlreadyMatched)
        {
            Debug.Log($"IsAlreadyMatched? {liveData.CardData.sprite} {liveData.IsAlreadyMatched}");
            isClickHandlerRunning = false;
            yield break;
        }
        if (liveData.IsFaceUp)
        {
            Debug.Log($"IsFaceUp? {liveData.CardData.sprite} {liveData.IsFaceUp}");
            isClickHandlerRunning = false;
            yield break;
        }
        liveData.IsFaceUp = true;
        Debug.Log($"Turning face up? {liveData.CardData.sprite} {liveData.IsFaceUp}");

        var cardsFaceUp = memoryGrid.CardInfo
            .Where(c => !c.IsAlreadyMatched)
            .Where(c => c.IsFaceUp)
            .ToList();
        Debug.Log($"cardsFaceUp {liveData.CardData.sprite} {string.Join(", ", cardsFaceUp.Select(c => c.CardData.sprite))}");
        if (cardsFaceUp.Count <= 1)
        {
            Debug.Log("Only one card, returning");
            isClickHandlerRunning = false;
            yield break;
        }

        var matchingCards = cardsFaceUp
            .Where(c => c.CardData.sprite == liveData.CardData.sprite)
            .ToList();
        Debug.Log($"matchingCards {liveData.CardData.sprite} {string.Join(", ", matchingCards.Select(c => c.CardData.sprite))}");
        if (matchingCards.Count == 2)
        {
            card.PlayMatchingSound();
        }

        yield return new WaitForSeconds(2);
        if (matchingCards.Count == 2)
        {
            matchingCards.ForEach(c => c.IsAlreadyMatched = true);
        }
        else {
            cardsFaceUp.ForEach(c => c.IsFaceUp = false);
        }
        Debug.Log($"cardsFaceUp {liveData.CardData.sprite} {string.Join(", ", cardsFaceUp.Select(c => c.CardData.sprite))}");

        if (memoryGrid.CardInfo.All(c => c.IsAlreadyMatched))
        {
            GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(2);
            Debug.Log("Starting again!");
            Start();
        }

        isClickHandlerRunning = false;
        yield break;

    }

    private MemoryGrid BuildGrid()
    {
        int numberOfCards = gameConfig.numberOfPairs * 2;

        int numberOfRows = Mathf.FloorToInt(Mathf.Sqrt(numberOfCards));
        int numberOfColumns = Mathf.CeilToInt(numberOfCards / numberOfRows);
        Debug.Log($"Rows: {numberOfRows}, Columns: {numberOfColumns}");
        MemoryGrid grid = new MemoryGrid()
        {
            NumberOfRows = numberOfRows,
            NumberOfColumns = numberOfColumns,
            CardInfo = new List<LiveCardInfo>(),
        };

        List<CardData> cardDataToAssign = new List<CardData>();
        cardDataToAssign.AddRange(gameConfig.cardsData);
        cardDataToAssign.AddRange(gameConfig.cardsData);
        List<CardData> shuffled = cardDataToAssign.OrderBy(a => Random.Range(0f, 1f)).ToList();
        Debug.Log($"toAssign: {cardDataToAssign.Select(d => d.sprite).ToList() }, shuffled: {shuffled.Select(d => d.sprite).ToList()}");

        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                if (shuffled.Count == 0)
                {
                    break;
                }

                LiveCardInfo info = new LiveCardInfo() { 
                    CardData = shuffled[0],
                    LeftPosition = j,
                    TopPosition = i,

                };
                grid.CardInfo.Add(info);
                shuffled.RemoveAt(0);
            }
        }
        return grid;
    }
}

public class MemoryGrid
{
    public int NumberOfRows { get; set; }
    public int NumberOfColumns { get; set; }
    public List<LiveCardInfo> CardInfo { get; set; }
}

public class LiveCardInfo
{
    public CardData CardData { get; set; }
    public int LeftPosition { get; set; }
    public int TopPosition { get; set; }
    public bool IsAlreadyMatched { get; set; }
    public bool IsFaceUp { get; set; }
}