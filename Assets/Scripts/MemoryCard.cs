using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCard : MonoBehaviour
{
    public LiveCardInfo liveData;
    public CardEventChannel CardEventChannel;
    public GameConfig gameConfig;

    private GameObject cardBack;
    private GameObject cardFront;

    private void Awake()
    {
        cardBack = transform.Find("Back/Icon").gameObject;
        cardBack.GetComponent<SpriteRenderer>().sprite = gameConfig.cardBack;
        cardFront = transform.Find("Front/Icon").gameObject;
        cardFront.SetActive(false);
    }

    public void Init()
    {
        cardFront.GetComponent<SpriteRenderer>().sprite = liveData.CardData.sprite;
        GetComponent<AudioSource>().clip = liveData.CardData.matchSound;
    }

    private void OnMouseDown()
    {
        Debug.Log($"OnMouseDown {liveData.CardData.sprite}");
        CardEventChannel.RaiseEvent(this);
    }

    public void Update()
    {
        if (liveData.IsAlreadyMatched)
        {
            cardBack.SetActive(false);
            cardFront.SetActive(false);
        }
        else if (liveData.IsFaceUp)
        {
            cardBack.SetActive(false);
            cardFront.SetActive(true);
        } 
        else
        {
            cardBack.SetActive(true);
            cardFront.SetActive(false);
        }
    }

    internal void PlayMatchingSound()
    {
        GetComponent<AudioSource>().Play();
    }
}
