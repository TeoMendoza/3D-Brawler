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
    public string Name;
    public uint MatchId;

    public Vector3 TargetPosition;
    public DbRotation2 TargetRotation = new DbRotation2(0, 0);
    public KinematicInformation KinematicInformation;

    public Animator Animator;
    public Transform CardThrowHand;

    private Camera mainCamera;

    float yaw = 0f;
    float pitch = 0f;

    private readonly float sensX = 200f;
    private readonly float sensY = 100f;
    private readonly float minPitch = -50f;
    private readonly float maxPitch = 75f;

    readonly float pitchSmooth = 0.08f;
    float pitchCurrent;
    float pitchVel;

    Vector3 CameraPositionOffset;
    float CameraYawOffset;
    float CameraPitchOffset;
    Vector3 HandPositionOffset;

    float TargetForwardSpeed;
    float TargetHorizontalSpeed;
    readonly float SpeedBlendTime = 0.15f;

    bool IsOwner;

    public void Initalize(Magician Character)
    {
        Identity = Character.Identity;
        Id = Character.Id;
        Name = Character.Name;
        MatchId = Character.MatchId;

        transform.position = Character.Position;
        TargetPosition = Character.Position;

        TargetRotation = Character.Rotation;
        KinematicInformation = Character.KinematicInformation;

        IsOwner = Identity.Equals(GameManager.LocalIdentity);

        if (thirdPersonCam != null && IsOwner)
            thirdPersonCam.gameObject.SetActive(true);

        mainCamera = FindFirstObjectByType<CinemachineBrain>().OutputCamera != null
            ? FindFirstObjectByType<CinemachineBrain>().OutputCamera
            : throw new System.Exception("No Main Camera Brain");

        Vector3 CameraWorldPosition = mainCamera.transform.position;
        Vector3 CharacterWorldPosition = transform.position;
        CameraPositionOffset = CameraWorldPosition - CharacterWorldPosition;

        CameraYawOffset = Mathf.DeltaAngle(0f, mainCamera.transform.localEulerAngles.y);
        CameraPitchOffset = Mathf.DeltaAngle(0f, mainCamera.transform.localEulerAngles.x);

        Vector2 Reticle = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray AimRay = mainCamera.ScreenPointToRay(Reticle);
        Vector3 D = AimRay.direction.normalized;

        Quaternion MagicianRotation = Quaternion.Euler(TargetRotation.Pitch, TargetRotation.Yaw, 0f);
        Vector3 LocalDir = Quaternion.Inverse(MagicianRotation) * D;

        CameraYawOffset = Mathf.Atan2(LocalDir.x, LocalDir.z);
        CameraPitchOffset = Mathf.Asin(Mathf.Clamp(LocalDir.y, -1f, 1f));
    }

    void Start()
    {
        GameManager.Conn.Db.Magician.OnUpdate += HandleMagicianUpdate;
    }

    void Update()
    {
        if (!IsOwner) return;

        MovementRequest req = new MovementRequest(MoveForward: false, MoveBackward: false, MoveLeft: false, MoveRight: false, Sprint: false, Jump: false, Crouch: false, Aim: new DbRotation2(0, 0) );

        if (Input.GetKey(KeyCode.W)) req.MoveForward = true;
        if (Input.GetKey(KeyCode.S)) req.MoveBackward = true;
        if (Input.GetKey(KeyCode.A)) req.MoveLeft = true;
        if (Input.GetKey(KeyCode.D)) req.MoveRight = true;

        if (Input.GetKey(KeyCode.LeftShift)) req.Sprint = true;
        if (Input.GetKeyDown(KeyCode.Space)) req.Jump = true;
        if (Input.GetKey(KeyCode.LeftControl)) req.Crouch = true;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        yaw = Mathf.Repeat(yaw + mx * sensX * Time.deltaTime, 360f);
        pitch = Mathf.Clamp(pitch - my * sensY * Time.deltaTime, minPitch, maxPitch);

        req.Aim = new DbRotation2(yaw, pitch);

        GameManager.Conn.Reducers.HandleMovementRequestMagician(req);

        if (Input.GetMouseButton(0))
        {
            GameManager.Conn.Reducers.HandleActionChangeRequestMagician(new ActionRequestMagician(State: MagicianState.Attack));
        }
    }

    void LateUpdate()
    {
        float k = 1f - Mathf.Exp(-12f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, TargetPosition, k);

        float targetYaw = TargetRotation.Yaw;
        Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-12f * Time.deltaTime));

        if (thirdPersonCamPivot != null)
        {
            pitchCurrent = Mathf.SmoothDampAngle(pitchCurrent, TargetRotation.Pitch, ref pitchVel, pitchSmooth);
            thirdPersonCamPivot.transform.localRotation = Quaternion.Euler(pitchCurrent, 0f, 0f);
        }

        if (Animator != null)
        {
            Animator.SetFloat("ForwardSpeed", TargetForwardSpeed, SpeedBlendTime, Time.deltaTime);
            Animator.SetFloat("HorizontalSpeed", TargetHorizontalSpeed, SpeedBlendTime, Time.deltaTime);
        }
    }

    public void HandleMagicianUpdate(EventContext context, Magician oldChar, Magician newChar)
    {
        if (Identity != newChar.Identity) return;

        TargetPosition = newChar.Position;
        TargetRotation = newChar.Rotation;

        bool wasGrounded = oldChar.KinematicInformation.Grounded;
        bool Grounded = newChar.KinematicInformation.Grounded;
        bool Crouching = newChar.KinematicInformation.Crouched;
        bool Falling = newChar.KinematicInformation.Falling;
        bool Attacking = newChar.State is MagicianState.Attack;

        DbVector3 Velocity = newChar.IsColliding ? newChar.CorrectedVelocity : newChar.Velocity;
        DbVector3 AnimationVelocity = newChar.Velocity;

        if (Animator != null)
        {
            if (wasGrounded && Grounded is false && Velocity.Y > 2f)
                Animator.SetTrigger("Jump");

            Animator.SetBool("Attacking", Attacking);
            Animator.SetBool("Crouching", Crouching);
            Animator.SetBool("Falling", Falling);
            Animator.SetBool("Grounded", Grounded);
        }

        Vector3 vWorld = new Vector3(AnimationVelocity.X, 0f, AnimationVelocity.Z);
        Quaternion yawOnly = Quaternion.Euler(0f, newChar.Rotation.Yaw, 0f);
        Vector3 vLocal = Quaternion.Inverse(yawOnly) * vWorld;

        TargetForwardSpeed = vLocal.z;
        TargetHorizontalSpeed = vLocal.x;
    }

    public void Delete(EventContext context)
    {
        Destroy(gameObject);
    }

    public void CardThrow()
    {
        float MaxDistance = 100f;

        Vector3 CharacterWorldPosition = transform.position;
        Vector3 HandWorldPosition = CardThrowHand.position;
        Quaternion Rotation = Quaternion.Euler(0, TargetRotation.Yaw, 0);
        HandPositionOffset = Quaternion.Inverse(Rotation) * (HandWorldPosition - CharacterWorldPosition);

        Vector2 Reticle = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray AimRay = mainCamera.ScreenPointToRay(Reticle);
        Vector3 D = AimRay.direction.normalized;

        GameManager.Conn.Reducers.SpawnThrowingCard(
            cameraPositionOffset: (DbVector3)CameraPositionOffset,
            cameraYawOffset: CameraYawOffset,
            cameraPitchOffset: CameraPitchOffset,
            handPositionOffset: (DbVector3)HandPositionOffset,
            maxDistance: MaxDistance
        );

        if (Input.GetMouseButton(0) is false)
        {
            CardThrowFinished();
        }
    }

    public void CardThrowFinished()
    {
        GameManager.Conn.Reducers.HandleActionChangeRequestMagician(new ActionRequestMagician(State: MagicianState.Default));
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Magician"))
        {
            
            MagicianController Player = other.gameObject.GetComponent<MagicianController>();
            if (Player.Id != Id)
            {
                CollisionEntry Entry = new CollisionEntry(Type: CollisionEntryType.Magician, Id: Player.Id);
                GameManager.Conn.Reducers.AddCollisionEntryMagician(Entry, Identity);
            }
            Debug.Log($"Magician Added To Entries With Id: {Player.Id}");
        }
        else if (other.gameObject.CompareTag("ThrowingCard"))
        {
            ThrowingCardController Projectile = other.gameObject.GetComponent<ThrowingCardController>();
            if (Projectile.OwnerIdentity != Identity)
            {
                CollisionEntry Entry = new CollisionEntry(Type: CollisionEntryType.ThrowingCard, Id: Projectile.Id);
                GameManager.Conn.Reducers.AddCollisionEntryMagician(Entry, Identity);
            }
        }
        else if (other.gameObject.CompareTag("Map"))
        {
            FloorController Map = other.gameObject.GetComponent<FloorController>();
            CollisionEntry Entry = new CollisionEntry(Type: CollisionEntryType.Map, Id: Map.Id);
            GameManager.Conn.Reducers.AddCollisionEntryMagician(Entry, Identity);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Magician"))
        {
            MagicianController Player = other.gameObject.GetComponent<MagicianController>();
            if (Player.Id != Id)
            {
                CollisionEntry Entry = new CollisionEntry(Type: CollisionEntryType.Magician, Id: Player.Id);
                GameManager.Conn.Reducers.RemoveCollisionEntryMagician(Entry, Identity);
            }
        }
        else if (other.gameObject.CompareTag("ThrowingCard"))
        {
            ThrowingCardController Projectile = other.gameObject.GetComponent<ThrowingCardController>();
            if (Projectile.OwnerIdentity != Identity)
            {
                CollisionEntry Entry = new CollisionEntry(Type: CollisionEntryType.ThrowingCard, Id: Projectile.Id);
                GameManager.Conn.Reducers.RemoveCollisionEntryMagician(Entry, Identity);
            }
        }
        else if (other.gameObject.CompareTag("Map"))
        {
            FloorController Map = other.gameObject.GetComponent<FloorController>();
            CollisionEntry Entry = new CollisionEntry(Type: CollisionEntryType.Map, Id: Map.Id);
            GameManager.Conn.Reducers.RemoveCollisionEntryMagician(Entry, Identity);
        }
    }
}
