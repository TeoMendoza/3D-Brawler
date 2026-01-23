using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Linq;
using Unity.VisualScripting;
public class MapPiece : MonoBehaviour
{
    public string PieceName;
    public uint Id;
    public void Initialize(Map MapPiece)
    {
        Id = (uint)MapPiece.Id;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}