using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class playerController : MonoBehaviour
{
    public ServerData playerData;
    public GameObject player;
    public Rigidbody rb;
    public bool recordPosition = true;
    public bool canMove;
    public float speed = 50f;
    public float rotMult = 4f;
    public float maxY = -65f;
    public float minY = 50f;
    public float bounds;
    public bool useController;

    private float minMapY;
    private string curScene;
    private float yaw = 0f;
    private float pitch = 0f;
    private float sprintSpeed;

    public float gravity = 20f;
    public float jumpForce = 30f;
    private CharacterController controller;
    private Vector3 moveDirection;
    private Vector3 lastPos;
    private Vector3 move;
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        canMove = false;
        recordPosition = false;
        useController = false;
        sprintSpeed = speed * 2f;
        player = GameObject.FindGameObjectWithTag("Player");
        controller = player.GetComponent<CharacterController>();
        rb = player.GetComponent<Rigidbody>();
        bounds = player.GetComponent<Collider>().bounds.extents.y;
        if (GameObject.FindGameObjectsWithTag("MainCamera").Length == 1)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if(playerData.curScene == "CCE Experiment")
        {
            StartCoroutine("moveAfterXSeconds");
        }
        else
        {
            canMove = true;
        }
    }

    IEnumerator moveAfterXSeconds()
    {
        yield return new WaitForSeconds(3);
        canMove = true;
    }

    void Update()
    {
        if (canMove)
        {
            // Player Movement
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            move = player.transform.right * horizontalInput + player.transform.forward * verticalInput;
            //controller.Move(move * speed * Time.deltaTime);

            if (useController)
            {
                // Xbox controller camera control
                yaw += rotMult * Input.GetAxis("Con X"); // Adjust "RightStickX" according to your input settings
                pitch -= rotMult * Input.GetAxis("Con Y"); // Adjust "RightStickY" according to your input settings

            }
            else
            {
                yaw += rotMult * Input.GetAxis("Mouse X");
                pitch -= rotMult * Input.GetAxis("Mouse Y");
            }

            pitch = Mathf.Clamp(pitch, maxY, minY); // Clamp viewing up and down
            player.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            // Applying Gravity
            if (!controller.isGrounded)
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }

            // Jumping
            if (controller.isGrounded && Input.GetButtonDown("Jump"))
            {
                //moveDirection += new Vector3(moveDirection.x, jumpForce, moveDirection.z);
                moveDirection.y = jumpForce;
            }

            controller.Move(speed * (moveDirection + move) * Time.deltaTime);

        }
    }

    private bool isGrounded()
    {
        return Physics.Raycast(player.transform.position, -Vector3.up, bounds + 0.1f);
    }

    public void SetActive(bool setting)
    {
        player.gameObject.SetActive(setting);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }

    public GameObject GetCurrentPlayer()
    {
        return player;
    }

    public void SetCanMove(bool setting)
    {
        canMove = setting;
    }
}
