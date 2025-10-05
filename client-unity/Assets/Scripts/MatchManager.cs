using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Linq;

#nullable enable
public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }
    public uint? MatchId = 1;
    public DbConnection Conn;
    public Dictionary<Identity, PlayableCharacterController> Players = new();
    public PlayableCharacterController PlayableCharacterPrefab;
    public Dictionary<uint, ProjectileController> Projectiles = new();
    public ProjectileController ProjectilePrefab;
        
    
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
        Conn.Db.Projectiles.OnInsert += AddNewProjectile;
        Conn.Db.Projectiles.OnDelete += RemoveProjectile;
        
        foreach (PlayableCharacter Character in Conn.Db.PlayableCharacter.Iter())
        {
            if (Character.MatchId == MatchId)
            {
                var prefab = Instantiate(PlayableCharacterPrefab);
                prefab.Initalize(Character);
                Players.Add(Character.Identity, prefab);
            }
        }
        
        foreach (Projectile Projectile in Conn.Db.Projectiles.Iter())
        {
            if (Projectile.MatchId == MatchId)
            {
                var prefab = Instantiate(ProjectilePrefab);
                prefab.Initalize(Projectile);
                Projectiles.Add(Projectile.Id, prefab);
            }
        }
    }

    public void EndMatch()
    {
        // Next Step Is To Destroy All Game Objects In Dictionary And Reset Values And Unsubscribe from DB Stuff
    }

    public void AddNewCharacter(EventContext context, PlayableCharacter Character)
    {
        if (MatchId is not null && Character.MatchId == MatchId)
        {
            var prefab = Instantiate(PlayableCharacterPrefab);
            prefab.Initalize(Character);     
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
    
    public void AddNewProjectile(EventContext context, Projectile Projectile)
    {
        if (MatchId is not null && Projectile.MatchId == MatchId)
        {
            var prefab = Instantiate(ProjectilePrefab);
            prefab.Initalize(Projectile);     
            Projectiles.Add(Projectile.Id, prefab);
        }
            
    }
    
    public void RemoveProjectile(EventContext context, Projectile Projectile)
    {
        if (MatchId is not null && Projectile.MatchId == MatchId)
        {
            Projectiles.TryGetValue(Projectile.Id, out var prefab);
            if (prefab != null)
            {
                Destroy(prefab);
                Projectiles.Remove(Projectile.Id);
            }
        }
            
    }
}
