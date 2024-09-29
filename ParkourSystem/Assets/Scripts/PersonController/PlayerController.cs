using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    [Header("Ground Check Setiings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    CameraController cameraController;
    Quaternion targetRotation;
    Animator animator;
    CharacterController characterController;
    EnviromentScaner enviromentScaner;
    float ySpeed;
    bool hasControl = true;
    public bool inAction { get; private set; }
    public bool isHanging;
    public bool isOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }

    Vector3 desiredMoveDir;
    Vector3 moveDir;
    Vector3 velocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        enviromentScaner = GetComponent<EnviromentScaner>();
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01( Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = (new Vector3(h, 0, v)).normalized;
        Debug.Log("ABCD : MOVE_INPUT : " +moveInput.ToString());
        Debug.Log("ABCD : FORWARD : " + transform.forward);
        desiredMoveDir = cameraController.PlanerRotation * moveInput;
        Debug.Log("ABCD : MOVER_DIR : " + moveInput.ToString());
        moveDir = desiredMoveDir;

        if (!hasControl)
            return;
        if (isHanging)
            return;

         velocity = Vector3.zero;
        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded) {
            ySpeed = -0.5f;
            velocity = desiredMoveDir * moveSpeed;
            isOnLedge = enviromentScaner.LedgeCheck(desiredMoveDir, out LedgeData ledgeData);
            if (isOnLedge) {
                LedgeMovement();
                LedgeData = ledgeData;
            }
            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);
        }
        else{
            ySpeed += Physics.gravity.y * Time.deltaTime;
            velocity = transform.forward * moveSpeed / 2;

        }

        
        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);
        if (moveAmount > 0 && moveDir.magnitude > 0.2f) {

            
            //transform.position += moveDir * moveSpeed * Time.deltaTime;
            targetRotation = Quaternion.LookRotation(moveDir);

            //transform.rotation = 
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            rotationSpeed * Time.deltaTime);

        Debug.Log("PlayerControllerMoveAmount : "+moveAmount);
        
        
    }

    void GroundCheck() {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
        
    }

    void LedgeMovement() {
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDir, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        if (Vector3.Angle(desiredMoveDir, transform.forward) >= 80) {
            velocity = Vector3.zero;
            return;
        }

        if (angle < 60)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }
        else if (angle < 90) {
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            
            var dir = left * Mathf.Sign(signedAngle);
            //Debug.DrawRay(transform.position, dir * 10, Color.red);
            velocity = velocity.magnitude * dir;
            moveDir = dir;

        }
    }


    public IEnumerator DoAction(string AnimName, MatchTargetParams matchTargetParams = null, Quaternion targetRotation = new Quaternion()
        , bool rotate = false,
        float postActionDelay = 0f, bool mirror = false )
    {

        inAction = true;
        
        animator.SetBool("mirrorAction", mirror);
        animator.CrossFadeInFixedTime(AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(AnimName))
            Debug.LogError("Parkour Animation is wrong");

        float rotateStartTime = matchTargetParams != null ? matchTargetParams.startTime : 0f;

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalisedTime = timer / animState.length;
            // Rotate player towards obstacle
            if (rotate && normalisedTime > rotateStartTime)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (matchTargetParams != null)
            {
                MatchTarget(matchTargetParams);
            }

            if (animator.IsInTransition(0) && timer > 0.5f)
                break;

            yield return null;
        }
        yield return new WaitForSeconds(postActionDelay);
        
        inAction = false;
    }

    public void ResetTargetRotation() {
        targetRotation = transform.rotation;
    }

    void MatchTarget(MatchTargetParams mp)
    {
        if (animator.isMatchingTarget) return;
        animator.MatchTarget(mp.pos, transform.rotation, mp.bodypart,
            new MatchTargetWeightMask(mp.posWeight, 0), mp.startTime,
            mp.targetTime);
    }

    public void SetControl(bool hasControl) {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;
        if (!hasControl) {
            animator.SetFloat("moveAmount", 0);
            targetRotation = transform.rotation;
        }
    }

    public void EnableCharacterController(bool enabled)
    {
        characterController.enabled = enabled;
        
    }

    public bool HasControl {
        get => hasControl;
        set => hasControl = value;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
}


public class MatchTargetParams {
    public Vector3 pos;
    public AvatarTarget bodypart;
    public Vector3 posWeight;
    public float startTime;
    public float targetTime;
}
