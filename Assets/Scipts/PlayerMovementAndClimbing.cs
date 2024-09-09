using StarterAssets;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Firebase.Analytics;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementAndClimbing : MonoBehaviour
{
    public FixedJoystick joystick;
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float jumpForce = 5f;

    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Transform groundCheck;
    public Transform wallCheckFront;
    public float checkRadius = 0.2f;
    public float wallCheckRadius = 0.2f;

    public float climbSpeed = 3f;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isClimbing;
    [SerializeField] private bool isFacingWall;

    private Rigidbody rb;

    public Transform leftHand;
    public Transform rightHand;
    private Vector3 leftHandStartPosition;
    private Vector3 rightHandStartPosition;
    public float handStickiness = 0.1f;
    bool isNearWall = false;

    [SerializeField] Animator anim;

    public float rayLength = 5.0f; // Length of the ray
    public LayerMask hitLayers; // Layers that the raycast can hit
    public Transform childObject; // The child object to rotate

    public GameObject bombObject;
    public bool HasBomb;

    [Header("Cinemachine")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;
    public float cameraRotationSpeed = 1f;
    // cinemachine
    [SerializeField] private float _cinemachineTargetYaw;
    [SerializeField] private float _cinemachineTargetPitch;
    [SerializeField] private GameObject _mainCamera;
    [SerializeField] private const float _threshold = 0.01f;
    [SerializeField] private StarterAssetsInputs _input;
    [SerializeField]
    private bool IsCurrentDeviceMouse
    {
        get
        {
#if UNITY_EDITOR
            return false;
#else
            return false;
#endif
        }
    }

    public bool canJump;

    private float fallMultiplier = 2.5f; // Multiplier for when the player is falling
    private float lowJumpMultiplier = 2f; // Multiplier for when the player is jumping but releases the jump button early

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }
    private void Start()
    {
        bombObject.SetActive(GameManager.Instance.isBombTag);
        HasBomb = GameManager.Instance.isBombTag;
    }
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundLayer, QueryTriggerInteraction.Ignore);
        isNearWall = Physics.CheckSphere(wallCheckFront.position, wallCheckRadius, wallLayer, QueryTriggerInteraction.Ignore);

        if (isNearWall /*&& !isGrounded*/)
        {
            if (!isClimbing)
            {
                StartClimbing();
            }
            // Determine if the character is facing the wall
            isFacingWall = IsFacingWall();
        }
        else if (isClimbing)
        {
            StopClimbing();
        }

        if (isClimbing)
        {
            Climb();
        }

        MoveAndRotate();
        ApplyGravity();
        UpdateHandPositions();
    }

    public void JumpBtn()
    {
        if (isGrounded || isClimbing) // Allow jump if grounded or climbing
        {
            canJump = true;
            Jump();

            // Log game session time
            FirebaseAnalytics.LogEvent("panda_jump");
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    public void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * cameraRotationSpeed;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * cameraRotationSpeed;
        }
        else
        {
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private bool IsFacingWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f, wallLayer))
        {
            // Character is facing the wall
            return true;
        }
        // Character is facing away from the wall
        return false;
    }

    void CheckSurroundingsAndRotateChild()
    {
        if (childObject == null)
        {
            Debug.LogError("Child object is not assigned.");
            return;
        }

        // Define the directions to check
        Vector3[] directions = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        foreach (Vector3 dir in directions)
        {
            RaycastHit hit;
            // Perform the raycast from the GameObject's position in the specified direction
            if (Physics.Raycast(transform.position, transform.TransformDirection(dir), out hit, rayLength, hitLayers))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(dir) * hit.distance, Color.yellow);
                Debug.Log("Hit " + hit.collider.name + " at " + hit.point);

                // Rotate the child object to face the hit direction
                // We use Quaternion.LookRotation to make the child face the direction of the raycast hit.
                // The direction needs to be converted from local space to world space.
                childObject.rotation = Quaternion.LookRotation(transform.TransformDirection(dir));

                // Once a hit is found and processed, no need to check further directions
                break;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(dir) * rayLength, Color.white);
            }
        }
    }

    private void StartClimbing()
    {
        anim.SetBool("walk", false);
        anim.SetBool("climb", true);
        isClimbing = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    private void StopClimbing()
    {
        childObject.localRotation = Quaternion.identity;
        isClimbing = false;
        rb.useGravity = true;
        anim.SetBool("climb", false);
    }

    private void Climb()
    {
        if (isFacingWall)
        {
            // Allow climbing up and down
            float inputVertical = joystick.Vertical;

            Vector3 climbingMovement = transform.up * inputVertical * climbSpeed;
            rb.velocity = new Vector3(rb.velocity.x, climbingMovement.y, rb.velocity.z);
        }
        CheckSurroundingsAndRotateChild();
        RotateWhileClimbing();
    }

    private void RotateWhileClimbing()
    {
        //float rotation = joystick.Horizontal * rotationSpeed * Time.deltaTime;
        //transform.Rotate(0, rotation, 0);
    }

    private void MoveAndRotate()
    {
        MoveAndRotateTowardsCameraDirection();

        // Log PandaRunning
        FirebaseAnalytics.LogEvent("panda_running");
    }

    private void MoveAndRotateTowardsCameraDirection()
    {
        float moveHorizontal = joystick.Horizontal;
        float moveVertical = joystick.Vertical;

        // Determine the direction relative to the camera's rotation
        Vector3 cameraForward = CinemachineCameraTarget.transform.forward;
        Vector3 cameraRight = CinemachineCameraTarget.transform.right;

        // Remove any influence of the y component to keep movement horizontal
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate the direction vector based on input and camera orientation
        Vector3 moveDirection = (cameraForward * moveVertical + cameraRight * moveHorizontal).normalized;

        // Move the player in the calculated direction
        if (moveDirection.magnitude > 0.1f)  // Ensuring there's sufficient input to consider
        {
            if (!isClimbing) anim.SetBool("walk", true);
            else
            {
                anim.SetBool("walk", false);
            }
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            if (!isClimbing || !isFacingWall) rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("walk", false);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    private void ApplyGravity()
    {
        if (!isGrounded && !isClimbing)
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rb.velocity.y > 0 && !canJump)
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
    }

    private void UpdateHandPositions()
    {
        if (isNearWall && !isGrounded)
        {
            RaycastHit hit;
            // Update left hand position if it hits a climbable wall
            if (Physics.Raycast(leftHand.position, -leftHand.up, out hit, 0.5f, wallLayer))
            {
                leftHand.position = Vector3.Lerp(leftHand.position, hit.point + leftHand.up * handStickiness, Time.deltaTime * 10);
            }
            else
            {
                leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, leftHandStartPosition, Time.deltaTime * 10);
            }

            // Update right hand position similarly
            if (Physics.Raycast(rightHand.position, -rightHand.up, out hit, 0.5f, wallLayer))
            {
                rightHand.position = Vector3.Lerp(rightHand.position, hit.point + rightHand.up * handStickiness, Time.deltaTime * 10);
            }
            else
            {
                rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rightHandStartPosition, Time.deltaTime * 10);
            }
        }
        else
        {
            // Reset hands to their starting positions when not climbing
            leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, leftHandStartPosition, Time.deltaTime * 10);
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rightHandStartPosition, Time.deltaTime * 10);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw Gizmos for Ground Check
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);

        // Draw Gizmos for Wall Check
        bool isNearWallPreview = Physics.CheckSphere(wallCheckFront.position, wallCheckRadius, wallLayer, QueryTriggerInteraction.Ignore);
        Gizmos.color = isNearWallPreview ? Color.blue : Color.yellow;
        Gizmos.DrawWireSphere(wallCheckFront.position, wallCheckRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AI")
        {
            if (GameManager.Instance.isBombTag)
            {
                if (HasBomb)
                {
                    other.gameObject.GetComponent<AIController>().hasBomb = true;
                    other.gameObject.GetComponent<AIController>().bombObject.SetActive(true);
                    HasBomb = false;
                    bombObject.SetActive(false);
                }
                else
                {
                    if (other.gameObject.GetComponent<AIController>().hasBomb)
                    {
                        other.gameObject.GetComponent<AIController>().hasBomb = false;
                        other.gameObject.GetComponent<AIController>().bombObject.SetActive(false);
                        HasBomb = true;
                        bombObject.SetActive(true);
                    }
                }
            }
            else if (GameManager.Instance.isRunner)
            {
                if (other.gameObject.GetComponent<AIController>().isHunter)
                {
                    GamePlayHandler.Instance.runnerLives--;
                    if (GamePlayHandler.Instance.runnerLives < 0)
                    {
                        if (!GamePlayHandler.Instance.winPanel.activeSelf) GamePlayHandler.Instance.failPanel.SetActive(true);
                    }
                    else
                    {
                        GamePlayHandler.Instance.UpdateRunnerLives();
                    }
                }
            }
            else
            {
                if (!other.gameObject.GetComponent<AIController>().isHunter)
                {
                    Destroy(Instantiate(GamePlayHandler.Instance.AIKillparticle, other.transform.position, Quaternion.identity), 3f);
                    GamePlayHandler.Instance.AI_Active.Remove(other.gameObject);
                    Destroy(other.gameObject);
                    GamePlayHandler.Instance.AI_Active.RemoveAll(x => x == null);
                    GamePlayHandler.Instance.UpdateHuntersVsRunnersCount();
                    if (GamePlayHandler.Instance.AI_Active.Count == 0 && !GamePlayHandler.Instance.failPanel.activeSelf)
                    {
                        GamePlayHandler.Instance.winPanel.SetActive(true);
                    }
                }
            }
        }
    }
}
