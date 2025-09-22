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
    public uint? MatchId;
    public DbConnection Conn;
    public Dictionary<Identity, string> Players = new(); // Change to Player Unity Made Script Class, Should be a unity copy of the spacetime DB class, same data just defined in unity
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        Conn = GameManager.Conn;
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeMatch(uint MatchId)
    {
        //

    // Example: subscribe by Identity (you have unique indices Id and Identity)
    }
    
    public void EndMatch()
    {
        
    }
}
