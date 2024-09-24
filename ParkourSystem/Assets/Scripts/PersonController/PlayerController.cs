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

    public void SetControl(bool hasControl) {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;
        if (!hasControl) {
            animator.SetFloat("moveAmount", 0);
            targetRotation = transform.rotation;
        }
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
