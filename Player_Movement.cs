using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour
{

    public Rigidbody rb;
    public CapsuleCollider Collider;

    public float speed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private Input_Controls player_controls;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;

    private int currentIndex = 0;

    public Transform groundCheck;
    public float groundDistance = 0.9f;
    public LayerMask groundMask;

    private Animator anim;
    private GameObject activeHologram;

    public GameObject[] POS_manager;
    public GameObject Hologram;

    private float fallTimer = 0f;
    private float fallTimeout = 3f;
    private bool isFallingTimerActive = false;

    public int BoxCount;

    public Transform origin;
    public Transform target;

    private bool isHoloActive = false;

    public bool flag = true;

    public GameObject gameoverUI;

    private void Awake()
    {
        player_controls = new Input_Controls();
        anim = GetComponentInChildren<Animator>();

        //player movement
        player_controls.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        player_controls.Player.Movement.canceled += ctx => moveInput = Vector2.zero;

        //ui movement
        player_controls.Player.Holo_UI.performed += ctx => ToggleHoloUI();
        player_controls.Player.Holo_scroll.performed += ctx => OnScroll(ctx.ReadValue<float>());

        player_controls.Player.Change_Gravity.performed += ctx => ChangeGravity();

        rb.freezeRotation = true;

    }

    private void OnEnable()
    {
        player_controls.Enable();
    }

    private void OnDisable()
    {
        player_controls.Disable();
    }

    void Update()
    {
        if (isHoloActive)
            return;

        Vector3 gravityUp = transform.up;

        Vector3 directionForGravity = (origin.position - target.position).normalized;
        Vector3 gravityForce = directionForGravity * gravity;
        rb.AddForce(gravityForce, ForceMode.Acceleration);


        //check if grounded 

        isGrounded = Physics.Raycast(groundCheck.position, -gravityUp, out RaycastHit hit, groundDistance, groundMask);
        Debug.DrawRay(groundCheck.position, -gravityUp * groundDistance, Color.red);

        if (isGrounded && Vector3.Dot(velocity, gravityUp) > 0)
            velocity = -gravityUp * 2f;


        if (!isGrounded)
        {
            if (!isFallingTimerActive)
            {
                fallTimer = 0f;
                isFallingTimerActive = true;
            }

            fallTimer += Time.deltaTime;

            if (fallTimer >= fallTimeout)
            {
                EndGame();
            }
        }
        else
        {
            isFallingTimerActive = false;
            fallTimer = 0f;
        }

        //player move as camera view

        Vector3 cameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, gravityUp).normalized;
            Vector3 cameraRight = Vector3.Cross(gravityUp, cameraForward);

            Vector3 move = cameraRight * moveInput.x + cameraForward * moveInput.y;
            Vector3 newPosition = rb.position + move * speed * Time.deltaTime;
            rb.MovePosition(newPosition);


            //jump

            if (player_controls.Player.Jump.triggered && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(gravityUp * jumpHeight, ForceMode.VelocityChange);

                anim.SetTrigger("Jump");
            }


            //animation

            bool isMoving = moveInput.magnitude > 0.1f;
            anim.SetBool("IsRunning", isMoving);
            bool isFalling = !isGrounded;
            anim.SetBool("IsFalling", isFalling);

    }
    
   


        private void ToggleHoloUI()
        {
            if (activeHologram == null)
            {
                activeHologram = Instantiate(Hologram, POS_manager[currentIndex].transform.position, POS_manager[currentIndex].transform.rotation);
                isHoloActive = true;
            }
            else
            {
                Destroy(activeHologram);
                activeHologram = null;
                isHoloActive = false;
            }
        }


        private void OnScroll(float scrollValue)
        {
            if (activeHologram == null || scrollValue == 0)
                return;

            // Change index depending on scroll direction
            if (scrollValue > 0)
                currentIndex = (currentIndex + 1) % POS_manager.Length;
            else
                currentIndex = (currentIndex - 1 + POS_manager.Length) % POS_manager.Length;

            // Replace hologram
            Destroy(activeHologram);
            activeHologram = Instantiate(Hologram, POS_manager[currentIndex].transform.position, POS_manager[currentIndex].transform.rotation);
        }


        private void ChangeGravity()
        {
            if (!isHoloActive || activeHologram == null)
                return;

            transform.position = activeHologram.transform.position;
            transform.rotation = activeHologram.transform.rotation;

            Destroy(activeHologram);
            activeHologram = null;
            isHoloActive = false;

            velocity = Vector3.zero;
            moveInput = Vector2.zero;
        }


        public void EndGame()
        {
            rb.isKinematic = true;
            player_controls.Disable();
            Time.timeScale = 0f;
            Debug.Log("gameover");
            flag = false;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Box"))
            {
                BoxCount++;
                Destroy(other.gameObject);
            }
        }
    
}

