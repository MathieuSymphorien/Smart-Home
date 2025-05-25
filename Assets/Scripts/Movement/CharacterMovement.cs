using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour 
{
    [Header("Références")]
    public Transform cam;  

    [Header("Paramètres")]
    public float speed = 6f;
    public float rotationSmoothTime = 0.1f;

    private CharacterController controller;
    private Animator animator;
    private float rotationSmoothVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator   = GetComponent<Animator>();

        if (cam == null)
        {
            cam = Camera.main?.transform;
            if (cam == null)
                Debug.LogWarning("Caméra non assignée !");
        }
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        /* ---------- 1. Lecture des axes clavier/manette ---------- */
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        // Animation (vitesse entre 0 et 1)
        animator.SetFloat("Speed", inputDir.magnitude);

        if (inputDir.sqrMagnitude < 0.01f)             // Rien à faire si aucune entrée
            return;

        /* ---------- 2. Repère caméra → repère monde ---------- */
        // On ignore l’inclinaison (pitch) de la caméra pour éviter que le perso ne lève/baisse la tête.
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1f, 0f, 1f)).normalized;
        Vector3 camRight   = Vector3.Scale(cam.right,   new Vector3(1f, 0f, 1f)).normalized;

        Vector3 moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

        /* ---------- 3. Rotation du personnage ---------- */
        float targetAngle   = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
        float smoothAngle   = Mathf.SmoothDampAngle(transform.eulerAngles.y,
                                                    targetAngle,
                                                    ref rotationSmoothVelocity,
                                                    rotationSmoothTime);
        transform.rotation  = Quaternion.Euler(0f, smoothAngle, 0f);

        /* ---------- 4. Déplacement ---------- */
        controller.Move(moveDir * speed * Time.deltaTime);
    }
}
