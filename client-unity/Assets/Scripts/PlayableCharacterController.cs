using SpacetimeDB;
using UnityEngine;
using System.Collections;
using SpacetimeDB.Types;
using UnityEngine.Playables;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
public class PlayableCharacterController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera thirdPersonCam;
    public Identity Identity;
    public uint Id;
    public string Name;
    public uint MatchId;
    public Vector3 TargetPosition;
    public float snapDist = 0.02f;
    public Animator Animator;
    public bool PrevGrounded = true;

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
        MovementRequest req = new (Velocity: new DbVelocity3(0, 0, 0), Sprint: false, Jump: false);

        if (Input.GetKey(KeyCode.W)) req.Velocity.Vz += 2;
        if (Input.GetKey(KeyCode.S)) req.Velocity.Vz -= 2;
        if (Input.GetKey(KeyCode.D)) req.Velocity.Vx += 2;
        if (Input.GetKey(KeyCode.A)) req.Velocity.Vx -= 2;
        if (Input.GetKey(KeyCode.LeftShift)) req.Sprint = true;
        if (Input.GetKeyDown(KeyCode.Space)) req.Jump = true;

        GameManager.Conn.Reducers.HandleMovementRequest(req);

        // Begin State Machine Implementation With Attacking
        if (Input.GetMouseButtonDown(0))
        { // Left Mouse Button
            //GameManager.Conn.Reducers.HandleActionRequest("Attack");
        }


        // Jitter Handling
        // ApproachRate ≈ how fast you catch up (bigger = snappier)
        float approachRate = 12f;
        float k = 1f - Mathf.Exp(-approachRate * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, TargetPosition, k);
        if (Vector3.Distance(transform.position, TargetPosition) < snapDist)
            transform.position = TargetPosition;
    }

    public void HandlePlayerUpdate(EventContext context, PlayableCharacter oldChar, PlayableCharacter newChar)
    {
        if (Identity != newChar.Identity) return;
        TargetPosition = newChar.Position;

        bool wasGrounded = PrevGrounded;
        float vy = newChar.Velocity.Vy;
        bool grounded = newChar.Position.Y <= 0.001f && vy <= 0;
        

        if (wasGrounded && !grounded && vy > 0f)
            Animator.SetTrigger("Jump");

        Animator.SetBool("IsGrounded", grounded);
        

        float horizSpeed = Mathf.Sqrt(newChar.Velocity.Vx*newChar.Velocity.Vx + newChar.Velocity.Vz*newChar.Velocity.Vz);
        Animator.SetFloat("Speed", horizSpeed);
        Animator.SetFloat("VerticleSpeed", vy);

        PrevGrounded = grounded;

        // if (oldChar.Position.Y <= 0 && newChar.Position.Y > 0) Animator.SetTrigger("Jump");

        // if (newChar.Velocity.Vy < 0 && newChar.Position.Y > 0) Animator.SetBool("Falling", true);
        
        // if (newChar.Position.Y > 0)  Animator.SetBool("IsGrounded", false);
        
        // if (newChar.Position.Y <= 0) {
        //     Animator.SetBool("IsGrounded", true);
        //     Animator.SetBool("Falling", false);
        // }

        // Animator.SetFloat("Speed", newChar.Velocity.Vz);
        
    }


    
}
