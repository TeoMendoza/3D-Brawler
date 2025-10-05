using SpacetimeDB;
using UnityEngine;
using System.Collections;
using SpacetimeDB.Types;
using UnityEngine.Playables;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEditor.Experimental.GraphView;
public class PlayableCharacterController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera thirdPersonCam;
    [SerializeField] private GameObject thirdPersonCamPivot;
    public Identity Identity;
    public uint Id;
    public string Name;
    public uint MatchId;
    public Vector3 TargetPosition;
    public DbRotation2 TargetRotation = new(0,0);
    public Animator Animator;
    public bool PrevGrounded = true;

    private Camera mainCamera;
    public Transform attackHand;

    float yaw = 0f; 
    float pitch = 0f;
    [SerializeField] float sensX = 200f, sensY = 100f;   // deg/sec at mouse = 1.0
    [SerializeField] float minPitch = -15f, maxPitch = 15f;
    [SerializeField] float pitchSmooth = 0.08f; // seconds
    float pitchCurrent; 
    float pitchVel;

    public void Initalize(PlayableCharacter Character)
    {
        Identity = Character.Identity;
        Id = Character.Id;
        Name = Character.Name;
        MatchId = Character.MatchId;
        transform.position = Character.Position;

        if (thirdPersonCam != null && Identity.Equals(GameManager.Conn.Identity))
            thirdPersonCam.gameObject.SetActive(true);

        mainCamera = FindFirstObjectByType<CinemachineBrain>().OutputCamera ?? throw new System.Exception("No Main Camera Brain");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Conn.Db.PlayableCharacter.OnUpdate += HandlePlayerUpdate;
    }

    // Update is called once per frame
    void Update()
    {
        MovementRequest req = new(Velocity: new DbVelocity3(0, 0, 0), Sprint: false, Jump: false, Aim: new DbRotation2(0, 0));

        if (Input.GetKey(KeyCode.W)) req.Velocity.Vz += 2;
        if (Input.GetKey(KeyCode.S)) req.Velocity.Vz -= 2;
        if (Input.GetKey(KeyCode.D)) req.Velocity.Vx += 1.5f;
        if (Input.GetKey(KeyCode.A)) req.Velocity.Vx -= 1.5f;
        if (Input.GetKey(KeyCode.LeftShift)) req.Sprint = true;
        if (Input.GetKeyDown(KeyCode.Space)) req.Jump = true;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        
        yaw = Mathf.Repeat(yaw + mx * sensX * Time.deltaTime, 360f);
        pitch = Mathf.Clamp(pitch - my * sensY * Time.deltaTime, minPitch, maxPitch);

        req.Aim = new DbRotation2(yaw, pitch);
        GameManager.Conn.Reducers.HandleMovementRequest(req);

        // Action Requests (States)
        if (Input.GetMouseButtonDown(0)) // Left Mouse Button
        {  
            GameManager.Conn.Reducers.HandleActionEnterRequest(request: new ActionRequest (PlayerState: PlayerState.Attack));
        }
        
    }
    
    void LateUpdate()
    {
        // Jitter Handling
        float k = 1f - Mathf.Exp(-12f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, TargetPosition, k);

        // Rotation & Camera Adjustment
        var targetYaw = TargetRotation.Yaw;
        var targetRot = Quaternion.Euler(0f, targetYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-12f * Time.deltaTime));

        pitchCurrent = Mathf.SmoothDampAngle(pitchCurrent, TargetRotation.Pitch, ref pitchVel, pitchSmooth);
        thirdPersonCamPivot.transform.localRotation = Quaternion.Euler(pitchCurrent, 0f, 0f);
    }

    public void HandlePlayerUpdate(EventContext context, PlayableCharacter oldChar, PlayableCharacter newChar)
    {
        if (Identity != newChar.Identity) return;
        TargetPosition = newChar.Position;
        TargetRotation = newChar.Rotation;

        bool wasGrounded = PrevGrounded;
        float vy = newChar.Velocity.Vy;
        bool grounded = newChar.Position.Y <= 0.001f && vy <= 0;
        bool attack = oldChar.State is PlayerState.Default && newChar.State is PlayerState.Attack;

        if (attack)
            Animator.SetTrigger("Attack");

        if (wasGrounded && !grounded && vy > 0f)
            Animator.SetTrigger("Jump");

        Animator.SetBool("IsGrounded", grounded);


        float horizSpeed = Mathf.Sqrt(newChar.Velocity.Vx * newChar.Velocity.Vx + newChar.Velocity.Vz * newChar.Velocity.Vz);
        Animator.SetFloat("Speed", horizSpeed);
        Animator.SetFloat("VerticleSpeed", vy);

        PrevGrounded = grounded;
    }

    public void OnAttackFinished()
    {
        GameManager.Conn.Reducers.HandleActionExitRequest(newPlayerState: PlayerState.Default);
    }
    
    public void OnAttackAnimation() // Triggers when the hand is at correct position to emulate bullet spawning where we want
    {
        Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray aimRay = mainCamera.ScreenPointToRay(screenCenter);
        Vector3 aimPoint = aimRay.GetPoint(2000f);
        Vector3 projectileDirection = (aimPoint - attackHand.position).normalized;
        GameManager.Conn.Reducers.SpawnProjectile(direction: (DbVector3)projectileDirection, spawnPoint: (DbVector3)attackHand.position);
    }    
}
