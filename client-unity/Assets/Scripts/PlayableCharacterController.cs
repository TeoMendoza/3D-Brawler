using SpacetimeDB;
using UnityEngine;
using System.Collections.Generic;
using SpacetimeDB.Types;
using UnityEngine.Playables;
using Cinemachine;
public class PlayableCharacterController : MonoBehaviour
{
    //[SerializeField] private Cinemachine thirdPersonCam;
    public Identity Identity;
    public uint Id;
    public string Name;
    public uint MatchId;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Velocity;

    public void Initalize(PlayableCharacter Character)
    {
        Identity = Character.Identity;
        Id = Character.Id;
        Name = Character.Name;
        MatchId = Character.MatchId;
        Position = (Vector3)Character.Position;
        Rotation = (Vector3)Character.Rotation;
        Velocity = (Vector3)Character.Velocity;
    }

    // Next Step Is To Translate Actual Data Of Spacetime DB PlayableCharacter Class Into Data For This Prefab/Controller.
    // Next Step Afterwards Is To Handle Input That Invoke Reducers To Make Changes To Playable Character Row That This Obj Owns (ONLY IF PERMISSION ALLOWS - Use Identity Or Something To Check)
    // Next Step Afterwards Is To Listen For Updates (Only For Row That This Obj Owns), And Reflect Those In The Game Object.
    // Afterwards, Confirm Everything Is Working As A Whole & Then Begin Figuring Out Basic Animations With Basic State Machine Behavior (Permission Config + Attacking States Come After, Projectiles Can Be Added Once The Permissions + Animation States Are Working, Since They Are Seperate Objects)




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Conn.Db.PlayableCharacter.OnUpdate += HandlePlayerUpdate;
    }

    // Update is called once per frame
    void Update()
    {
        var v = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) v.z += 5;
        if (Input.GetKey(KeyCode.S)) v.z -= 5;
        if (Input.GetKey(KeyCode.D)) v.x += 5;
        if (Input.GetKey(KeyCode.A)) v.x -= 5;

        GameManager.Conn.Reducers.HandleMovementRequest((DbVelocity3)v);
    }

    public void HandlePlayerUpdate(EventContext context, PlayableCharacter _oldChar, PlayableCharacter newChar)
    {
        if (Identity == newChar.Identity)
            transform.position = newChar.Position;
    }
}
