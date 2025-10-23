using SpacetimeDB;
using UnityEngine;
using SpacetimeDB.Types;
using Unity.Cinemachine;

public class MagicianController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera thirdPersonCam;
    [SerializeField] private GameObject thirdPersonCamPivot;
    public Identity Identity;
    public uint Id;
    // public ReducerTarget ReducerTargetInformation;
    public string Name;
    public uint MatchId;
    public Vector3 TargetPosition;
    public DbRotation2 TargetRotation = new(0, 0);
    public KinematicInformation KinematicInformation;
    public Animator Animator;
    public UnityEngine.CapsuleCollider Collider;
    private Camera mainCamera;
    public Transform attackHand;
    float yaw = 0f; 
    float pitch = 0f;
    private readonly float sensX = 200f;
    private readonly float sensY = 100f;
    private readonly float minPitch = -50f;
    private readonly float maxPitch = 75f;
    readonly float pitchSmooth = 0.08f; // seconds
    float pitchCurrent;
    float pitchVel;

    public void Initalize(Magician Character)
    {
        Identity = Character.Identity;
        Id = Character.Id;
        Name = Character.Name;
        MatchId = Character.MatchId;
        transform.position = Character.Position;
        TargetPosition = Character.Position;
        KinematicInformation = Character.KinematicInformation;

        if (thirdPersonCam != null && Identity.Equals(GameManager.Conn.Identity))
            thirdPersonCam.gameObject.SetActive(true);

        mainCamera = FindFirstObjectByType<CinemachineBrain>().OutputCamera ?? throw new System.Exception("No Main Camera Brain");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Conn.Db.Magician.OnUpdate += HandleMagicianUpdate;
    }

    // Update is called once per frame
    void Update()
    {
        MovementRequest req = new(Velocity: new DbVector3(0, 0, 0), Sprint: false, Jump: false, Crouch: false, Aim: new DbRotation2(0, 0));

        if (Input.GetKey(KeyCode.W)) req.Velocity.Z += 2;
        if (Input.GetKey(KeyCode.S)) req.Velocity.Z -= 2;
        if (Input.GetKey(KeyCode.D)) req.Velocity.X += 1.5f;
        if (Input.GetKey(KeyCode.A)) req.Velocity.X -= 1.5f;
        if (Input.GetKey(KeyCode.LeftShift)) req.Sprint = true;
        if (Input.GetKeyDown(KeyCode.Space)) req.Jump = true;
        if (Input.GetKey(KeyCode.LeftControl)) req.Crouch = true;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        
        yaw = Mathf.Repeat(yaw + mx * sensX * Time.deltaTime, 360f);
        pitch = Mathf.Clamp(pitch - my * sensY * Time.deltaTime, minPitch, maxPitch);

        req.Aim = new DbRotation2(yaw, pitch);
        GameManager.Conn.Reducers.HandleMovementRequestMagician(req);

        // Action Requests (States)
        // if (Input.GetMouseButtonDown(0)) // Left Mouse Button
        // {  
        //     GameManager.Conn.Reducers.HandleActionEnterRequest(request: new ActionRequest (PlayerState: PlayerState.Attack));
        // }
        
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

    public void HandleMagicianUpdate(EventContext context, Magician oldChar, Magician newChar)
    {
        if (Identity != newChar.Identity) return;
        TargetPosition = newChar.Position;
        TargetRotation = newChar.Rotation;

        bool wasGrounded = oldChar.KinematicInformation.Grounded;
        bool Grounded = newChar.KinematicInformation.Grounded;
        bool Landing = newChar.KinematicInformation.Landing;
        bool Crouching = newChar.KinematicInformation.Crouched;
        bool Falling = newChar.KinematicInformation.Falling;
        bool Attack = oldChar.State is MagicianState.Default && newChar.State is MagicianState.Attack;
   
        if (Attack)
            Animator.SetTrigger("Attack");

        if (wasGrounded && Grounded is false)
            Animator.SetTrigger("Jump");

        Animator.SetBool("Crouching", Crouching);
        Animator.SetBool("Falling", Falling);
        Animator.SetBool("Grounded", Grounded);
        Animator.SetBool("Landing", Landing);

        Vector3 vWorld = new Vector3(newChar.Velocity.X, 0f, newChar.Velocity.Z);
        Quaternion yawOnly = Quaternion.Euler(0f, TargetRotation.Yaw, 0f);

        Vector3 vLocal = Quaternion.Inverse(yawOnly) * vWorld;

        float forward = vLocal.z;
        float side = vLocal.x;

        const float damp = 0.1f; // try 0.12–0.20
        Animator.SetFloat("ForwardSpeed",    forward, damp, Time.deltaTime);
        Animator.SetFloat("HorizontalSpeed", side,    damp, Time.deltaTime);
    }



    public void Delete(EventContext context)
    {
        Destroy(gameObject);
    }

    public void HandleFinishedLanding()
    {
        GameManager.Conn.Reducers.MagicianFinishedLanding();
    }
    

    
    
}
