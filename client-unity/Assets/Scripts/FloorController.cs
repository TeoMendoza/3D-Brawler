using UnityEngine;
using SpacetimeDB;
using SpacetimeDB.Types;
using Unity.Cinemachine;

public class FloorController : MonoBehaviour
{
    public uint Id;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Map Floor)
    {
        Id = Floor.Id;
    }
}
