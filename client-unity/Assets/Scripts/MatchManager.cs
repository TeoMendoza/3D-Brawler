using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Linq;
using Unity.VisualScripting;

#nullable enable
public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }
    public uint? MatchId = 1;
    public DbConnection Conn;
    public Dictionary<Identity, MagicianController> Players = new();
    public MagicianController MagicianPrefab;
    public Dictionary<uint, ThrowingCardController> ThrowingCards = new();
    public ThrowingCardController ThrowingCardPrefab;
        
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeMatch(uint MatchId)
    {
        Conn = GameManager.Conn;
        Conn.Db.Magician.OnInsert += AddNewCharacter;
        Conn.Db.Magician.OnDelete += RemoveCharacter;
        Conn.Db.ThrowingCards.OnInsert += AddThrowingCard;
        Conn.Db.ThrowingCards.OnDelete += RemoveThrowingCard;
        
        foreach (Magician Character in Conn.Db.Magician.Iter())
        {
            if (Character.MatchId == MatchId)
            {
                var prefab = Instantiate(MagicianPrefab);
                prefab.Initalize(Character);
                Players.Add(Character.Identity, prefab);
            }
        }
        
        foreach (ThrowingCard throwingCard in Conn.Db.ThrowingCards.Iter())
        {
            if (throwingCard.MatchId == MatchId)
            {
                var prefab = Instantiate(ThrowingCardPrefab);
                prefab.Initalize(throwingCard);
                ThrowingCards.Add(throwingCard.Id, prefab);
            }
        }
    }

    public void EndMatch()
    {
        // Next Step Is To Destroy All Game Objects In Dictionary And Reset Values And Unsubscribe from DB Stuff
    }

    public void AddNewCharacter(EventContext context, Magician Character)
    {
        if (MatchId is not null && Character.MatchId == MatchId)
        {
            var prefab = Instantiate(MagicianPrefab);
            prefab.Initalize(Character);     
            Players.Add(Character.Identity, prefab);
        }
            
    }

    public void RemoveCharacter(EventContext context, Magician Character)
    {
        if (MatchId is not null && Character.MatchId == MatchId)
        {
            Players.TryGetValue(Character.Identity, out var prefab);
            if (prefab != null)
            {
                prefab.Delete(context);
                Players.Remove(Character.Identity);
            }
        }

    }
    
    public void AddThrowingCard(EventContext context, ThrowingCard throwingCard)
    {
        if (MatchId is not null && throwingCard.MatchId == MatchId)
        {
            var prefab = Instantiate(ThrowingCardPrefab);
            prefab.Initalize(throwingCard);     
            ThrowingCards.Add(throwingCard.Id, prefab);
        } 
    }
    
    public void RemoveThrowingCard(EventContext context, ThrowingCard throwingCard)
    {
        if (MatchId is not null && throwingCard.MatchId == MatchId)
        {
            ThrowingCards.TryGetValue(throwingCard.Id, out var prefab);
            if (prefab != null)
            {
                prefab.Delete(context);
                ThrowingCards.Remove(throwingCard.Id);
            }
        }
            
    }
}
