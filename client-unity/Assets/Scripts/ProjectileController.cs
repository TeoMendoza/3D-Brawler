using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Linq;

#nullable enable
public class ProjectileController : MonoBehaviour
{
    public uint Id;
    public uint MatchId;
    public Vector3 TargetPosition;
    public ProjectileType ProjectileType;

    public void Initalize(Projectile Projectile)
    {
        Id = Projectile.Id;
        MatchId = Projectile.MatchId;
        ProjectileType = Projectile.ProjectileType;
        transform.position = Projectile.Position;
        TargetPosition = Projectile.Position;
        transform.rotation = Quaternion.LookRotation(Projectile.Direction, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
    }

    void Start()
    {
        GameManager.Conn.Db.Projectiles.OnUpdate += HandleProjectileUpdate;
    }

    void Update()
    {

    }

    void LateUpdate()
    {
        float k = 1f - Mathf.Exp(-12f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, TargetPosition, k);
    }
    
    public void HandleProjectileUpdate(EventContext context, Projectile oldProjectile, Projectile newProjectile)
    {
        if (Id != newProjectile.Id) return;
        TargetPosition = newProjectile.Position;
    }
}