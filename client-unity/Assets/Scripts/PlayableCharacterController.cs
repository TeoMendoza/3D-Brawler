using SpacetimeDB;
using UnityEngine;
using System.Collections;
using SpacetimeDB.Types;
using UnityEngine.Playables;
using Unity.Cinemachine;
public class PlayableCharacterController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera thirdPersonCam;
    public Identity Identity;
    public uint Id;
    public string Name;
    public uint MatchId;
    public Vector3 TargetPosition;
    [SerializeField] float snapDist = 0.02f;


    Coroutine lerpRoutine;

    public void Initalize(PlayableCharacter Character)
    {
        Identity = Character.Identity;
        Id = Character.Id;
        Name = Character.Name;
        MatchId = Character.MatchId;


        if (thirdPersonCam != null && Identity.Equals(GameManager.Conn.Identity))
            thirdPersonCam.gameObject.SetActive(true);
        
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

        if (Input.GetKey(KeyCode.W)) v.z += 2;
        if (Input.GetKey(KeyCode.S)) v.z -= 2;
        if (Input.GetKey(KeyCode.D)) v.x += 2;
        if (Input.GetKey(KeyCode.A)) v.x -= 2;

        GameManager.Conn.Reducers.HandleMovementRequest((DbVelocity3)v);

        // approachRate ≈ how fast you catch up (bigger = snappier)
        float approachRate = 12f;

        // k will be ~0.2 for 60 fps with approachRate=12
        float k = 1f - Mathf.Exp(-approachRate * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, TargetPosition, k);
        if (Vector3.Distance(transform.position, TargetPosition) < snapDist)
            transform.position = TargetPosition; // snap when close enough
    }

    public void HandlePlayerUpdate(EventContext context, PlayableCharacter _oldChar, PlayableCharacter newChar)
    {
        if (Identity != newChar.Identity) return;
        TargetPosition = newChar.Position;
        
    }


    
}
