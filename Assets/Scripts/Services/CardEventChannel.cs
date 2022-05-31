using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CardEventChannel", menuName = "Events/Card Event Channel", order = 1)]
public class CardEventChannel : ScriptableObject
{
    public UnityAction<MemoryCard> OnEventRaised;

    public void RaiseEvent(MemoryCard card)
    {
        OnEventRaised?.Invoke(card);
    }
}
