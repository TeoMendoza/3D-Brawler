using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;

#nullable enable
public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }
    public uint? MatchId = 1;
    public DbConnection Conn;
    public Dictionary<Identity, PlayableCharacterController> Players = new();
    public PlayableCharacterController PlayableCharacterPrefab;
    
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
        Conn.Db.PlayableCharacter.OnInsert += AddNewCharacter;
        Conn.Db.PlayableCharacter.OnDelete += RemoveCharacter;
    }

    public void EndMatch()
    {

    }

    public void AddNewCharacter(EventContext context, PlayableCharacter Character)
    {
        if (MatchId is not null && Character.MatchId == MatchId)
        {
            var prefab = Instantiate(PlayableCharacterPrefab);
            Players.Add(Character.Identity, prefab);
            
        }
            
    }
    
    public void RemoveCharacter(EventContext context, PlayableCharacter Character)
    {
        if (MatchId is not null && Character.MatchId == MatchId)
        {
            Players.TryGetValue(Character.Identity, out var prefab);
            if (prefab != null)
            {
                Destroy(prefab);
                Players.Remove(Character.Identity);
            }
        }
            
    }
}
