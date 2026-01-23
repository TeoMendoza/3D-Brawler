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
    public uint? GameId = null;
    public DbConnection Conn;
    public Dictionary<Identity, MagicianController> Players = new();
    public MagicianController MagicianPrefab;
    public Dictionary<uint, MapPiece> MapPieces = new();
    public List<MapPiece> MapPrefabs;
    public bool Initalized = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Initalized && GameId is null && Input.GetKeyDown(KeyCode.P))
            Conn.Reducers.TryJoinGame();
    }

    public void InitializeMatchManager()
    {
        Conn = GameManager.Conn;
        Conn.Db.Magician.OnInsert += AddNewCharacter;
        Conn.Db.Magician.OnDelete += RemoveCharacter;
        Initalized = true;
    }

    public void InitializeMatch()
    {
        foreach (Magician Character in Conn.Db.Magician.Iter())
        {
            if (Character.GameId == GameId && Players.ContainsKey(Character.Identity) is false)
            {
                var prefab = Instantiate(MagicianPrefab);
                prefab.Initalize(Character);
                Players.Add(Character.Identity, prefab);
            }
        }

        foreach (Map MapPiece in Conn.Db.Map.Iter())
        {
            if (MapPieces.ContainsKey((uint)MapPiece.Id)) continue;

            MapPiece MatchingPrefab = default!;

            for (int PrefabIndex = 0; PrefabIndex < MapPrefabs.Count; PrefabIndex++)
            {
                MapPiece CandidatePrefab = MapPrefabs[PrefabIndex];
                if (CandidatePrefab != null && CandidatePrefab.PieceName == MapPiece.Name)
                {
                    MatchingPrefab = CandidatePrefab;
                    break;
                }
            }

            if (MatchingPrefab == null) continue;

            MapPiece Prefab = Instantiate(MatchingPrefab);
            Prefab.Initialize(MapPiece);
            MapPieces.Add((uint)MapPiece.Id, Prefab);
        }
    }

    public void AddNewCharacter(EventContext context, Magician Character)
    {
        if (Character.Identity == GameManager.LocalIdentity)
        {
            GameId = Character.GameId;
            InitializeMatch();
        }

        if (GameId is not null && Character.GameId == GameId && Players.ContainsKey(Character.Identity) is false)
        {
            var prefab = Instantiate(MagicianPrefab);
            prefab.Initalize(Character);     
            Players.Add(Character.Identity, prefab);
        }
            
    }
    public void RemoveCharacter(EventContext context, Magician Character)
    {
        if (GameId is not null && Character.GameId == GameId)
        {
            Players.TryGetValue(Character.Identity, out var prefab);
            if (prefab != null)
            {
                prefab.Delete();
                Players.Remove(Character.Identity);
            }
        }
    }

    public void EndMatch()
    {
        var PlayerIdentities = Players.Keys.ToList();
        for (int Index = 0; Index < PlayerIdentities.Count; Index++)
        {
            var Identity = PlayerIdentities[Index];
            if (Players.TryGetValue(Identity, out var Prefab) && Prefab != null)
            {
                Prefab.Delete();
            }
            Players.Remove(Identity);
        }

        var MapPieceIds = MapPieces.Keys.ToList();
        for (int Index = 0; Index < MapPieceIds.Count; Index++)
        {
            var MapPieceId = MapPieceIds[Index];
            if (MapPieces.TryGetValue(MapPieceId, out var Prefab) && Prefab != null)
            {
                Prefab.Delete();
            }
            MapPieces.Remove(MapPieceId);
        }

        GameId = null;
    }



}
