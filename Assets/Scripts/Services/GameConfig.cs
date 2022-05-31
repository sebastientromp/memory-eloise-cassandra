using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    public CardData[] cardsData;
    public Sprite cardBack;
    public int numberOfPairs;
    public AudioClip wonAudio;
}
