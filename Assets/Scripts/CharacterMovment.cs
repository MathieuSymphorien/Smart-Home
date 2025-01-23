using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovment : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;      // Movement speed
    public float turnSpeed = 150f;    // Rotation speed
    public float jumpForce = 5f;      // Force applied when jumping

    private CharacterController characterController;
    private Vector3 velocity;
    private float gravity = -9.81f;

    void Start()
    {
        // Get the CharacterController component on the same object
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Get input for movement
        float horizontal = Input.GetAxis("Horizontal");   // A/D or Left/Right arrow by default
        float vertical   = Input.GetAxis("Vertical");     // W/S or Up/Down arrow by default

        // Calculate movement relative to the object's forward direction
        Vector3 move = transform.forward * vertical * moveSpeed;

        // Move the capsule on the horizontal/vertical plane
        characterController.Move(move * Time.deltaTime);

        // Rotate the capsule around the Y-axis
        transform.Rotate(0f, horizontal * turnSpeed * Time.deltaTime, 0f);

        // Check if grounded
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }

        // Jump
        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            velocity.y = jumpForce;
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Move the capsule vertically
        characterController.Move(velocity * Time.deltaTime);
    }
}
