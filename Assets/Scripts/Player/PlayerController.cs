using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 move;

    [Header("Speed Settings")]
    public float forwardSpeed = 19f;
    public float maxSpeed = 50f;

    [Header("Lane Settings")]
    private int desiredLane = 1;
    public float laneDistance = 2.5f;
    public float laneSwitchSpeed = 30f;

    [Header("Ground Check")]
    public bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundCheck;

    [Header("Physics")]
    public float gravity = -12f;
    public float jumpHeight = 2;
    private Vector3 velocity;

    [Header("Animation")]
    public Animator animator;

    [Header("Slide Settings")]
    private bool isSliding = false;
    public float slideDuration = 1.5f;

    [Header("Shield")]
    public GameObject shieldVisual;

    // Death
    private bool isDying = false;
    private float deathDecelerationRate = 15f;
    private Vector3 deathKnockback = Vector3.zero;

    // Speed progression
    private bool toggle = false;

    // Cached references
    private AudioManager cachedAudio;
    private ScreenFlash cachedScreenFlash;
    private TimeManager cachedTimeManager;
    private CameraController cachedCamera;
    private ObjectPool cachedPool;
    private Coroutine slideCoroutine;

    // Pre-allocated vectors
    private Vector3 targetPosition;
    private Vector3 diff;
    private Vector3 moveDir;
    private Vector3 safePos;

    void Awake()
    {
        if (groundCheck == null)
        {
            Transform check = transform.Find("groundCheck");
            if (check != null)
                groundCheck = check;
        }
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Don't overwrite forwardSpeed - use whatever is set in inspector
        Time.timeScale = 1f;

        // Cache references
        cachedAudio = AudioManager.instance;
        cachedScreenFlash = FindObjectOfType<ScreenFlash>();
        cachedTimeManager = TimeManager.instance;
        cachedCamera = FindObjectOfType<CameraController>();
        cachedPool = ObjectPool.instance;
    }

    private void FixedUpdate()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
            return;

        if (toggle)
        {
            toggle = false;
            if (forwardSpeed < maxSpeed)
                forwardSpeed += 0.1f * Time.fixedDeltaTime;
        }
        else
        {
            toggle = true;

        }
    }

    void Update()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
            return;

        if (isDying)
        {
            HandleDeathMovement();
            return;
        }

        animator.SetBool("isGameStarted", true);
        move.z = forwardSpeed;

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.85f, groundLayer);

        // Floor safety
        if (transform.position.y < 0.5f)
        {
            safePos.x = transform.position.x;
            safePos.y = 1f;
            safePos.z = transform.position.z;
            transform.position = safePos;
            velocity.y = -1f;
            isGrounded = true;
        }

        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded && velocity.y < 0)
            velocity.y = -1f;

        if (isGrounded)
        {
            if (SwipeManager.swipeUp)
                Jump();
            if (SwipeManager.swipeDown && !isSliding)
                slideCoroutine = StartCoroutine(Slide());
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            if (SwipeManager.swipeDown && !isSliding)
            {
                StartCoroutine(Slide());
                velocity.y = -10;
            }
        }

        controller.Move(velocity * Time.deltaTime);

        // Lane switching
        if (SwipeManager.swipeRight)
        {
            desiredLane++;
            if (desiredLane == 3)
                desiredLane = 2;
        }
        if (SwipeManager.swipeLeft)
        {
            desiredLane--;
            if (desiredLane == -1)
                desiredLane = 0;
        }

        // Calculate target position
        targetPosition.x = 0;
        targetPosition.y = transform.position.y;
        targetPosition.z = transform.position.z;

        if (desiredLane == 0)
            targetPosition.x = -laneDistance;
        else if (desiredLane == 2)
            targetPosition.x = laneDistance;

        if (transform.position.x != targetPosition.x)
        {
            diff.x = targetPosition.x - transform.position.x;
            diff.y = 0;
            diff.z = 0;

            float moveMagnitude = laneSwitchSpeed * Time.deltaTime;

            if (moveMagnitude < Mathf.Abs(diff.x))
            {
                moveDir.x = Mathf.Sign(diff.x) * moveMagnitude;
                moveDir.y = 0;
                moveDir.z = 0;
                controller.Move(moveDir);
            }
            else
            {
                controller.Move(diff);
            }
        }

        controller.Move(move * Time.deltaTime);

        if (shieldVisual != null)
            shieldVisual.SetActive(PlayerManager.shieldActive);
    }

    private void HandleDeathMovement()
    {
        forwardSpeed = Mathf.Lerp(forwardSpeed, 0, deathDecelerationRate * Time.deltaTime);
        move.z = forwardSpeed;

        deathKnockback = Vector3.Lerp(deathKnockback, Vector3.zero, 8f * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;

        controller.Move((move + deathKnockback) * Time.deltaTime);
        controller.Move(velocity * Time.deltaTime);

        if (forwardSpeed < 0.1f && !PlayerManager.gameOver)
            PlayerManager.gameOver = true;
    }

    private void Jump()
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        animator.SetBool("isSliding", false);
        animator.SetTrigger("jump");
        controller.center = Vector3.zero;
        controller.height = 2;
        isSliding = false;
        velocity.y = Mathf.Sqrt(jumpHeight * 2 * -gravity);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag("Obstacle"))
        {
            if (PlayerManager.shieldActive)
            {
                hit.gameObject.SetActive(false);
                if (cachedAudio != null)
                    cachedAudio.PlaySound("ShieldHit");
                StartCoroutine(ReenableCollision(hit.collider, 0.1f));
            }
            else
            {
                StartCoroutine(DeathSequence(hit));
            }
        }
    }

    private IEnumerator DeathSequence(ControllerColliderHit hit)
    {
        if (isDying) yield break;
        isDying = true;

        Vector3 impactDirection = (transform.position - hit.point).normalized;
        impactDirection.y = 0;
        deathKnockback = impactDirection * 3f;

        if (animator != null)
        {
            animator.SetTrigger("death");
            animator.SetBool("isGameStarted", false);
        }

        if (cachedScreenFlash != null)
            cachedScreenFlash.FlashGameOver();

        if (cachedTimeManager != null)
            cachedTimeManager.SlowMotion(0.3f, 0.7f);

        yield return new WaitForSecondsRealtime(0.1f);

        if (cachedAudio != null)
            cachedAudio.PlaySound("GameOver");

        SpawnImpactEffect(hit.point);

        if (cachedCamera != null)
            cachedCamera.Shake(0.4f, 0.25f);
    }

    private void SpawnImpactEffect(Vector3 position)
    {
        if (cachedPool == null) return;

        GameObject effect = cachedPool.GetPooledObject();
        if (effect != null)
        {
            effect.transform.position = position;
            effect.SetActive(true);
        }
    }

    private IEnumerator ReenableCollision(Collider obstacle, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obstacle != null)
            Physics.IgnoreCollision(controller, obstacle, false);
    }

    private IEnumerator Slide()
    {
        isSliding = true;
        animator.SetBool("isSliding", true);

        yield return new WaitForSeconds(0.25f / Time.timeScale);

        controller.center = new Vector3(0, -0.5f, 0);
        controller.height = 1;

        yield return new WaitForSeconds((slideDuration - 0.25f) / Time.timeScale);

        animator.SetBool("isSliding", false);
        controller.center = Vector3.zero;
        controller.height = 2;
        isSliding = false;
        slideCoroutine = null;
    }
}