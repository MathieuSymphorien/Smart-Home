using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovment : MonoBehaviour
{
    public Transform cam;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;

    private CharacterController controller;
    private Animator animator;
    private float turnSmoothVelocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // récupère l'Animator

        if (cam == null)
        {
            cam = Camera.main?.transform;
            if (cam == null)
                Debug.LogWarning("Caméra non assignée !");
        }
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Met à jour le paramètre Speed de l'Animator
        float speedParam = direction.magnitude;
        animator.SetFloat("Speed", speedParam);


        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, 
                                                 ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
    }
}
