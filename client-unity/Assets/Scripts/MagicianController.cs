using SpacetimeDB;
using UnityEngine;
using SpacetimeDB.Types;
using Unity.Cinemachine;
using Unity.VisualScripting;

public class MagicianController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera thirdPersonCam;
    [SerializeField] private GameObject thirdPersonCamPivot;

    bool IsOwner;
    public Identity Identity;
    public uint Id;
    public string Name;
    public uint MatchId;

    public Vector3 TargetPosition;
    public DbRotation2 TargetRotation = new(0, 0);
    public KinematicInformation KinematicInformation;

    public Animator Animator;
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

    float TargetForwardSpeed;
    float TargetHorizontalSpeed;
    readonly float SpeedBlendTime = 0.15f;

    public void Initalize(Magician Character)
    {
        Identity = Character.Identity;
        Id = (uint)Character.Id;
        Name = Character.Name;
        MatchId = Character.GameId;

        transform.position = Character.Position;
        TargetPosition = Character.Position;

        TargetRotation = Character.Rotation;
        KinematicInformation = Character.KinematicInformation;

        IsOwner = Identity.Equals(GameManager.LocalIdentity);

        if (thirdPersonCam != null)
            thirdPersonCam.gameObject.SetActive(IsOwner);

        if (IsOwner)
            mainCamera = FindFirstObjectByType<CinemachineBrain>()?.OutputCamera ?? throw new System.Exception("No Main Camera Brain OutputCamera");
    }

    void Start()
    {
        GameManager.Conn.Db.Magician.OnUpdate += HandleMagicianUpdate;
    }

    void Update()
    {
        if (!IsOwner) return;

        MovementRequest req = new(MoveForward: false, MoveBackward: false, MoveLeft: false, MoveRight: false, Sprint: false, Jump: false, Crouch: false, Aim: new DbRotation2(0, 0) );

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
            Vector2 reticle = new(Screen.width * 0.5f, Screen.height * 0.5f);
            Ray aimRay = mainCamera.ScreenPointToRay(reticle);
            Vector3 clientReticleDirection = aimRay.direction.normalized;

            Vector3 cameraWorldPosition = mainCamera.transform.position;
            Vector3 characterWorldPosition = transform.position;
            Vector3 cameraWorldDelta = cameraWorldPosition - characterWorldPosition;

            Quaternion magicianRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 localDir = Quaternion.Inverse(magicianRotation) * clientReticleDirection;

            float cameraYawOffset = Mathf.Atan2(localDir.x, localDir.z);
            float cameraPitchOffset = Mathf.Asin(Mathf.Clamp(localDir.y, -1f, 1f));

            float cameraYawRadians = (yaw * Mathf.Deg2Rad) + cameraYawOffset;
            float cameraPitchRadians = (pitch * Mathf.Deg2Rad) + cameraPitchOffset;
            Quaternion cameraRotation = Quaternion.Euler(0f, cameraYawRadians * Mathf.Rad2Deg, 0f) * Quaternion.Euler(cameraPitchRadians * Mathf.Rad2Deg, 0f, 0f);

            Vector3 cameraOffsetLocal = Quaternion.Inverse(cameraRotation) * cameraWorldDelta;

            GameManager.Conn.Reducers.HandleActionChangeRequestMagician(new ActionRequestMagician(State: MagicianState.Attack, new AttackInformation(CameraPositionOffset: new DbVector3(cameraOffsetLocal.x, cameraOffsetLocal.y, cameraOffsetLocal.z), CameraYawOffset: cameraYawOffset, CameraPitchOffset: cameraPitchOffset, SpawnPointOffset: new(0f, 1.15f, 0.45f), MaxDistance: 100f), new ReloadInformation()));
        }

        if (Input.GetKey(KeyCode.R))     
            GameManager.Conn.Reducers.HandleActionChangeRequestMagician(new ActionRequestMagician(State: MagicianState.Reload, new AttackInformation(), new ReloadInformation()));
        
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
        
        bool Jump = oldChar.KinematicInformation.Jump is false && newChar.KinematicInformation.Jump is true;

        bool Attack = oldChar.State is not MagicianState.Attack && newChar.State is MagicianState.Attack;
        bool AttackDone = oldChar.State is MagicianState.Attack && newChar.State is not MagicianState.Attack;

        bool Reload = oldChar.State is not MagicianState.Reload && newChar.State is MagicianState.Reload;
        bool ReloadDone = oldChar.State is MagicianState.Reload && newChar.State is not MagicianState.Reload;

        bool Grounded = newChar.KinematicInformation.Grounded;
        bool Crouching = newChar.KinematicInformation.Crouched;
        bool Falling = newChar.KinematicInformation.Falling;

        if (Animator != null)
        {
            if (Jump) Animator.SetTrigger("Jump");

            if (Attack) Animator.SetTrigger("Attack");
            if (AttackDone) Animator.SetTrigger("AttackDone");

            if (Reload) Animator.SetTrigger("Reload");
            if (ReloadDone) Animator.SetTrigger("ReloadDone");

            Animator.SetBool("Crouching", Crouching);
            Animator.SetBool("Falling", Falling);
            Animator.SetBool("Grounded", Grounded);
        }

        DbVector3 AnimationVelocity = newChar.Velocity;
        Vector3 vWorld = new(AnimationVelocity.X, 0f, AnimationVelocity.Z);
        Quaternion yawOnly = Quaternion.Euler(0f, newChar.Rotation.Yaw, 0f);
        Vector3 vLocal = Quaternion.Inverse(yawOnly) * vWorld;

        TargetForwardSpeed = vLocal.z;
        TargetHorizontalSpeed = vLocal.x;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Magician"))
        {
            MagicianController Player = other.gameObject.GetComponent<MagicianController>();
            if (Player.Id != Id)
            {
                CollisionEntry Entry = new(EntryType: CollisionEntryType.Magician, Id: Player.Id);
                GameManager.Conn.Reducers.AddCollisionEntryMagician(Entry, Identity);
            }
        }

        else if (other.gameObject.CompareTag("Map"))
        {
            MapPiece Map = other.gameObject.GetComponent<MapPiece>();
            CollisionEntry Entry = new(EntryType: CollisionEntryType.Map, Id: Map.Id);
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
                CollisionEntry Entry = new(EntryType: CollisionEntryType.Magician, Id: Player.Id);
                GameManager.Conn.Reducers.RemoveCollisionEntryMagician(Entry, Identity);
            }
        }

        else if (other.gameObject.CompareTag("Map"))
        {
            MapPiece Map = other.gameObject.GetComponent<MapPiece>();
            CollisionEntry Entry = new(EntryType: CollisionEntryType.Map, Id: Map.Id);
            GameManager.Conn.Reducers.RemoveCollisionEntryMagician(Entry, Identity);
        }
    }

    public void Delete(EventContext context)
    {
        Destroy(gameObject);
    }
}
