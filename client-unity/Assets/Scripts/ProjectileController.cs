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
    public Identity OwnerIdentity;
    public uint MatchId;
    public Vector3 TargetPosition;

    public void Initalize(Projectile Projectile)
    {
        Id = Projectile.Id;
        OwnerIdentity = Projectile.OwnerIdentity;
        MatchId = Projectile.MatchId;
        transform.position = Projectile.Position;
        TargetPosition = Projectile.Position;

        transform.rotation = Quaternion.FromToRotation(Vector3.up, Projectile.Direction);
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
    
    public void Delete(EventContext context)
    {
        Destroy(gameObject);
    }
}