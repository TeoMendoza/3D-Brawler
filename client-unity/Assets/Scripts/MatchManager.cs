using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }
    public GameManager GameManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
