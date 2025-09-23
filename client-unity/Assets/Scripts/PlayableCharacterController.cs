using SpacetimeDB;
using UnityEngine;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;

public class PlayableCharacterController : MonoBehaviour
{
    public Identity Identity;
    public uint Id;
    public uint MatchId;
    public DbVector3 position;
    public DbRotation2 rotation;
    public DbVelocity3 velocity;

    public PlayableCharacterController(Identity identity)
    {
        Identity = identity;
    }

    // Next Step Is To Translate Actual Data Of Spacetime DB PlayableCharacter Class Into Data For This Prefab/Controller.
    // Next Step Afterwards Is To Handle Input That Invoke Reducers To Make Changes To Playable Character Row That This Obj Owns (ONLY IF PERMISSION ALLOWS - Use Identity Or Something To Check)
    // Next Step Afterwards Is To Listen For Updates (Only For Row That This Obj Owns), And Reflect Those In The Game Object.
    // Afterwards, Confirm Everything Is Working As A Whole & Then Begin Figuring Out Basic Animations With Basic State Machine Behavior (Permission Config + Attacking States Come After, Projectiles Can Be Added Once The Permissions + Animation States Are Working, Since They Are Seperate Objects)




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
