using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Paramètres de rotation")]
    public float rotationAngleX = 45f;    // Inclinaison fixe vers le sol
    public float rotationSpeedY = 100f;   // Vitesse de rotation autour de Y (Q/E)

    [Header("Paramètres de déplacement")]
    public float moveSpeed = 10f;         // Vitesse de déplacement (W/S/A/D)

    [Header("Paramètres de zoom")]
    public float zoomSpeed = 20f;         // Vitesse de zoom (molette souris)

    // Rotation courante autour de l'axe Y
    private float currentYRotation = 0f;

    private void Start()
    {
        // On fixe l'inclinaison initiale à rotationAngleX, la rotation Y à 0
        // (ou un autre angle si vous voulez un Y initial différent)
        currentYRotation = transform.eulerAngles.y;
        ApplyRotation();
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
        HandleZoom();
    }

    /// <summary>
    /// Gère la rotation autour de l'axe Y via Q/E
    /// </summary>
    private void HandleRotation()
    {
        // Q => tourne à gauche ; E => tourne à droite
        if (Input.GetKey(KeyCode.Q))
            currentYRotation -= rotationSpeedY * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            currentYRotation += rotationSpeedY * Time.deltaTime;

        // Applique la rotation x=45°, y=currentYRotation, z=0
        ApplyRotation();
    }

    /// <summary>
    /// Applique la rotation X/Y calculée
    /// </summary>
    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(rotationAngleX, currentYRotation, 0f);
    }

    /// <summary>
    /// Gère les déplacements W/S/A/D dans le plan, mais en suivant l'orientation Y de la caméra
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D ou flèches gauche/droite
        float vertical   = Input.GetAxis("Vertical");   // W/S ou flèches haut/bas

        // Déterminer la direction "avant" de la caméra, ignorante du pitch pour ne pas bouger en Y
        Vector3 forward = transform.forward;
        forward.y = 0; // on ignore la composante verticale
        forward.Normalize();

        // Pareil pour la direction "droite"
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        // Calcul du vecteur de déplacement (dans le plan XZ selon la direction de la caméra)
        Vector3 moveDirection = forward * vertical + right * horizontal;
        moveDirection.Normalize(); // si on veut éviter d’aller plus vite en diagonale

        // Déplacement
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Zoom / Dézoom en avançant/reculant la caméra (axe local Z) avec la molette
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // molette souris
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // On avance/recule la caméra dans sa direction locale (incluse dans l’angle X=45°)
            transform.Translate(Vector3.forward * scroll * zoomSpeed, Space.Self);
        }
    }
}
